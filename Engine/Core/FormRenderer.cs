using System.Windows.Forms;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;

namespace Engine.Core
{
    public abstract class FormRenderer : Renderer
    {
        public Form Window { get; }
        public string WindowCaption { get; set; }

        protected FormRenderer(Form window)
        {
            Window = window;
        }

        #region Renderer

        protected sealed override SwapChainDescription GetSwapChainDescription()
            => new SwapChainDescription
            {
                OutputHandle = Window.Handle,
                IsWindowed = true,
                BufferCount = BufferCount,
                Usage = Usage.RenderTargetOutput,
                SwapEffect = SwapEffect.Discard,
                SampleDescription = SampleDescription,
                ModeDescription = new ModeDescription(Window.ClientSize.Width, Window.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
            };

        protected sealed override Texture2DDescription GetDepthStencilDescription()
            => new Texture2DDescription
            {
                Format = Format.D32_Float_S8X24_UInt,
                ArraySize = 1,
                MipLevels = 1,
                Width = Window.ClientSize.Width,
                Height = Window.ClientSize.Height,
                SampleDescription = SampleDescription,
                Usage = ResourceUsage.Default,
                BindFlags = BindFlags.DepthStencil,
            };

        protected sealed override void MainLoop() => RenderLoop.Run(Window, MainLoopBody);

        /// <summary>
        /// While overriding this methods either its base or resize(int, int) should be called.
        /// </summary>
        protected override void InitOverride()
        {
            Resize(Window.ClientSize.Width, Window.ClientSize.Height);
        }
        
        protected override void FrameStart(float dt)
        {
            base.FrameStart(dt);
            CalculateFrameStats();
        }

        #endregion

        private long _frameCount;
        private float _timeElapsed;

        private void CalculateFrameStats()
        {
            var curTime = Timer.GameTime;
            _frameCount++;

            if (!(curTime - _timeElapsed >= 1f)) return;

            var fps = (float)_frameCount;
            var msPerFrame = 1000f / fps;

            Window.Text = $"{WindowCaption}, FPS: {fps}, Frame time: {msPerFrame} ms";

            #if DEBUG
            {
                Window.Text += ", Debug";
            }
            #endif

            _frameCount = 0;
            _timeElapsed = curTime;
        }
    }
}