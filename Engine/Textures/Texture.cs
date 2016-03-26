using System;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Textures
{
    public abstract class Texture : IDisposable
    {
        public ShaderResourceView ShaderResourceView { get; protected set; }

        public abstract Matrix TexTransform { get; }

        #region IDisposable

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_disposed)
                    return;
                _disposed = true;
            }

            DisposeOverride();
            ShaderResourceView.Dispose();
        }

        protected abstract void DisposeOverride();

        private bool _disposed;
        private readonly object _disposeLock = new object();

        #endregion
    }
}