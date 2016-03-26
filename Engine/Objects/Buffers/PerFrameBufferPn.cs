using System.Runtime.InteropServices;
using Engine.Lights;
using SharpDX;

namespace Engine.Objects.Buffers
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PerFrameBufferPn
    {
        public DirectionalLight DirectionalLight;
        public PointLight PointLight;
        public SpotLight SpotLight;
        public Vector3 EyePos;
        public float Pad;
    }
}