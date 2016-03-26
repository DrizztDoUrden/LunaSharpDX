using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Engine.Vertices
{
    public struct VertexPnt : IVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 Texture;

        public int Size => SharpDX.Utilities.SizeOf<VertexPnt>();

        public InputElement[] InputLayout => new[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0),
        };
    }
}