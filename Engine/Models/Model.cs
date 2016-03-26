using System;
using System.Collections.Generic;
using Engine.Buffers;
using Engine.Vertices;
using SharpDX.Direct3D11;

namespace Engine.Models
{
    public abstract class Model
    {
        public bool RegisteredToBuffer { get; private set; }
        public int VertexOffset { get; private set; }
        public int IndexOffset { get; private set; }
        public abstract int IndexCount { get; }

        internal void RegisterToBuffer(ModelBuffer buffer, int id, int vertexOffset, int indexOffset)
        {
            lock (_lock)
            {
                if (RegisteredToBuffer)
                    throw new InvalidOperationException("Model has already been registered to buffer");

                RegisteredToBuffer = true;
            }

            _buffer = buffer;
            VertexOffset = vertexOffset;
            IndexOffset = indexOffset;
        }

        internal void UnregisterFromBuffer(ModelBuffer buffer)
        {
            lock (_lock)
            {
                if (!RegisteredToBuffer)
                    throw new InvalidOperationException("Model has not been registered to buffer");

                RegisteredToBuffer = false;
            }

            _buffer = null;
            VertexOffset = -1;
            IndexOffset = -1;
        }

        internal void Draw(DeviceContext context)
        {
            _buffer.Prepare(context);
            context.DrawIndexed(IndexCount, IndexOffset, VertexOffset);
        }

        private ModelBuffer _buffer;
        private readonly object _lock = new object();
    }

    public class Model<TVertex, TIndex> : Model
        where TVertex : struct, IVertex
        where TIndex : struct
    {
        public List<TVertex> Vertices { get; set; }
        public List<TIndex> Indices { get; set; }
        public override int IndexCount => Indices.Count;

        public Model()
        {
            Vertices = new List<TVertex>();
            Indices = new List<TIndex>();
        } 

        public Model(List<TVertex> vertices, List<TIndex> indices)
        {
            Vertices = vertices;
            Indices = indices;
        }
    }
}