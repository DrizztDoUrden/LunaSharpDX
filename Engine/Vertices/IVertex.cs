using SharpDX.Direct3D11;

namespace Engine.Vertices
{
    public interface IVertex
    {
        int Size { get; }
        InputElement[] InputLayout { get; }
    }
}