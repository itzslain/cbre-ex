using CBRE.DataStructures;
using CBRE.DataStructures.Geometric;
using CBRE.Editor.Documents;
using CBRE.Editor.Extensions;
using CBRE.Extensions;
using CBRE.Graphics;
using CBRE.Settings;
using CBRE.UI;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace CBRE.Editor.Tools.Widgets
{
    public class RotationWidget : Widget
    {
        private enum CircleType
        {
            None,
            Outer,
            X,
            Y,
            Z
        }

        public RotationWidget(Document document)
        {
            Document = document;
        }

        private class CachedLines
        {
            public int Width { get; set; }
            public int Height { get; set; }
            public Coordinate CameraLocation { get; set; }
            public Coordinate CameraLookAt { get; set; }
            public Coordinate PivotPoint { get; set; }
            public Viewport3D Viewport3D { get; set; }
            public Dictionary<CircleType, List<Line>> Cache { get; set; }

            public CachedLines(Viewport3D viewport3D)
            {
                Viewport3D = viewport3D;
                Cache = new Dictionary<CircleType, List<Line>>
                {
                    {CircleType.Outer, new List<Line>()},
                    {CircleType.X, new List<Line>()},
                    {CircleType.Y, new List<Line>()},
                    {CircleType.Z, new List<Line>()}
                };
            }
        }

        private readonly List<CachedLines> _cachedLines = new List<CachedLines>();

        private bool _autoPivot = true;
        private bool _movingPivot = false;

        private Coordinate _pivotPoint = Coordinate.Zero;
        private CircleType _mouseOver;
        private CircleType _mouseDown;
        private Coordinate _mouseDownPoint;
        private Coordinate _mouseMovePoint;

        public Coordinate GetPivotPoint()
        {
            return _pivotPoint;
        }

        public override void SelectionChanged()
        {
            if (Document.Selection.IsEmpty()) _autoPivot = true;
            if (!_autoPivot) return;

            Box bb = Document.Selection.GetSelectionBoundingBox();
            _pivotPoint = bb == null ? Coordinate.Zero : bb.Center;
        }

        #region Line cache

        private void AddLine(CircleType type, Coordinate start, Coordinate end, Plane test, CachedLines cache)
        {
            Line line = new Line(start, end);
            PlaneClassification cls = line.ClassifyAgainstPlane(test);
            if (cls == PlaneClassification.Back) return;
            if (cls == PlaneClassification.Spanning)
            {
                Coordinate isect = test.GetIntersectionPoint(line, true);
                Coordinate first = test.OnPlane(line.Start) > 0 ? line.Start : line.End;
                line = new Line(first, isect);
            }
            cache.Cache[type].Add(new Line(cache.Viewport3D.WorldToScreen(line.Start), cache.Viewport3D.WorldToScreen(line.End)));
        }

        private void UpdateCache(Viewport3D viewport, Document document)
        {
            Coordinate ccl = new Coordinate((decimal)viewport.Camera.Location.X, (decimal)viewport.Camera.Location.Y, (decimal)viewport.Camera.Location.Z);
            Coordinate ccla = new Coordinate((decimal)viewport.Camera.LookAt.X, (decimal)viewport.Camera.LookAt.Y, (decimal)viewport.Camera.LookAt.Z);

            CachedLines cache = _cachedLines.FirstOrDefault(x => x.Viewport3D == viewport);
            if (cache == null)
            {
                cache = new CachedLines(viewport);
                _cachedLines.Add(cache);
            }
            if (ccl == cache.CameraLocation && ccla == cache.CameraLookAt && cache.PivotPoint == _pivotPoint && cache.Width == viewport.Width && cache.Height == viewport.Height) return;

            Coordinate origin = _pivotPoint;
            decimal distance = (ccl - origin).VectorMagnitude();

            if (distance <= 1) return;

            cache.CameraLocation = ccl;
            cache.CameraLookAt = ccla;
            cache.PivotPoint = _pivotPoint;
            cache.Width = viewport.Width;
            cache.Height = viewport.Height;

            Coordinate normal = (ccl - origin).Normalise();
            Coordinate right = normal.Cross(Coordinate.UnitZ).Normalise();
            Coordinate up = normal.Cross(right).Normalise();

            Plane plane = new Plane(normal, origin.Dot(normal));

            const decimal sides = 32;
            decimal diff = (2 * DMath.PI) / sides;

            decimal radius = 0.15m * distance;

            cache.Cache[CircleType.Outer].Clear();
            cache.Cache[CircleType.X].Clear();
            cache.Cache[CircleType.Y].Clear();
            cache.Cache[CircleType.Z].Clear();

            for (int i = 0; i < sides; i++)
            {
                decimal cos1 = DMath.Cos(diff * i);
                decimal sin1 = DMath.Sin(diff * i);
                decimal cos2 = DMath.Cos(diff * (i + 1));
                decimal sin2 = DMath.Sin(diff * (i + 1));

                // outer circle
                AddLine(CircleType.Outer,
                    origin + right * cos1 * radius * 1.2m + up * sin1 * radius * 1.2m,
                    origin + right * cos2 * radius * 1.2m + up * sin2 * radius * 1.2m,
                    plane, cache);

                cos1 *= radius;
                sin1 *= radius;
                cos2 *= radius;
                sin2 *= radius;

                // X/Y plane = Z axis
                AddLine(CircleType.Z,
                    origin + Coordinate.UnitX * cos1 + Coordinate.UnitY * sin1,
                    origin + Coordinate.UnitX * cos2 + Coordinate.UnitY * sin2,
                    plane, cache);

                // Y/Z plane = X axis
                AddLine(CircleType.X,
                    origin + Coordinate.UnitY * cos1 + Coordinate.UnitZ * sin1,
                    origin + Coordinate.UnitY * cos2 + Coordinate.UnitZ * sin2,
                    plane, cache);

                // X/Z plane = Y axis
                AddLine(CircleType.Y,
                    origin + Coordinate.UnitZ * cos1 + Coordinate.UnitX * sin1,
                    origin + Coordinate.UnitZ * cos2 + Coordinate.UnitX * sin2,
                    plane, cache);
            }
        }

        #endregion

        private Matrix4? GetTransformationMatrix(Viewport3D viewport)
        {
            if (_mouseMovePoint == null || _mouseDownPoint == null || _pivotPoint == null) return null;

            Coordinate originPoint = viewport.WorldToScreen(_pivotPoint);
            Coordinate origv = (_mouseDownPoint - originPoint).Normalise();
            Coordinate newv = (_mouseMovePoint - originPoint).Normalise();
            decimal angle = DMath.Acos(Math.Max(-1, Math.Min(1, origv.Dot(newv))));
            if ((origv.Cross(newv).Z < 0)) angle = 2 * DMath.PI - angle;

            bool shf = KeyboardState.Shift;
            RotationStyle def = Select.RotationStyle;
            bool snap = (def == RotationStyle.SnapOnShift && shf) || (def == RotationStyle.SnapOffShift && !shf);
            if (snap)
            {
                decimal deg = angle * (180 / DMath.PI);
                decimal rnd = Math.Round(deg / 15) * 15;
                angle = rnd * (DMath.PI / 180);
            }

            Vector3 axis;
            Vector3 dir = (viewport.Camera.Location - _pivotPoint.ToVector3()).Normalized();
            switch (_mouseDown)
            {
                case CircleType.Outer:
                    axis = dir;
                    break;
                case CircleType.X:
                    axis = Vector3.UnitX;
                    break;
                case CircleType.Y:
                    axis = Vector3.UnitY;
                    break;
                case CircleType.Z:
                    axis = Vector3.UnitZ;
                    break;
                default:
                    return null;
            }
            double dirAng = Math.Acos(Vector3.Dot(dir, axis)) * 180 / Math.PI;
            if (dirAng > 90) angle = -angle;

            Matrix4 rotm = Matrix4.CreateFromAxisAngle(axis, (float)angle);
            Matrix4 mov = Matrix4.CreateTranslation(-_pivotPoint.ToVector3());
            Matrix4 rot = Matrix4.Mult(mov, rotm);
            return Matrix4.Mult(rot, Matrix4.Invert(mov));
        }

        private bool MouseOver(CircleType type, ViewportEvent ev, Viewport3D viewport)
        {
            CachedLines cache = _cachedLines.FirstOrDefault(x => x.Viewport3D == viewport);
            if (cache == null) return false;
            List<Line> lines = cache.Cache[type];
            Coordinate point = new Coordinate(ev.X, viewport.Height - ev.Y, 0);
            return lines.Any(x => (x.ClosestPoint(point) - point).VectorMagnitude() <= 8);
        }

        private bool MouseOverPivot(Viewport2D vp, ViewportEvent e)
        {
            if (Document.Selection.IsEmpty()) return false;

            Coordinate pivot = vp.WorldToScreen(vp.Flatten(_pivotPoint));
            int x = e.X;
            int y = vp.Height - e.Y;
            return pivot.X > x - 8 && pivot.X < x + 8 &&
                   pivot.Y > y - 8 && pivot.Y < y + 8;
        }

        public override void MouseLeave(ViewportBase viewport, ViewportEvent e)
        {
            viewport.Cursor = Cursors.Default;
        }

        public override void MouseMove(ViewportBase viewport, ViewportEvent e)
        {
            if (viewport is Viewport2D)
            {
                Viewport2D vp2 = (Viewport2D)viewport;
                if (_movingPivot)
                {
                    Coordinate pp = SnapToSelection(vp2.ScreenToWorld(e.X, vp2.Height - e.Y), vp2);
                    _pivotPoint = vp2.GetUnusedCoordinate(_pivotPoint) + vp2.Expand(pp);
                    _autoPivot = false;
                    e.Handled = true;
                }
                else if (MouseOverPivot(vp2, e))
                {
                    vp2.Cursor = Cursors.Cross;
                    e.Handled = true;
                }
                else
                {
                    vp2.Cursor = Cursors.Default;
                }
                return;
            }

            Viewport3D vp = viewport as Viewport3D;
            if (vp == null || vp != _activeViewport) return;

            if (Document.Selection.IsEmpty() || !vp.IsUnlocked(this)) return;

            if (_mouseDown != CircleType.None)
            {
                _mouseMovePoint = new Coordinate(e.X, vp.Height - e.Y, 0);
                e.Handled = true;
                Matrix4? tform = GetTransformationMatrix(vp);
                OnTransforming(tform);
            }
            else
            {
                UpdateCache(vp, Document);

                if (MouseOver(CircleType.Z, e, vp)) _mouseOver = CircleType.Z;
                else if (MouseOver(CircleType.Y, e, vp)) _mouseOver = CircleType.Y;
                else if (MouseOver(CircleType.X, e, vp)) _mouseOver = CircleType.X;
                else if (MouseOver(CircleType.Outer, e, vp)) _mouseOver = CircleType.Outer;
                else _mouseOver = CircleType.None;
            }
        }

        public override void MouseDown(ViewportBase viewport, ViewportEvent ve)
        {
            if (viewport is Viewport2D)
            {
                Viewport2D vp2 = (Viewport2D)viewport;
                if (ve.Button == MouseButtons.Left && MouseOverPivot(vp2, ve))
                {
                    _movingPivot = true;
                    ve.Handled = true;
                }
                return;
            }

            Viewport3D vp = viewport as Viewport3D;
            if (vp == null || vp != _activeViewport) return;

            if (ve.Button != MouseButtons.Left || _mouseOver == CircleType.None) return;
            _mouseDown = _mouseOver;
            _mouseDownPoint = new Coordinate(ve.X, vp.Height - ve.Y, 0);
            _mouseMovePoint = null;
            ve.Handled = true;
            vp.AquireInputLock(this);
        }

        public override void MouseUp(ViewportBase viewport, ViewportEvent ve)
        {
            if (viewport is Viewport2D)
            {
                // var vp2 = (Viewport2D) viewport;
                if (_movingPivot && ve.Button == MouseButtons.Left)
                {
                    _movingPivot = false;
                    ve.Handled = true;
                }
                return;
            }

            Viewport3D vp = viewport as Viewport3D;
            if (vp == null || vp != _activeViewport) return;

            if (_mouseDown != CircleType.None && _mouseMovePoint != null) ve.Handled = true;

            Matrix4? transformation = GetTransformationMatrix(vp);
            OnTransformed(transformation);
            _mouseDown = CircleType.None;
            _mouseMovePoint = null;
            vp.ReleaseInputLock(this);
        }

        public override void MouseWheel(ViewportBase viewport, ViewportEvent ve)
        {
            if (viewport != _activeViewport) return;
            if (_mouseDown != CircleType.None) ve.Handled = true;
        }

        public override void Render(ViewportBase viewport)
        {
            if (Document.Selection.IsEmpty()) return;

            if (viewport is Viewport2D)
            {
                Render2D((Viewport2D)viewport);
                return;
            }

            Viewport3D vp = viewport as Viewport3D;
            if (vp == null) return;

            switch (_mouseMovePoint == null ? CircleType.None : _mouseDown)
            {
                case CircleType.None:
                    RenderCircleTypeNone(vp, Document);
                    break;
                case CircleType.Outer:
                case CircleType.X:
                case CircleType.Y:
                case CircleType.Z:
                    RenderAxisRotating(vp, Document);
                    break;
            }
        }

        private void Render2D(Viewport2D viewport)
        {
            Coordinate pp = viewport.Flatten(_pivotPoint);
            GL.Begin(PrimitiveType.Lines);
            GL.Color3(Color.Cyan);
            GLX.Circle(new Vector2d(pp.DX, pp.DY), 4, (double)viewport.Zoom);
            GL.Color3(Color.White);
            GLX.Circle(new Vector2d(pp.DX, pp.DY), 8, (double)viewport.Zoom);
            GL.End();
        }

        private void RenderAxisRotating(Viewport3D viewport, Document document)
        {
            Vector3 axis = Vector3.UnitX;
            Color c = Color.Red;

            if (_mouseDown == CircleType.Y)
            {
                axis = Vector3.UnitY;
                c = Color.Lime;
            }

            if (_mouseDown == CircleType.Z)
            {
                axis = Vector3.UnitZ;
                c = Color.Blue;
            }

            if (_mouseDown == CircleType.Outer)
            {
                Viewport3D vp3 = _activeViewport as Viewport3D;
                if (vp3 != null) axis = (vp3.Camera.LookAt - vp3.Camera.Location).Normalized();
                c = Color.White;
            }

            if (_activeViewport != viewport || _mouseDown != CircleType.Outer)
            {
                GL.Begin(PrimitiveType.Lines);

                Vector3 zero = new Vector3((float)_pivotPoint.DX, (float)_pivotPoint.DY, (float)_pivotPoint.DZ);

                GL.Color4(c);
                GL.Vertex3(zero - axis * 100000);
                GL.Vertex3(zero + axis * 100000);

                GL.End();
            }

            if (_activeViewport == viewport)
            {
                GL.Disable(EnableCap.DepthTest);
                GL.Enable(EnableCap.LineStipple);
                GL.LineStipple(5, 0xAAAA);
                GL.Begin(PrimitiveType.Lines);

                GL.Color4(Color.FromArgb(64, Color.Gray));
                GL.Vertex3(_pivotPoint.ToVector3());
                GL.Vertex3(viewport.ScreenToWorld(_mouseDownPoint).ToVector3());

                GL.Color4(Color.LightGray);
                GL.Vertex3(_pivotPoint.ToVector3());
                GL.Vertex3(viewport.ScreenToWorld(_mouseMovePoint).ToVector3());

                GL.End();
                GL.Disable(EnableCap.LineStipple);
                GL.Enable(EnableCap.DepthTest);
            }
        }

        private void RenderCircleTypeNone(Viewport3D viewport, Document document)
        {
            Coordinate center = _pivotPoint;
            Vector3 origin = new Vector3((float)center.DX, (float)center.DY, (float)center.DZ);
            float distance = (viewport.Camera.Location - origin).Length;

            if (distance <= 1) return;

            float radius = 0.15f * distance;

            Vector3 normal = Vector3.Subtract(viewport.Camera.Location, origin).Normalized();
            Vector3 right = Vector3.Cross(normal, Vector3.UnitZ).Normalized();
            Vector3 up = Vector3.Cross(normal, right).Normalized();


            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Texture2D);

            const int sides = 32;
            const float diff = (float)(2 * Math.PI) / sides;

            GL.Begin(PrimitiveType.Lines);
            for (int i = 0; i < sides; i++)
            {
                float cos1 = (float)Math.Cos(diff * i);
                float sin1 = (float)Math.Sin(diff * i);
                float cos2 = (float)Math.Cos(diff * (i + 1));
                float sin2 = (float)Math.Sin(diff * (i + 1));
                GL.Color4(Color.DarkGray);
                GL.Vertex3(origin + right * cos1 * radius + up * sin1 * radius);
                GL.Vertex3(origin + right * cos2 * radius + up * sin2 * radius);
                GL.Color4(_mouseOver == CircleType.Outer ? Color.White : Color.LightGray);
                GL.Vertex3(origin + right * cos1 * radius * 1.2f + up * sin1 * radius * 1.2f);
                GL.Vertex3(origin + right * cos2 * radius * 1.2f + up * sin2 * radius * 1.2f);
            }
            GL.End();

            GL.Enable(EnableCap.ClipPlane0);
            GL.ClipPlane(ClipPlaneName.ClipPlane0, new double[] { normal.X, normal.Y, normal.Z, -Vector3.Dot(origin, normal) });

            GL.LineWidth(2);
            GL.Begin(PrimitiveType.Lines);
            for (int i = 0; i < sides; i++)
            {
                float cos1 = (float)Math.Cos(diff * i) * radius;
                float sin1 = (float)Math.Sin(diff * i) * radius;
                float cos2 = (float)Math.Cos(diff * (i + 1)) * radius;
                float sin2 = (float)Math.Sin(diff * (i + 1)) * radius;

                GL.Color4(_mouseOver == CircleType.Z ? Color.Blue : Color.DarkBlue);
                GL.Vertex3(origin + Vector3.UnitX * cos1 + Vector3.UnitY * sin1);
                GL.Vertex3(origin + Vector3.UnitX * cos2 + Vector3.UnitY * sin2);

                GL.Color4(_mouseOver == CircleType.X ? Color.Red : Color.DarkRed);
                GL.Vertex3(origin + Vector3.UnitY * cos1 + Vector3.UnitZ * sin1);
                GL.Vertex3(origin + Vector3.UnitY * cos2 + Vector3.UnitZ * sin2);

                GL.Color4(_mouseOver == CircleType.Y ? Color.Lime : Color.LimeGreen);
                GL.Vertex3(origin + Vector3.UnitZ * cos1 + Vector3.UnitX * sin1);
                GL.Vertex3(origin + Vector3.UnitZ * cos2 + Vector3.UnitX * sin2);
            }
            GL.End();
            GL.LineWidth(1);

            GL.Disable(EnableCap.ClipPlane0);

            GL.Enable(EnableCap.DepthTest);
        }
    }
}
