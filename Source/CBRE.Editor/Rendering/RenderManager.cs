using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Documents;
using CBRE.Editor.Rendering.Renderers;
using CBRE.UI;
using OpenTK;
using System;
using System.Collections.Generic;

namespace CBRE.Editor.Rendering
{
    public class RenderManager : IDisposable
    {
        private readonly Document _document;
        private readonly IRenderer _renderer;

        public RenderManager(Document document)
        {
            _document = document;
            _renderer = new ModernRenderer(_document);
        }

        public void Dispose()
        {
            _renderer.Dispose();
        }

        public void UpdateGrid(decimal gridSpacing, bool showIn2D, bool showIn3D, bool force)
        {
            _renderer.UpdateGrid(gridSpacing, showIn2D, showIn3D, force);
        }

        public void SetSelectionTransform(Matrix4 selectionTransform)
        {
            _renderer.SetSelectionTransform(selectionTransform);
        }

        public void Draw2D(ViewportBase context, Matrix4 viewport, Matrix4 camera, Matrix4 modelView)
        {
            _renderer.Draw2D(context, viewport, camera, modelView);
        }

        public void Draw3D(ViewportBase context, Matrix4 viewport, Matrix4 camera, Matrix4 modelView)
        {
            _renderer.Draw3D(context, viewport, camera, modelView);
        }

        public void Update()
        {
            _renderer.Update();
        }

        public void UpdateSelection(IEnumerable<MapObject> objects)
        {
            _renderer.UpdateSelection(objects);
        }

        public void UpdatePartial(IEnumerable<MapObject> objects)
        {
            _renderer.UpdatePartial(objects);
        }

        public void UpdatePartial(IEnumerable<Face> faces)
        {
            _renderer.UpdatePartial(faces);
        }

        public void UpdateDocumentToggles()
        {
            _renderer.UpdateDocumentToggles();
        }

        public void Register(IEnumerable<ViewportBase> viewports)
        {
            foreach (ViewportBase vp in viewports)
            {
                vp.RenderContext.Add(new RenderManagerRenderable(vp, this));
            }
        }
    }
}
