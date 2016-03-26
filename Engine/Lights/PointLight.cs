using System.Runtime.InteropServices;
using SharpDX;

namespace Engine.Lights
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PointLight
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Vector3 Position;
        public float Range;
        public Vector3 Att;
        public float Pad;
    }
}