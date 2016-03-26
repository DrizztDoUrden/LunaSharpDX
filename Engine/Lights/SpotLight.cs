﻿using System.Runtime.InteropServices;
using SharpDX;

namespace Engine.Lights
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SpotLight
    {
        public Color4 Ambient;
        public Color4 Diffuse;
        public Color4 Specular;
        public Vector3 Position;
        public float Range;
        public Vector3 Direction;
        public float Spot;
        public Vector3 Att;
        public float Pad;
    }
}