using System.Runtime.InteropServices;
using Engine.Lights;
using SharpDX;

namespace Engine.Objects.Buffers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PerObjectBufferPnt
    {
        public Matrix World;
        public Matrix WorldInv;
        public Matrix WorldViewProj;
        public Matrix TexTransform;
        public Material Material;
    }
}