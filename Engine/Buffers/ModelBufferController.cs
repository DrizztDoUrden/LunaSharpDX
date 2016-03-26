using System;
using System.Collections.Generic;
using Engine.Models;
using Engine.Vertices;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace Engine.Buffers
{
    public class ModelBufferController : IDisposable
    {
        public ModelBufferController(Device device)
        {
            _device = device;
            _buffers = new List<ModelBuffer>();
        }

        public ModelBufferController(Device device, int capacity)
        {
            _device = device;
            _buffers = new List<ModelBuffer>(capacity);
        }

        public int AddBuffer<TVertex, TIndex>(PrimitiveTopology topology, Format indexFormat, Model<TVertex, TIndex>[] models)
            where TVertex : struct, IVertex
            where TIndex : struct
        {
            var buf = new ModelBuffer<TVertex, TIndex>(topology, indexFormat, _device, models);
            var id = _buffers.Count;

            _buffers.Add(buf);
            return id;
        }

        public void RemoveBuffer(int id)
        {
            _buffers.RemoveAt(id);
        }
        
        private readonly Device _device;
        private readonly List<ModelBuffer> _buffers;

        #region IDisposable

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }
            
            foreach (var buf in _buffers)
                buf.Dispose();
        }

        private bool _disposed;
        private readonly object _disposeLock = new object();

        #endregion
    }
}