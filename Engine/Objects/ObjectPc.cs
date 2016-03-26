using Engine.Models;
using Engine.Objects.Buffers;
using Engine.Vertices;
using SharpDX;

namespace Engine.Objects
{
    public class ObjectPc : Object3D<VertexPc, int, PerObjectBufferPc>
    {
        public ObjectPc(Model<VertexPc, int> model) : base(model)
        {
        }

        protected override PerObjectBufferPc PrepareBuffer(Matrix viewProj)
            => new PerObjectBufferPc { WorldViewProj = Matrix.Transpose(World * viewProj), };
    }
}