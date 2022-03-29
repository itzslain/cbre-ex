using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace CBRE.Graphics
{
    public static class GLX
    {
        public static void Square(Vector2d origin, double radius, double currentZoom = 1, bool loop = false)
        {
            radius /= currentZoom;
            Vector2d[] points = new[]
                             {
                                 new Vector2d(origin.X - radius, origin.Y - radius),
                                 new Vector2d(origin.X + radius, origin.Y - radius),
                                 new Vector2d(origin.X + radius, origin.Y + radius),
                                 new Vector2d(origin.X - radius, origin.Y + radius)
                             };
            for (int i = 0; i < points.Length; i++)
            {
                GL.Vertex2(points[i]);
                if (!loop) GL.Vertex2(points[(i + 1) % points.Length]);
            }
        }

        public static void Circle(Vector2d origin, double radius, double currentZoom = 1, int sides = 0, bool loop = false)
        {
            if (sides < 3)
            {
                if (radius < 20) sides = 16;
                else if (radius < 50) sides = 24;
                else if (radius < 100) sides = 42;
                else if (radius < 300) sides = 64;
                else sides = 128;
            }
            radius /= currentZoom;
            double diff = (2 * Math.PI) / sides;
            Vector2d last = new Vector2d(0, 0);
            for (int i = 0; i < sides; i++)
            {
                double deg = diff * i;
                Vector2d point = new Vector2d(origin.X + Math.Cos(deg) * radius, origin.Y + Math.Sin(deg) * radius);
                if (i > 0 || loop)
                {
                    if (!loop) GL.Vertex2(last);
                    GL.Vertex2(point);
                }
                last = point;
            }
            if (loop) return;
            GL.Vertex2(last);
            GL.Vertex2(origin.X + radius, origin.Y);
        }
    }
}
