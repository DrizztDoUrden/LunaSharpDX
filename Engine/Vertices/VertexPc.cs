using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Engine.Vertices
{
    public struct VertexPc : IVertex
    {
        public Vector3 Position;
        public Vector4 Color;

        public int Size => SharpDX.Utilities.SizeOf<VertexPc>();

        public InputElement[] InputLayout => new[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 12, 0),
        };
    }
}