using SharpDX.Windows;

namespace Textures
{
    public static class Program
    {
        public static void Main()
        {
            using (var renderer = new MainRenderer(new RenderForm()))
            {
                renderer.Run();
            }
        }
    }
}