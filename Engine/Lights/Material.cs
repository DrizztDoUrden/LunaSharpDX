using System.Runtime.InteropServices;
using SharpDX;

namespace Engine.Lights
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Material
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Color4 Reflect;
    }
}