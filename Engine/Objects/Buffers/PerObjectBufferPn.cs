using System.Runtime.InteropServices;
using Engine.Lights;
using SharpDX;

namespace Engine.Objects.Buffers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PerObjectBufferPn
    {
        public Matrix World;
        public Matrix WorldInv;
        public Matrix WorldViewProj;
        public Material Material;
    }
}