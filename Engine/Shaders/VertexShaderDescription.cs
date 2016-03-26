using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace Engine.Shaders
{
    public class VertexShaderDescription : ShaderDescription
    {
        public VertexShaderDescription(Device device, InputElement[] inputElements, string path, string entryPoint, string profile, ShaderFlags flags) : base(device, path, entryPoint, profile, flags)
        {
            _shader = new VertexShader(device, Bytecode);
            PrepareInputLayout(device, inputElements);
        }

        protected sealed override void SelectOverride(DeviceContext context)
        {
            context.InputAssembler.InputLayout = Layout;
            context.VertexShader.Set(_shader);
        }

        protected sealed override CommonShaderStage GetShaderStage(DeviceContext context)
            => context.VertexShader;

        protected sealed override bool Check()
            => CheckVertexShader();

        protected override void DisposeCore()
        {
            _shader?.Dispose();
        }

        private readonly VertexShader _shader;
    }

    public class VertexShaderDescription<TPerFrameBuffer> : VertexShaderDescription
        where TPerFrameBuffer : struct
    {
        public VertexShaderDescription(Device device, InputElement[] inputElements, string path, string entryPoint, string profile, ShaderFlags flags) : base(device, inputElements, path, entryPoint, profile, flags)
        {
        }

        protected override void UpdatePerFrameBuffer(DeviceContext context)
        {
            if (PerFrameBuffer == null) return;

            var content = (TPerFrameBuffer) PerFrameBufferContent;
            context.UpdateSubresource(ref content, PerFrameBuffer);
        }
    }
}