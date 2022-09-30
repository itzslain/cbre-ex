using CBRE.DataStructures.Models;
using OpenTK.Graphics.OpenGL;
using System.Linq;

namespace CBRE.Editor.Rendering.Immediate
{
    public static class ModelRenderer
    {
        public static void Render(Model model)
        {
            System.Collections.Generic.List<DataStructures.Geometric.MatrixF> transforms = model.GetTransforms();

            GL.Color4(1f, 1f, 1f, 1f);

            foreach (IGrouping<int, Mesh> group in model.GetActiveMeshes().GroupBy(x => x.SkinRef))
            {
                Common.ITexture texture = model.Textures[group.Key].TextureObject;
                if (texture != null) texture.Bind();
                foreach (Mesh mesh in group)
                {
                    GL.Begin(PrimitiveType.Triangles);
                    foreach (MeshVertex v in mesh.Vertices)
                    {
                        DataStructures.Geometric.MatrixF transform = transforms[v.BoneWeightings.First().Bone.BoneIndex];
                        DataStructures.Geometric.CoordinateF c = v.Location * transform;
                        if (texture != null)
                        {
                            GL.TexCoord2(v.TextureU, v.TextureV);
                        }
                        GL.Vertex3(c.X, c.Y, c.Z);
                    }
                    GL.End();
                }
                if (texture != null) texture.Unbind();
            }
        }
    }
}