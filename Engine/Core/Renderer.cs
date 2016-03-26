using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Engine.Buffers;
using Engine.Models;
using Engine.Objects;
using Engine.Shaders;
using Engine.Utilities;
using Engine.Vertices;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Resource = SharpDX.Direct3D11.Resource;
using ShaderDescription = Engine.Shaders.ShaderDescription;

namespace Engine.Core
{
    public abstract class Renderer : IDisposable
    {
        #region Public API

        public List<Object3D> Scene { get; } = new List<Object3D>();

        public void Run()
        {
            Init();
            MainLoop();
        }

        public void Pause()
        {
            Timer.Stop();
            Paused = true;
        }

        public void Unpause()
        {
            Paused = false;
            Timer.Start();
        }

        public void Resize(int width, int height)
        {
            Width = width;
            Height = height;
            Resized = true;
        }

        public void AddModelBuffer<TVertex, TIndex>(PrimitiveTopology topology, Format indexFormat, params Model<TVertex, TIndex>[] models)
            where TVertex : struct, IVertex
            where TIndex : struct
            => _modelsController.AddBuffer(topology, indexFormat, models);

        public PixelShaderDescription AddPixelShader(string path, string entryPoint, string profile, ShaderFlags flags)
            => _shadersController.AddPixelShader(path, entryPoint, profile, flags);

        public VertexShaderDescription AddVertexShader(InputElement[] inputElements, string path, string entryPoint, string profile, ShaderFlags flags)
            => _shadersController.AddVertexShader(inputElements, path, entryPoint, profile, flags);

        public PixelShaderDescription AddPixelShader<TPerFrameBuffer>(string path, string entryPoint, string profile, ShaderFlags flags)
            where TPerFrameBuffer : struct
            => _shadersController.AddPixelShader<TPerFrameBuffer>(path, entryPoint, profile, flags);

        public VertexShaderDescription AddVertexShader<TPerFrameBuffer>(InputElement[] inputElements, string path, string entryPoint, string profile, ShaderFlags flags)
            where TPerFrameBuffer : struct
            => _shadersController.AddVertexShader<TPerFrameBuffer>(inputElements, path, entryPoint, profile, flags);

        #endregion

        #region Protected API

        protected GameTimer Timer { get; private set; } = new GameTimer();
        protected bool Paused { get; private set; }
        protected int Width { get; private set; }
        protected int Height { get; private set; }
        protected float AspectRatio => (float) Width/Height;
        protected bool Resized { get; set; }
        protected Matrix Proj { get; set; }
        protected abstract bool EnableMsaa { get; }
        protected abstract int MsaaCount { get; }
        protected abstract int MsaaQuality { get; }
        protected SampleDescription SampleDescription => EnableMsaa ? new SampleDescription(MsaaCount, MsaaQuality) : new SampleDescription(1, 0);

        protected Device Device { get; private set; }
        protected DeviceContext ImmediateContext { get; private set; }
        protected SwapChain SwapChain { get; private set; }
        protected Texture2D DepthStencilBuffer { get; private set; }
        protected Texture2D BackBuffer { get; private set; }
        protected RenderTargetView RenderTargetView { get; private set; }
        protected DepthStencilView DepthStencilView { get; private set; }

        protected void MainLoopBody()
        {
            Timer.Tick();
            if (!Paused)
            {
                if (Resized)
                    Resize();

                var dt = Timer.DeltaTime;
                FrameStart(dt);

                Parallel.ForEach(Scene, obj =>
                    UpdateObject(obj, dt)
                );

                foreach (var obj in Scene)
                    DrawObject(obj);

                Present();

                return;
            }

            Thread.Sleep(10);
        }

        #endregion

        #region Private

        private bool _firstResize = true;
        private ModelBufferController _modelsController;
        private ShadersController _shadersController;

        private void Resize()
        {
            if (!_firstResize)
            {
                Dispose(BackBuffer);
                Dispose(RenderTargetView);
                Dispose(DepthStencilBuffer);
                Dispose(DepthStencilView);
                
                SwapChain.ResizeBuffers(BufferCount, Width, Height, Format.Unknown, SwapChainFlags.None);

                _firstResize = false;
            }

            RegisterToDispose(BackBuffer = Resource.FromSwapChain<Texture2D>(SwapChain, 0));
            RegisterToDispose(RenderTargetView = new RenderTargetView(Device, BackBuffer));
            RegisterToDispose(DepthStencilBuffer = new Texture2D(Device, GetDepthStencilDescription()));
            RegisterToDispose(DepthStencilView = new DepthStencilView(Device, DepthStencilBuffer));

            ImmediateContext.Rasterizer.SetViewport(new Viewport(0, 0, Width, Height, 0.0f, 1.0f));
            ImmediateContext.OutputMerger.SetTargets(DepthStencilView, RenderTargetView);

            Proj = Matrix.PerspectiveFovLH(FieldOfView, AspectRatio, MinZ, MaxZ);

            Resized = false;
        }

        private void Init()
        {
            InitMain();
            InitD3D();
            InitOverride();
        }

        private void InitMain()
        {
            Timer.Reset();
            Paused = false;
        }

        private void InitD3D()
        {
            Device device;
            SwapChain swapChain;
            var flags = DeviceCreationFlags.None;

            #if DEBUG
            {
                flags |= DeviceCreationFlags.Debug;
            }
            #endif

            var scDesc = GetSwapChainDescription();
            Device.CreateWithSwapChain(DriverType.Hardware, flags, scDesc, out device, out swapChain);

            RegisterToDispose(Device = device);
            RegisterToDispose(SwapChain = swapChain);
            RegisterToDispose(ImmediateContext = device.ImmediateContext);
            RegisterToDispose(_modelsController = new ModelBufferController(Device));
            RegisterToDispose(_shadersController = new ShadersController(Device));
        }

        #endregion

        #region To override

        protected abstract int BufferCount { get; }
        protected abstract float FieldOfView { get;}
        protected abstract float MinZ { get;}
        protected abstract float MaxZ { get;}

        protected abstract SwapChainDescription GetSwapChainDescription();
        protected abstract Texture2DDescription GetDepthStencilDescription();
        protected abstract void MainLoop();

        protected virtual void InitOverride() { }
        protected virtual void DisposeOverride() { }

        protected virtual void UpdateObject(Object3D obj, float dt) { }
        protected virtual void DrawObject(Object3D obj) { }

        protected virtual void FrameStart(float dt)
        {
            ImmediateContext.ClearDepthStencilView(DepthStencilView, DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        protected virtual void Present()
        {
            SwapChain.Present(0, PresentFlags.None);
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            if (_disposed)
                return;

            DisposeOverride();
            
            _toDispose.Reverse();
            foreach (var toDispose in _toDispose)
                toDispose.Dispose();

            _disposed = true;
        }

        protected void RegisterToDispose(IDisposable toDispose)
            => _toDispose.Add(toDispose);

        protected void RegisterToDispose(params IDisposable[] toDispose)
            => _toDispose.AddRange(toDispose);

        protected void Dispose(IDisposable prevValue)
        {
            if (prevValue == null)
                return;
            
            _toDispose.Remove(prevValue);
            prevValue.Dispose();
        }

        private readonly List<IDisposable> _toDispose = new List<IDisposable>(); 
        private bool _disposed;

        #endregion
    }
}