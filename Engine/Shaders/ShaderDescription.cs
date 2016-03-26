using System;
using System.Collections.Generic;
using Engine.Utilities;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace Engine.Shaders
{
    public abstract class ShaderDescription : IDisposable
    {
        public int Id { get; set; } = -1;
        public ShadersController Controller { get; set; }
        public Buffer PerObjBuffer { get; private set; }
        public ValueType PerFrameBufferContent { get; set; }

        #region Constant buffers

        public Buffer AddConstantBuffer<TBuffer>(ShaderBufferType bufferType = ShaderBufferType.PerFrame)
            where TBuffer : struct
        {
            var buffer = new Buffer(_device, SharpDX.Utilities.SizeOf<TBuffer>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            _toDispose.Add(buffer);
            _constantBuffers.Add(buffer);
            BufferAdded(buffer, bufferType);
            return buffer;
        }

        public Buffer AddConstantBuffer(int size, ShaderBufferType bufferType = ShaderBufferType.PerFrame)
        {
            var buffer = new Buffer(_device, size, ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            _toDispose.Add(buffer);
            _constantBuffers.Add(buffer);
            BufferAdded(buffer, bufferType);
            return buffer;
        }

        public void AddConstantBuffer(Buffer buffer, ShaderBufferType bufferType = ShaderBufferType.PerFrame)
        {
            _constantBuffers.Add(buffer);
            BufferAdded(buffer, bufferType);
        }

        #endregion

        #region Resources

        public void SetResourses(DeviceContext context, params ShaderResourceView[] resources)
        {
            var stage = GetShaderStage(context);
            stage.SetShaderResources(0, resources);
        } 

        #endregion

        public void Select(DeviceContext context)
        {
            if (Check())
                return;

            SelectOverride(context);

            var stage = GetShaderStage(context);
            var cbs = _constantBuffers.ToArray();
            stage.SetConstantBuffers(0, cbs);

            UpdatePerFrameBuffer(context);
        }

        protected ShaderBytecode Bytecode { get; }
        protected InputLayout Layout { get; private set; }
        protected Buffer PerFrameBuffer { get; private set; }

        protected abstract CommonShaderStage GetShaderStage(DeviceContext context);

        protected abstract void SelectOverride(DeviceContext context);
        protected abstract bool Check();

        protected virtual void UpdatePerFrameBuffer(DeviceContext context) {}

        protected bool CheckPixelShader()
            => Controller != null && Controller.CheckPixelShader(Id);

        protected bool CheckVertexShader()
            => Controller != null && Controller.CheckVertexShader(Id);

        protected ShaderDescription(Device device, string path, string entryPoint, string profile, ShaderFlags flags)
        {
            using (var include = new IncludeHelper())
            {
                using (var compilationResult = ShaderBytecode.CompileFromFile(path, entryPoint, profile, flags, include: include))
                {
                    if (compilationResult.Bytecode == null)
                        throw new Exception(compilationResult.Message);

                    Bytecode = compilationResult.Bytecode;
                    _toDispose.Add(Bytecode);
                }
            }

            _device = device;
        }

        protected void PrepareInputLayout(Device device, InputElement[] elements)
        {
            lock (_layoutLock)
            {
                if (Layout != null) return;

                Layout = new InputLayout(device, Bytecode, elements);
                _toDispose.Add(Layout);
            }
        }

        private readonly List<IDisposable> _toDispose = new List<IDisposable>();
        private readonly List<Buffer> _constantBuffers = new List<Buffer>();
        private readonly object _layoutLock = new object();
        private readonly Device _device;

        private void BufferAdded(Buffer buffer, ShaderBufferType bufferType)
        {
            switch (bufferType)
            {
                case ShaderBufferType.PerObject:
                    PerObjBuffer = buffer;
                    break;
                case ShaderBufferType.PerFrame:
                    PerFrameBuffer = buffer;
                    break;
                case ShaderBufferType.Other:
                    throw new NotImplementedException("ShaderBufferType.Other not implemented yet.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(bufferType), bufferType, null);
            }
        }

        #region IDisposable

        public void Dispose()
        {
            lock (_disposeLock)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            DisposeCore();
            foreach (var disposable in _toDispose)
                disposable.Dispose();
        }

        protected virtual void DisposeCore()
        {
        }

        private bool _disposed;
        private readonly object _disposeLock = new object();

        #endregion
    }
}