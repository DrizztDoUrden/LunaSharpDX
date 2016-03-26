using SharpDX.Windows;

namespace Mirror
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