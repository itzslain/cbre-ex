using CBRE.Common;
using CBRE.DataStructures.Models;
using CBRE.Graphics.Arrays;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using System.Linq;

namespace CBRE.Editor.Rendering.Arrays
{
    public class ModelArray : VBO<Model, MapObjectVertex>
    {
        private const int Textured = 0;

        public ModelArray(Model model)
            : base(new[] { model })
        {
        }

        public void RenderTextured(IGraphicsContext context)
        {
            foreach (Subset subset in GetSubsets<ITexture>(Textured))
            {
                ((ITexture)subset.Instance).Bind();
                Render(context, PrimitiveType.Triangles, subset);
            }
        }

        protected override void CreateArray(IEnumerable<Model> objects)
        {
            foreach (Model model in objects)
            {
                PushOffset(model);

                List<DataStructures.Geometric.MatrixF> transforms = model.GetTransforms();

                foreach (IGrouping<int, Mesh> g in model.GetActiveMeshes().GroupBy(x => x.SkinRef))
                {
                    StartSubset(Textured);
                    Texture tex = model.Textures[g.Key];

                    foreach (Mesh mesh in g)
                    {
                        foreach (MeshVertex vertex in mesh.Vertices)
                        {
                            DataStructures.Geometric.MatrixF transform = transforms[vertex.BoneWeightings.First().Bone.BoneIndex];
                            DataStructures.Geometric.CoordinateF c = vertex.Location * transform;
                            DataStructures.Geometric.CoordinateF n = vertex.Normal * transform;
                            uint index = PushData(new[]
                            {
                                new MapObjectVertex
                                {
                                    Position = new Vector3(c.X, c.Y, c.Z),
                                    Normal = new Vector3(n.X, n.Y, n.Z),
                                    Colour = Color4.White,
                                    Texture = new Vector2(vertex.TextureU, vertex.TextureV),
                                    LightmapUv = new Vector2(-500.0f, -500.0f),
                                    IsSelected = 0
                                }
                            });
                            PushIndex(Textured, index, new[] { (uint)0 });
                        }
                    }
                    PushSubset(Textured, tex.TextureObject);
                }
            }
        }
    }
}