using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Models;
using Engine.Vertices;
using SharpDX;

namespace Engine.Utilities
{
    public static class ModelGenerator
    {
        public static Model<VertexPn, int> ToPn(this Model<VertexPnt, int> pnt)
            => new Model<VertexPn, int>
            {
                Vertices = pnt.Vertices
                    .Select(v => new VertexPn { Position = v.Position, Normal = v.Normal, })
                    .ToList(),
                Indices = pnt.Indices,
            };

        private static void Subdivide(Model<VertexPnt, int> model)
        {
            var newV = new List<VertexPnt>();
            var newI = new List<int>();

            for (var i = 0; i < model.IndexCount; i += 3)
            {
                var i0 = model.Indices[i];
                var i1 = model.Indices[i + 1];
                var i2 = model.Indices[i + 2];

                var v0 = model.Vertices[i0];
                var v1 = model.Vertices[i1];
                var v2 = model.Vertices[i2];

                var v01 = new VertexPnt
                {
                    Position = (v0.Position + v1.Position) / 2,
                    Texture = (v0.Texture + v1.Texture) / 2,
                };
                var v02 = new VertexPnt
                {
                    Position = (v0.Position + v2.Position) / 2,
                    Texture = (v0.Texture + v2.Texture) / 2,
                };
                var v12 = new VertexPnt
                {
                    Position = (v1.Position + v2.Position) / 2,
                    Texture = (v1.Texture + v2.Texture) / 2,
                };

                var baseI = newV.Count;

                newV.AddRange(new[]
                {
                    v0,  v1,  v2,
                    v01, v02, v12,
                });

                newI.AddRange(new[]
                {
                    baseI    , baseI + 3, baseI + 4,
                    baseI + 1, baseI + 5, baseI + 3,
                    baseI + 2, baseI + 4, baseI + 5,
                    baseI + 3, baseI + 5, baseI + 4,
                });
            }

            model.Indices = newI;
            model.Vertices = newV;
        }

        public static Model<VertexPn, int> CreateIcosahedron(float radius)
            => CreateIcosahedronPnt(radius).ToPn();

        public static Model<VertexPnt, int> CreateIcosahedronPnt(float radius)
        {
            var x = 0.525731f * radius;
            var y = 0.850651f * radius;
            var model = new Model<VertexPnt, int>();

            Vector3[] pos =
            {
                new Vector3(-x, 0.0f, y),  new Vector3(x, 0.0f, y),
                new Vector3(-x, 0.0f, -y), new Vector3(x, 0.0f, -y),
                new Vector3(0.0f, y, x),   new Vector3(0.0f, y, -x),
                new Vector3(0.0f, -y, x),  new Vector3(0.0f, -y, -x),
                new Vector3(y, x, 0.0f),   new Vector3(-y, x, 0.0f),
                new Vector3(y, -x, 0.0f),  new Vector3(-y, -x, 0.0f)
            };

            int[] k =
            {
                1,4,0,  4,9,0,  4,5,9,  8,5,4,  1,8,4,
                1,10,8, 10,3,8, 8,3,5,  3,2,5,  3,7,2,
                3,10,7, 10,6,7, 6,11,7, 6,0,11, 6,1,0,
                10,1,6, 11,0,9, 2,11,9, 5,2,9,  11,2,7
            };

            for (var i = 0; i < 12; ++i)
                model.Vertices.Add(new VertexPnt { Position = pos[i], });

            for (var i = 0; i < 60; ++i)
                model.Indices.Add(k[i]);

            return model;
        }

        public static Model<VertexPn, int> CreateGeosphere(float radius, int numSubdivisions)
            => CreateGeospherePnt(radius, numSubdivisions).ToPn();

        public static Model<VertexPnt, int> CreateGeospherePnt(float radius, int numSubdivisions)
        {
            // Put a cap on the number of subdivisions.
            numSubdivisions = Math.Min(numSubdivisions, 5);

            // Approximate a sphere by tessellating an icosahedron.
            var model = CreateIcosahedronPnt(1);

            for (var i = 0; i < numSubdivisions; ++i)
                Subdivide(model);

            // Project vertices onto sphere and scale.
            for (var i = 0; i < model.Vertices.Count; ++i)
            {
                // Project onto unit sphere.
                var n = Vector3.Normalize(model.Vertices[i].Position);

                // Project onto sphere.
                var p = radius * n;

                model.Vertices[i] = new VertexPnt
                {
                    Position = p,
                    Normal = n,
                };

                // Derive texture coordinates from spherical coordinates.
                /*float theta = MathHelper::AngleFromXY(
                    model.Vertices[i].Position.X,
                    model.Vertices[i].Position.Y);

                float phi = (float)Math.Acos(model.Vertices[i].Position.Z / radius);

                model.Vertices[i].TexC.x = theta / XM_2PI;
                model.Vertices[i].TexC.y = phi / XM_PI;

                // Partial derivative of P with respect to theta
                model.Vertices[i].TangentU.X = -radius * sinf(phi) * sinf(theta);
                model.Vertices[i].TangentU.Z = 0.0f;
                model.Vertices[i].TangentU.Y = +radius * sinf(phi) * cosf(theta);

                XMVECTOR T = XMLoadFloat3(&model.Vertices[i].TangentU);
                XMStoreFloat3(&model.Vertices[i].TangentU, XMVector3Normalize(T));*/
            }

            return model;
        }

        public static Model<VertexPn, int> CreateBox(float width, float height, float depth)
            => CreateBoxPnt(width, height, depth).ToPn();

