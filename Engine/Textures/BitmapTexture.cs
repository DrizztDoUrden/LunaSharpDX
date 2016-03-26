using Engine.Utilities;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.WIC;

namespace Engine.Textures
{
    public class BitmapTexture : Texture
    {
        public BitmapSource Bitmap { get; }
        public Texture2D Texture2D { get; }

        public override Matrix TexTransform
            => Matrix.Identity;

        public BitmapTexture(Device device, BitmapSource bitmap)
        {
            Bitmap = bitmap;
            Texture2D = device.CreateTexture2DFromBitmap(bitmap);
            ShaderResourceView = new ShaderResourceView(device, Texture2D);
        }

        public BitmapTexture(Device device, string path)
        {
            using (var factory = new ImagingFactory2())
                Bitmap = factory.LoadBitmap(path);
            
            Texture2D = device.CreateTexture2DFromBitmap(Bitmap);
            ShaderResourceView = new ShaderResourceView(device, Texture2D);
        }

        protected override void DisposeOverride()
        {
            Bitmap.Dispose();
            Texture2D.Dispose();
        }
    }
}