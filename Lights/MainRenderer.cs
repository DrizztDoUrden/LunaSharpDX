using System;
using System.Windows.Forms;
using Engine.Core;
using Engine.Lights;
using Engine.Models;
using Engine.Objects;
using Engine.Objects.Buffers;
using Engine.Shaders;
using Engine.Utilities;
using Engine.Vertices;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using ShaderDescription = Engine.Shaders.ShaderDescription;

namespace Lights
{
    public class MainRenderer : FormRenderer
    {
        public Matrix View { get; set; }

        public MainRenderer(Form window) : base(window)
        {
            WindowCaption = "Lights";
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

            _pixelShader.PerFrameBufferContent = PerFrameBuffer;
        }

        private Material _landMat;
        private Material _wavesMat;
        private DirectionalLight _directionalLight;
        private PointLight _pointLight;
        private SpotLight _spotLight;
        private Model<VertexPn, int> _cubeModel;
        private Model<VertexPn, int> _sphereModel;
        private PixelShaderDescription _pixelShader;
        private VertexShaderDescription _vertexShader;
        
        private Matrix _viewProj;

        private void PrepareModels()
        {
            AddModelBuffer(PrimitiveTopology.TriangleList, Format.R32_UInt,
                _cubeModel      = ModelGenerator.CreateBox(2, 2, 2),
                _sphereModel    = ModelGenerator.CreateGeosphere(1, 5)
            );
        }

        private void PrepareMaterials()
        {
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
            var layout = new VertexPn().InputLayout;
            var flags = ShaderFlags.None;

            #if DEBUG
            {
                flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization;
            }
            #endif

            _vertexShader = AddVertexShader(layout, @"Shaders/Shaders.fx", "Vs", "vs_5_0", flags);
            _pixelShader = AddPixelShader<PerFrameBufferPn>(@"Shaders/Shaders.fx", "Ps", "ps_5_0", flags);

            var pob = _vertexShader.AddConstantBuffer<PerObjectBufferPn>(ShaderBufferType.PerObject);
            _pixelShader.AddConstantBuffer<PerFrameBufferPn>();
            _pixelShader.AddConstantBuffer(pob, ShaderBufferType.PerObject);
        }
        
        private void PrepareObjects()
        {
            Scene.Add(new ObjectPn(_cubeModel)
            {
                Translate = Matrix.Translation(0, 2f, 0),
                Scale = Matrix.Identity,
                Rotate = Matrix.RotationY((float)Math.PI / 5),
                Material = _landMat,
                VertexShader = _vertexShader,
                PixelShader = _pixelShader,
            });

            Scene.Add(new ObjectPn(_sphereModel)
            {
                Translate = Matrix.Translation(0, -2f, 0),
                Scale = Matrix.Identity,
                Rotate = Matrix.RotationY((float)Math.PI / 5),
                Material = _wavesMat,
                VertexShader = _vertexShader,
                PixelShader = _pixelShader,
            });

            Eye = new Vector3(-10, 0, 0);
            EyeTarget = Vector3.Zero;
            Up = Vector3.UnitZ;

            View = Matrix.LookAtLH(Eye, EyeTarget, Up);
        }

        private static void CalculateNorms(Model<VertexPn, int> mesh)
        {
            for (var i = 0; i < mesh.Indices.Count; i += 3)
            {
                var i0 = mesh.Indices[i];
                var i1 = mesh.Indices[i + 1];
                var i2 = mesh.Indices[i + 2];

                var v0 = mesh.Vertices[i0];
                var v1 = mesh.Vertices[i1];
                var v2 = mesh.Vertices[i2];
                
                var e0 = (v1.Position - v0.Position);
                var e1 = (v2.Position - v0.Position);
                var n = Vector3.Cross(e0, e1);

                v0.Normal += n;
                v1.Normal += n;
                v2.Normal += n;

                mesh.Vertices[i0] = v0;
                mesh.Vertices[i1] = v1;
                mesh.Vertices[i2] = v2;
            }
        }

        protected override void UpdateObject(Object3D obj, float dt)
        {
            obj.Rotate *= Matrix.RotationZ(dt);
        }

        protected override void DrawObject(Object3D obj)
            => obj.Draw(ImmediateContext, _viewProj);
    }
}