        public static Model<VertexPnt, int> CreateBoxPnt(float width, float height, float depth)
        {
            var w2 = 0.5f * width;
            var h2 = 0.5f * height;
            var d2 = 0.5f * depth;

            // Create the vertices.
            var vertices = new List<VertexPnt>
            {
                // Fill in the front face vertex data.
                new VertexPnt { Position = new Vector3(-w2, -h2, -d2), Normal = new Vector3(0.0f, 0.0f, -1.0f), Texture = new Vector2(0.0f, 1.0f), },
                new VertexPnt { Position = new Vector3(-w2, +h2, -d2), Normal = new Vector3(0.0f, 0.0f, -1.0f), Texture = new Vector2(0.0f, 0.0f), },
                new VertexPnt { Position = new Vector3(+w2, +h2, -d2), Normal = new Vector3(0.0f, 0.0f, -1.0f), Texture = new Vector2(1.0f, 0.0f), },
                new VertexPnt { Position = new Vector3(+w2, -h2, -d2), Normal = new Vector3(0.0f, 0.0f, -1.0f), Texture = new Vector2(1.0f, 1.0f), },

                // Fill in the back face vertex data.
                new VertexPnt { Position = new Vector3(-w2, -h2, +d2), Normal = new Vector3(0.0f, 0.0f, 1.0f), Texture = new Vector2(1.0f, 1.0f), },
                new VertexPnt { Position = new Vector3(+w2, -h2, +d2), Normal = new Vector3(0.0f, 0.0f, 1.0f), Texture = new Vector2(0.0f, 1.0f), },
                new VertexPnt { Position = new Vector3(+w2, +h2, +d2), Normal = new Vector3(0.0f, 0.0f, 1.0f), Texture = new Vector2(0.0f, 0.0f), },
                new VertexPnt { Position = new Vector3(-w2, +h2, +d2), Normal = new Vector3(0.0f, 0.0f, 1.0f), Texture = new Vector2(1.0f, 0.0f), },

                // Fill in the top face vertex data.
                new VertexPnt { Position = new Vector3(-w2, +h2, -d2), Normal = new Vector3(0.0f, 1.0f, 0.0f), Texture = new Vector2(0.0f, 1.0f), },
                new VertexPnt { Position = new Vector3(-w2, +h2, +d2), Normal = new Vector3(0.0f, 1.0f, 0.0f), Texture = new Vector2(0.0f, 0.0f), },
                new VertexPnt { Position = new Vector3(+w2, +h2, +d2), Normal = new Vector3(0.0f, 1.0f, 0.0f), Texture = new Vector2(1.0f, 0.0f), },
                new VertexPnt { Position = new Vector3(+w2, +h2, -d2), Normal = new Vector3(0.0f, 1.0f, 0.0f), Texture = new Vector2(1.0f, 1.0f), },

                // Fill in the bottom face vertex data.
                new VertexPnt { Position = new Vector3(-w2, -h2, -d2), Normal = new Vector3(0.0f, -1.0f, 0.0f), Texture = new Vector2(1.0f, 1.0f), },
                new VertexPnt { Position = new Vector3(+w2, -h2, -d2), Normal = new Vector3(0.0f, -1.0f, 0.0f), Texture = new Vector2(0.0f, 1.0f), },
                new VertexPnt { Position = new Vector3(+w2, -h2, +d2), Normal = new Vector3(0.0f, -1.0f, 0.0f), Texture = new Vector2(0.0f, 0.0f), },
                new VertexPnt { Position = new Vector3(-w2, -h2, +d2), Normal = new Vector3(0.0f, -1.0f, 0.0f), Texture = new Vector2(1.0f, 0.0f), },

                // Fill in the left face vertex data.
                new VertexPnt { Position = new Vector3(-w2, -h2, +d2), Normal = new Vector3(-1.0f, 0.0f, 0.0f), Texture = new Vector2(0.0f, 1.0f), },
                new VertexPnt { Position = new Vector3(-w2, +h2, +d2), Normal = new Vector3(-1.0f, 0.0f, 0.0f), Texture = new Vector2(0.0f, 0.0f), },
                new VertexPnt { Position = new Vector3(-w2, +h2, -d2), Normal = new Vector3(-1.0f, 0.0f, 0.0f), Texture = new Vector2(1.0f, 0.0f), },
                new VertexPnt { Position = new Vector3(-w2, -h2, -d2), Normal = new Vector3(-1.0f, 0.0f, 0.0f), Texture = new Vector2(1.0f, 1.0f), },

                // Fill in the right face vertex data.
                new VertexPnt { Position = new Vector3(+w2, -h2, -d2), Normal = new Vector3(1.0f, 0.0f, 0.0f), Texture = new Vector2(0.0f, 1.0f), },
                new VertexPnt { Position = new Vector3(+w2, +h2, -d2), Normal = new Vector3(1.0f, 0.0f, 0.0f), Texture = new Vector2(0.0f, 0.0f), },
                new VertexPnt { Position = new Vector3(+w2, +h2, +d2), Normal = new Vector3(1.0f, 0.0f, 0.0f), Texture = new Vector2(1.0f, 0.0f), },
                new VertexPnt { Position = new Vector3(+w2, -h2, +d2), Normal = new Vector3(1.0f, 0.0f, 0.0f), Texture = new Vector2(1.0f, 1.0f), },
            };
            
            // Create the indices.
            var indices = new int[36];
            for (int i = 0, j = 0; i < 36; i += 6, j += 4)
            {
                indices[i    ] = j;
                indices[i + 1] = j + 1;
                indices[i + 2] = j + 2;
                indices[i + 3] = j;
                indices[i + 4] = j + 2;
                indices[i + 5] = j + 3;
            }

            return new Model<VertexPnt, int>(vertices, new List<int>(indices));
        }
    }
}