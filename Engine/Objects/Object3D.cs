using System.Collections.Generic;
using Engine.Models;
using Engine.Shaders;
using Engine.Vertices;
using SharpDX;
using SharpDX.Direct3D11;

namespace Engine.Objects
{
    public abstract class Object3D
    {
        public PixelShaderDescription PixelShader { get; set; }
        public VertexShaderDescription VertexShader { get; set; }

        public Matrix Scale
        {
            get { return _scale; }
            set
            {
                _worldNeedsUpdate = true;
                _scale = value;
            }
        }

        public Matrix Rotate
        {
            get { return _rotate; }
            set
            {
                _worldNeedsUpdate = true;
                _rotate = value;
            }
        }

        public Matrix Translate
        {
            get { return _translate; }
            set
            {
                _worldNeedsUpdate = true;
                _translate = value;
            }
        }

        public Matrix World
        {
            get
            {
                lock (_worldLock)
                {
                    if (!_worldNeedsUpdate)
                        return _world;

                    _world = _scale * _rotate * _translate;
                    _worldNeedsUpdate = false;

                    return _world;
                }
            }
        }

        public abstract void Draw(DeviceContext context, Matrix viewProj);

        private Matrix _scale;
        private Matrix _rotate;
        private Matrix _translate;
        private Matrix _world;
        private bool _worldNeedsUpdate;
        private readonly object _worldLock = new object();
    }

    public abstract class Object3D<TVertex, TIndex, TPerObjBuffer> : Object3D
        where TVertex : struct, IVertex
        where TIndex : struct
        where TPerObjBuffer : struct 
    {
        public Model<TVertex, TIndex> Model { get; }

        public sealed override void Draw(DeviceContext context, Matrix viewProj)
        {
            Buffer perObjBuffer = null;

            PixelShader.Select(context);
            if (PixelShader.PerObjBuffer != null)
                perObjBuffer = PixelShader.PerObjBuffer;

            VertexShader.Select(context);
            if (VertexShader.PerObjBuffer != null)
                perObjBuffer = VertexShader.PerObjBuffer;

            if (perObjBuffer != null)
            {
                var bufferContent = PrepareBuffer(viewProj);
                context.UpdateSubresource(ref bufferContent, perObjBuffer);
            }

            PrepareForDrawing(context);

            Model.Draw(context);
        }

        protected Object3D(Model<TVertex, TIndex> model)
        {
            Model = model;
        }

        protected abstract TPerObjBuffer PrepareBuffer(Matrix viewProj);
        protected virtual void PrepareForDrawing(DeviceContext context) { }
    }
}