using System;
using System.Collections.Generic;
using Engine.Models;
using Engine.Vertices;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace Engine.Buffers
{
    public abstract class ModelBuffer : IDisposable
    {
        public PrimitiveTopology Topology { get; protected set; }
        public Format IndexFormat { get; protected set; }

        public void Prepare(DeviceContext context)
        {
            context.InputAssembler.PrimitiveTopology = Topology;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(_vertices, VertexSize, 0));
            context.InputAssembler.SetIndexBuffer(_indices, IndexFormat, 0);
        }

        protected abstract int VertexSize { get; }

        protected void Init(Buffer vertices, Buffer indices)
        {
            _vertices = vertices;
            _indices = indices;
        }

        private Buffer _vertices;
        private Buffer _indices;

        #region IDisposable

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            _vertices.Dispose();
            _indices.Dispose();
        }

        private bool _disposed;
        private readonly object _disposeLock = new object();

        #endregion
    }

    public sealed class ModelBuffer<TVertex, TIndex> : ModelBuffer
        where TVertex : struct, IVertex
        where TIndex : struct
    {
        public ModelBuffer(PrimitiveTopology topology, Format indexFormat, Device device, IReadOnlyList<Model<TVertex, TIndex>> models)
        {
            var vl = new List<TVertex>();
            var il = new List<TIndex>();
            
            Topology = topology;
            IndexFormat = indexFormat;
            
            for (var i = 0; i < models.Count; i++)
            {
                var model = models[i];
                model.RegisterToBuffer(this, i, vl.Count, il.Count);

                vl.AddRange(model.Vertices);
                il.AddRange(model.Indices);
            }

            var vb = Buffer.Create(device, BindFlags.VertexBuffer, vl.ToArray());
            var ib = Buffer.Create(device, BindFlags.VertexBuffer, il.ToArray());

            Init(vb, ib);
        }

        protected override int VertexSize { get; } = SharpDX.Utilities.SizeOf<TVertex>();
    }
}