using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Engine.Core;
using Engine.Models;
using Engine.Objects;
using Engine.Objects.Buffers;
using Engine.Shaders;
using Engine.Vertices;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.DXGI;
using ShaderDescription = Engine.Shaders.ShaderDescription;

namespace ColoredCube
{
    public class MainRenderer : FormRenderer
    {
        public Matrix View { get; set; }

        public MainRenderer(Form window) : base(window)
        {
            WindowCaption = "ColoredCube";
        }

        protected override bool EnableMsaa { get; } = false;
        protected override int MsaaCount { get; }
        protected override int MsaaQuality { get; }
        protected override int BufferCount { get; } = 1;
        protected override float FieldOfView { get; } = (float) Math.PI/4;
        protected override float MinZ { get; } = 0.1f;
        protected override float MaxZ { get; } = 1000f;

        protected override void InitOverride()
        {
            base.InitOverride();
            PrepareModels();
            PrepareShaders();
            PrepareObjects();
        }

        protected override void FrameStart(float dt)
        {
            base.FrameStart(dt);
            ImmediateContext.ClearRenderTargetView(RenderTargetView, Color.LightSteelBlue);
            _viewProj = View*Proj;
        }

        private Model<VertexPc, int> _cubeModel;
        private PixelShaderDescription _pixelShader;
        private VertexShaderDescription _vertexShader;
        private Matrix _viewProj;

        private void PrepareModels()
        {
            _cubeModel = new Model<VertexPc, int>(
                vertices: new List<VertexPc>
                {
                    new VertexPc { Position = new Vector3(-1, -1, -1), Color = new Vector4(1, 0, 0, 1), },
                    new VertexPc { Position = new Vector3(-1, +1, -1), Color = new Vector4(0, 1, 0, 1), },
                    new VertexPc { Position = new Vector3(+1, +1, -1), Color = new Vector4(0, 0, 1, 1), },
                    new VertexPc { Position = new Vector3(+1, -1, -1), Color = new Vector4(1, 1, 1, 1), },
                    new VertexPc { Position = new Vector3(-1, -1, +1), Color = new Vector4(1, 0, 0, 1), },
                    new VertexPc { Position = new Vector3(-1, +1, +1), Color = new Vector4(0, 1, 0, 1), },
                    new VertexPc { Position = new Vector3(+1, +1, +1), Color = new Vector4(0, 0, 1, 1), },
                    new VertexPc { Position = new Vector3(+1, -1, +1), Color = new Vector4(1, 1, 1, 1), },
                }, 
                indices: new List<int>
                {
                    // front face
		            0, 1, 2,  0, 2, 3,
                    // back face
		            4, 6, 5,  4, 7, 6,
		            // left face
		            4, 5, 1,  4, 1, 0,
		            // right face
		            3, 2, 6,  3, 6, 7,
		            // top face
		            1, 5, 6,  1, 6, 2,
		            // bottom face
		            4, 0, 3,  4, 3, 7,
                }
            );

            AddModelBuffer(PrimitiveTopology.TriangleList, Format.R32_UInt, _cubeModel);
        }

        private void PrepareShaders()
        {
            var layout = new VertexPc().InputLayout;
            var flags = ShaderFlags.None;

            #if DEBUG
            {
                flags |= ShaderFlags.Debug | ShaderFlags.SkipOptimization;
            }
            #endif

            _vertexShader = AddVertexShader(layout, @"Shaders/Colors.fx", "Vs", "vs_5_0", flags);
            _pixelShader = AddPixelShader(@"Shaders/Colors.fx", "Ps", "ps_5_0", flags);

            _vertexShader.AddConstantBuffer<PerObjectBufferPc>(ShaderBufferType.PerObject);
        }
        
        private void PrepareObjects()
        {
            Scene.Add(new ObjectPc(_cubeModel)
            {
                Translate = Matrix.Translation(0, 2f, 0),
                Scale = Matrix.Identity,
                Rotate = Matrix.RotationY((float)Math.PI / 5),
                VertexShader = _vertexShader,
                PixelShader = _pixelShader,
            });

            Scene.Add(new ObjectPc(_cubeModel)
            {
                Translate = Matrix.Translation(0, -2f, 0),
                Scale = Matrix.Identity,
                Rotate = Matrix.RotationY((float)Math.PI / 5),
                VertexShader = _vertexShader,
                PixelShader = _pixelShader,
            });

            var eye = new Vector3(-10, 0, 0);
            var target = Vector3.Zero;
            var up = Vector3.UnitZ;

            View = Matrix.LookAtLH(eye, target, up);
        }

        protected override void UpdateObject(Object3D obj, float dt)
        {
            obj.Rotate *= Matrix.RotationZ(dt);
        }

        protected override void DrawObject(Object3D obj) => obj.Draw(ImmediateContext, _viewProj);
    }
}
