using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;

namespace Engine.Shaders
{
    public class PixelShaderDescription : ShaderDescription
    {
        public PixelShaderDescription(Device device, string path, string entryPoint, string profile, ShaderFlags flags) : base(device, path, entryPoint, profile, flags)
        {
            _shader = new PixelShader(device, Bytecode);
        }

        protected override void SelectOverride(DeviceContext context)
        {
            context.PixelShader.Set(_shader);
        }

        protected override CommonShaderStage GetShaderStage(DeviceContext context)
            => context.PixelShader;

        protected override bool Check()
            => CheckPixelShader();

        protected override void DisposeCore()
        {
            _shader?.Dispose();
        }

        private readonly PixelShader _shader;
    }

    public class PixelShaderDescription<TPerFrameBuffer> : PixelShaderDescription
        where TPerFrameBuffer : struct
    {
        public PixelShaderDescription(Device device, string path, string entryPoint, string profile, ShaderFlags flags) : base(device, path, entryPoint, profile, flags)
        {
        }

        protected override void UpdatePerFrameBuffer(DeviceContext context)
        {
            if (PerFrameBuffer == null) return;

            var content = (TPerFrameBuffer)PerFrameBufferContent;
            context.UpdateSubresource(ref content, PerFrameBuffer);
        }
    }
}