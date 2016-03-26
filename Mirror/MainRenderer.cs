using System;
using System.Windows.Forms;
using Engine.Core;
using Engine.Lights;
using Engine.Models;
using Engine.Objects;
using Engine.Objects.Buffers;
using Engine.Shaders;
using Engine.Textures;
using Engine.Utilities;
using Engine.Vertices;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;

namespace Mirror
{
    public class MainRenderer : FormRenderer
    {
        public Matrix View { get; set; }

        public MainRenderer(Form window) : base(window)
        {
            WindowCaption = "Main window";
        }

        protected override bool EnableMsaa { get; } = false;
        protected override int MsaaCount { get; }
        protected override int MsaaQuality { get; }
        protected override int BufferCount { get; } = 1;
        protected override float FieldOfView { get; } = (float) Math.PI/4;
        protected override float MinZ { get; } = 0.1f;
        protected override float MaxZ { get; } = 1000f;
        protected Vector3 Eye { get; set; }
        protected Vector3 EyeTarget { get; set; }
        protected Vector3 Up { get; set; }
        protected PerFrameBufferPn PerFrameBuffer;

        protected override void InitOverride()
        {
            base.InitOverride();
            PrepareMaterials();
            PrepareTextures();
            PrepareLights();
            PrepareModels();
            PrepareShaders();
            PrepareObjects();
        }

        protected override void FrameStart(float dt)
        {
            base.FrameStart(dt);
            ImmediateContext.ClearRenderTargetView(RenderTargetView, Color.LightSteelBlue);
            _viewProj = View*Proj;

            _pointLight.Position = new Vector3(
                70.0f * (float)Math.Cos(0.2f * Timer.GameTime),
                70.0f * (float)Math.Sin(0.2f * Timer.GameTime),
                10.0f
            );
            
            _spotLight.Position = Eye;
            _spotLight.Direction = Vector3.Normalize(EyeTarget - Eye);
            
            PerFrameBuffer.DirectionalLight = _directionalLight;
            PerFrameBuffer.PointLight = _pointLight;
            PerFrameBuffer.SpotLight = _spotLight;
            PerFrameBuffer.EyePos = Eye;

            _lightsPixelShader.PerFrameBufferContent = PerFrameBuffer;
            _texturesPixelShader.PerFrameBufferContent = PerFrameBuffer;
        }

        private Material _boxMat;
        private Material _landMat;
        private Material _wavesMat;

        private DirectionalLight _directionalLight;
        private PointLight _pointLight;
        private SpotLight _spotLight;

        private Model<VertexPnt, int> _cubeModel;
        private Model<VertexPn, int> _sphereModel;

        private PixelShaderDescription _texturesPixelShader;
        private VertexShaderDescription _texturesVertexShader;
        private PixelShaderDescription _lightsPixelShader;
        private VertexShaderDescription _lightsVertexShader;

        private BitmapTexture _boxTexture;

        private Matrix _viewProj;

        private void PrepareModels()
        {
            AddModelBuffer(PrimitiveTopology.TriangleList, Format.R32_UInt,
                _cubeModel      = ModelGenerator.CreateBoxPnt(2, 2, 2)
            );
            AddModelBuffer(PrimitiveTopology.TriangleList, Format.R32_UInt,
                _sphereModel    = ModelGenerator.CreateGeosphere(1, 5)
            );
        }

        private void PrepareTextures()
        {
            _boxTexture = new BitmapTexture(Device, @"Textures\WoodCrate01.dds");
        }

        private void PrepareMaterials()
        {
            _boxMat.Ambient = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            _boxMat.Diffuse = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            _boxMat.Specular = new Color4(0.6f, 0.6f, 0.6f, 16.0f);

            _landMat.Ambient = new Color4(0.48f, 0.77f, 0.46f, 1.0f);
            _landMat.Diffuse = new Color4(0.48f, 0.77f, 0.46f, 1.0f);
            _landMat.Specular = new Color4(0.2f, 0.2f, 0.2f, 16.0f);

            _wavesMat.Ambient = new Color4(0.137f, 0.42f, 0.556f, 1.0f);
            _wavesMat.Diffuse = new Color4(0.137f, 0.42f, 0.556f, 1.0f);
            _wavesMat.Specular = new Color4(0.8f, 0.8f, 0.8f, 96.0f);
        }

