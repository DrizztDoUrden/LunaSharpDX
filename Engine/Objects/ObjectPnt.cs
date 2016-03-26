using Engine.Lights;
using Engine.Models;
using Engine.Objects.Buffers;
using Engine.Textures;
using Engine.Vertices;
using SharpDX.Direct3D11;
using Matrix = SharpDX.Matrix;

namespace Engine.Objects
{
    public class ObjectPnt : Object3D<VertexPnt, int, PerObjectBufferPnt>
    {
        public Material Material { get; set; }
        public Texture Texture { get; set; }

        public ObjectPnt(Model<VertexPnt, int> model) : base(model)
        {
        }

        protected override PerObjectBufferPnt PrepareBuffer(Matrix viewProj)
        {
            var wt = Matrix.Transpose(World);

            return new PerObjectBufferPnt
            {
                World = wt,
                WorldInv = Matrix.Invert(wt),
                WorldViewProj = Matrix.Transpose(World * viewProj),
                TexTransform = Texture.TexTransform,
                Material = Material,
            };
        }

        protected override void PrepareForDrawing(DeviceContext context)
        {
            if (Texture != null)
                context.PixelShader.SetShaderResource(0, Texture.ShaderResourceView);
        }
    }
}