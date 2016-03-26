using System;
using System.Collections.Generic;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace Engine.Shaders
{
    public class ShadersController : IDisposable
    {
        public ShadersController(Device device)
        {
            _device = device;
        }

        public PixelShaderDescription AddPixelShader(string path, string entryPoint, string profile, ShaderFlags flags)
        {
            var shader = new PixelShaderDescription(_device, path, entryPoint, profile, flags)
            {
                Controller = this,
                Id = _shaders.Count,
            };

            _shaders.Add(shader);
            return shader;
        }

        public PixelShaderDescription AddPixelShader<TPerFrameBuffer>(string path, string entryPoint, string profile, ShaderFlags flags)
            where TPerFrameBuffer : struct
        {
            var shader = new PixelShaderDescription<TPerFrameBuffer>(_device, path, entryPoint, profile, flags)
            {
                Controller = this,
                Id = _shaders.Count,
            };

            _shaders.Add(shader);
            return shader;
        }

        public VertexShaderDescription AddVertexShader(InputElement[] inputElements, string path, string entryPoint, string profile, ShaderFlags flags)
        {
            var shader = new VertexShaderDescription(_device, inputElements, path, entryPoint, profile, flags)
            {
                Controller = this,
                Id = _shaders.Count,
            };

            _shaders.Add(shader);
            return shader;
        }

        public VertexShaderDescription AddVertexShader<TPerFrameBuffer>(InputElement[] inputElements, string path, string entryPoint, string profile, ShaderFlags flags)
            where TPerFrameBuffer : struct
        {
            var shader = new VertexShaderDescription<TPerFrameBuffer>(_device, inputElements, path, entryPoint, profile, flags)
            {
                Controller = this,
                Id = _shaders.Count,
            };

            _shaders.Add(shader);
            return shader;
        }

        public bool CheckPixelShader(int id)
        {
            if (id == _selectedPixelShader)
                return true;

            _selectedPixelShader = id;
            return false;
        }

        public bool CheckVertexShader(int id)
        {
            if (id == _selectedVertexShader)
                return true;

            _selectedVertexShader = id;
            return false;
        }

        private readonly List<ShaderDescription> _shaders = new List<ShaderDescription>();
        private readonly Device _device;
        private int _selectedVertexShader = -1;
        private int _selectedPixelShader = -1;

        #region IDisposable

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            foreach (var shader in _shaders)
                shader.Dispose();
        }

        private bool _disposed;
        private readonly object _disposeLock = new object();

        #endregion
    }
}