        private void PrepareLights()
        {
            // Directional light.
            _directionalLight.Ambient = new Color4(0.2f, 0.2f, 0.2f, 1.0f);
            _directionalLight.Diffuse = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            _directionalLight.Specular = new Color4(0.5f, 0.5f, 0.5f, 1.0f);
            _directionalLight.Direction = new Vector3(0.57735f, -0.57735f, 0.57735f);

            // Point light--position is changed every frame to animate
            // in UpdateScene function.
            _pointLight.Ambient = new Color4(0.3f, 0.3f, 0.3f, 1.0f);
            _pointLight.Diffuse = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            _pointLight.Specular = new Color4(0.7f, 0.7f, 0.7f, 1.0f);
            _pointLight.Att = new Vector3(0.0f, 0.1f, 0.0f);
            _pointLight.Range = 25.0f;

            // Spot light--position and direction changed every frame to
            // animate in UpdateScene function.
            _spotLight.Ambient = new Color4(0.0f, 0.0f, 0.0f, 1.0f);
            _spotLight.Diffuse = new Color4(1.0f, 1.0f, 0.0f, 1.0f);
            _spotLight.Specular = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
            _spotLight.Att = new Vector3(1.0f, 0.0f, 0.0f);
            _spotLight.Spot = 96.0f;
            _spotLight.Range = 10000.0f;
        }

        private void PrepareShaders()
        {
            var layout = new VertexPnt().InputLayout;
            var flags = ShaderFlags.None;

            #if DEBUG
            {
                flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization;
            }
            #endif

            _lightsVertexShader = AddVertexShader(layout, @"Shaders\Lights.fx", "Vs", "vs_5_0", flags);
            _lightsPixelShader = AddPixelShader<PerFrameBufferPn>(@"Shaders\Lights.fx", "Ps", "ps_5_0", flags);

            var pob = _lightsVertexShader.AddConstantBuffer<PerObjectBufferPn>(ShaderBufferType.PerObject);
            _lightsPixelShader.AddConstantBuffer<PerFrameBufferPn>();
            _lightsPixelShader.AddConstantBuffer(pob, ShaderBufferType.PerObject);

            _texturesVertexShader = AddVertexShader(layout, @"Shaders\Textures.fx", "Vs", "vs_5_0", flags);
            _texturesPixelShader = AddPixelShader<PerFrameBufferPn>(@"Shaders\Textures.fx", "Ps", "ps_5_0", flags);

            pob = _texturesVertexShader.AddConstantBuffer<PerObjectBufferPnt>(ShaderBufferType.PerObject);
            _texturesPixelShader.AddConstantBuffer<PerFrameBufferPn>();
            _texturesPixelShader.AddConstantBuffer(pob, ShaderBufferType.PerObject);
        }
        
        private void PrepareObjects()
        {
            Scene.Add(new ObjectPnt(_cubeModel)
            {
                Translate = Matrix.Translation(0, 2f, 0),
                Scale = Matrix.Identity,
                Rotate = Matrix.RotationY((float)Math.PI / 5),
                VertexShader = _texturesVertexShader,
                PixelShader = _texturesPixelShader,
                Texture = _boxTexture,
                Material = _boxMat,
            });

            Scene.Add(new ObjectPn(_sphereModel)
            {
                Translate = Matrix.Translation(0, -2f, 0),
                Scale = Matrix.Identity,
                Rotate = Matrix.RotationY((float)Math.PI / 5),
                VertexShader = _lightsVertexShader,
                PixelShader = _lightsPixelShader,
                Material = _wavesMat,
            });

            Eye = new Vector3(-10, 0, 0);
            EyeTarget = Vector3.Zero;
            Up = Vector3.UnitZ;

            View = Matrix.LookAtLH(Eye, EyeTarget, Up);
        }

        protected override void UpdateObject(Object3D obj, float dt)
            => obj.Rotate *= Matrix.RotationZ(dt/3);

        protected override void DrawObject(Object3D obj)
            => obj.Draw(ImmediateContext, _viewProj);
    }
}
