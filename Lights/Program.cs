using SharpDX.Windows;

namespace Lights
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