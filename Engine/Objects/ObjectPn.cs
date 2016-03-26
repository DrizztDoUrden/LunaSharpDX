using Engine.Lights;
using Engine.Models;
using Engine.Objects.Buffers;
using Engine.Vertices;
using Matrix = SharpDX.Matrix;

namespace Engine.Objects
{
    public class ObjectPn : Object3D<VertexPn, int, PerObjectBufferPn>
    {
        public Material Material { get; set; }

        public ObjectPn(Model<VertexPn, int> model) : base(model)
        {
        }

        protected override PerObjectBufferPn PrepareBuffer(Matrix viewProj)
        {
            var wt = Matrix.Transpose(World);

            return new PerObjectBufferPn
            {
                World = wt,
                WorldInv = Matrix.Invert(wt),
                WorldViewProj = Matrix.Transpose(World * viewProj),
                Material = Material,
            };
        }
    }
}