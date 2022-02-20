using CBRE.Common;
using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Extensions;
using CBRE.Graphics.Arrays;
using CBRE.Graphics.Helpers;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using GL = OpenTK.Graphics.OpenGL.GL;

namespace CBRE.Editor.Rendering.Arrays {
    public class MapObjectArray : VBO<MapObject, MapObjectVertex> {
        private const int Textured = 0;
        private const int Transparent = 1;
        private const int BrushWireframe = 2;
        private const int EntityWireframe = 3;

        public MapObjectArray(IEnumerable<MapObject> data)
            : base(data) {
        }

        public void RenderTextured(IGraphicsContext context, ITexture lightmapTexture) {
            foreach (Subset subset in GetSubsets<ITexture>(Textured).Where(x => x.Instance != null)) {
                ITexture tex = (ITexture)subset.Instance;
                GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
                
                tex.Bind();
                GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture1);
                lightmapTexture.Bind();
                Render(context, PrimitiveType.Triangles, subset);
                GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
            }
        }
        public void RenderUntextured(IGraphicsContext context, Coordinate location) {
            GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
            TextureHelper.Unbind();
            GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture1);
            TextureHelper.Unbind();
            GL.ActiveTexture(OpenTK.Graphics.OpenGL.TextureUnit.Texture0);
            foreach (var subset in GetSubsets<ITexture>(Textured).Where(x => x.Instance == null)) {
                Render(context, PrimitiveType.Triangles, subset);
            }
            foreach (var subset in GetSubsets<Entity>(Textured)) {
                var e = (Entity)subset.Instance;
                if (!CBRE.Settings.View.DisableModelRendering && e.HasModel() && e.HideDistance() > (location - e.Origin).VectorMagnitude()) continue;
                Render(context, PrimitiveType.Triangles, subset);
            }
        }

        private decimal LookAtOrder(Face face, Coordinate cameraLocation, Coordinate lookAt) {
            return -(face.BoundingBox.Center - cameraLocation).LengthSquared();
        }

        public void RenderTransparent(IGraphicsContext context, Action<TextureReference> textureCallback, Coordinate cameraLocation, Coordinate lookAt) {

            IEnumerable<Subset> sorted =
                from subset in GetSubsets<Face>(Transparent)
                let face = subset.Instance as Face
                where face != null
                orderby LookAtOrder(face, cameraLocation, lookAt) ascending
                select subset;
            foreach (Subset subset in sorted) {
                TextureReference tex = ((Face)subset.Instance).Texture;
                if(opts.HideToolTextures) {
                    if (tex.Name.ToLowerInvariant() == "tooltextures/invisible_collision") continue;
                    if (tex.Name.ToLowerInvariant() == "tooltextures/remove_face") continue;
                    if (tex.Name.ToLowerInvariant() == "tooltextures/block_light") continue;
                }
                if (tex.Texture != null) tex.Texture.Bind();
                else TextureHelper.Unbind();
                textureCallback(tex);
                GL.DepthMask(false);
                if (!tex.IsToolTexture) {
                    GL.Disable(OpenTK.Graphics.OpenGL.EnableCap.CullFace);
                }
                //program.Set("isTextured", tex.Texture != null);
                Render(context, PrimitiveType.Triangles, subset);
                GL.DepthMask(true);
                GL.Enable(OpenTK.Graphics.OpenGL.EnableCap.CullFace);
            }
        }

        public void RenderWireframe(IGraphicsContext context) {
            foreach (var subset in GetSubsets(BrushWireframe)) {
                Render(context, PrimitiveType.Lines, subset);
            }

            foreach (var subset in GetSubsets(EntityWireframe)) {
                Render(context, PrimitiveType.Lines, subset);
            }
        }

        public void RenderVertices(IGraphicsContext context, int pointSize) {
            GL.PointSize(pointSize);
            foreach (var subset in GetSubsets(BrushWireframe)) {
                Render(context, PrimitiveType.Points, subset);
            }

        }

        public void UpdatePartial(IEnumerable<MapObject> objects) {
            UpdatePartial(objects.OfType<Solid>().SelectMany(x => x.Faces));
            UpdatePartial(objects.OfType<Entity>().Where(x => !x.HasChildren));
        }

        public void UpdatePartial(IEnumerable<Face> faces) {
            foreach (var face in faces) {
                var offset = GetOffset(face);
                if (offset < 0) continue;
                var conversion = Convert(face);
                Update(offset, conversion);
            }
        }

        public void UpdatePartial(IEnumerable<Entity> entities) {
            foreach (var entity in entities) {
                var offset = GetOffset(entity);
                if (offset < 0) continue;
                var conversion = entity.GetBoxFaces().SelectMany(Convert);
                Update(offset, conversion);
            }
        }

        protected override void CreateArray(IEnumerable<MapObject> objects) {
            var obj = objects.Where(x => !x.IsVisgroupHidden && !x.IsCodeHidden).ToList();
            var faces = obj.OfType<Solid>().SelectMany(x => x.Faces).ToList();
            var entities = obj.OfType<Entity>().Where(x => !x.HasChildren).ToList();

            StartSubset(BrushWireframe);

            // Render solids
            foreach (var group in faces.GroupBy(x => new { x.Texture.Texture, Transparent = HasTransparency(x) })) {
                var subset = group.Key.Transparent ? Transparent : Textured;
                if (!group.Key.Transparent) StartSubset(subset);

                foreach (var face in group) {
                    if (group.Key.Transparent) StartSubset(subset);

                    PushOffset(face);
                    var index = PushData(Convert(face));
                    if (!face.Parent.IsRenderHidden3D && face.Opacity > 0) PushIndex(subset, index, face.GetTriangleIndices());
                    if (!face.Parent.IsRenderHidden2D) PushIndex(BrushWireframe, index, face.GetLineIndices());

                    if (group.Key.Transparent) PushSubset(subset, face);
                }

                if (!group.Key.Transparent) PushSubset(subset, group.Key.Texture);
            }

            PushSubset(BrushWireframe, (object)null);
            StartSubset(EntityWireframe);

            // Render entities
            foreach (var g in entities.GroupBy(x => x.HasModel())) {
                // key = false -> no model, put in the untextured group
                // key = true  -> model, put in the entity group
                if (!g.Key) StartSubset(Textured);
                foreach (var entity in g) {
                    if (g.Key) StartSubset(Textured);
                    PushOffset(entity);
                    foreach (var face in entity.GetBoxFaces()) {
                        var index = PushData(Convert(face));
                        if (!face.Parent.IsRenderHidden3D) PushIndex(Textured, index, face.GetTriangleIndices());
                        if (!face.Parent.IsRenderHidden2D) PushIndex(EntityWireframe, index, face.GetLineIndices());
                    }
                    if (g.Key) PushSubset(Textured, entity);
                }
                if (!g.Key) PushSubset(Textured, (ITexture)null);
            }

            PushSubset(EntityWireframe, (object)null);
        }

        private bool HasTransparency(Face face) {
            return face.Opacity < 0.95
                   || (face.Texture.Texture != null && face.Texture.Texture.HasTransparency());
        }

        protected IEnumerable<MapObjectVertex> Convert(Face face) {
            float nx = (float)face.Plane.Normal.DX,
              ny = (float)face.Plane.Normal.DY,
              nz = (float)face.Plane.Normal.DZ;
            float r = face.Colour.R / 255f,
                  g = face.Colour.G / 255f,
                  b = face.Colour.B / 255f,
                  a = face.Opacity;
            var lmed = face.GetIndexedVertices().Where(v => v.LMU != 0).ToList();
            return face.GetIndexedVertices().Select(vert => new MapObjectVertex {
                Position = new Vector3((float)vert.Location.DX, (float)vert.Location.DY, (float)vert.Location.DZ),
                Normal = new Vector3(nx, ny, nz),
                Texture = new Vector2((float)vert.TextureU, (float)vert.TextureV),
                LightmapUv = new Vector2(vert.LMU, vert.LMV),
                Colour = new Color4(r, g, b, a),
                IsSelected = face.IsSelected || (face.Parent != null && face.Parent.IsSelected) ? 1 : 0
            });
        }
    }
}
