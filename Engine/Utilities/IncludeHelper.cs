using System;
using System.IO;
using SharpDX.D3DCompiler;

namespace Engine.Utilities
{
    public sealed class IncludeHelper : Include
    {
        public IDisposable Shadow { get; set; }

        public void Dispose() { }
        
        public Stream Open(IncludeType type, string fileName, Stream parentStream) => File.OpenRead(fileName);

        public void Close(Stream stream) => stream.Close();
    }
}