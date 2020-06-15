using OpenTK;
using System.Drawing;

namespace CBRE.Graphics
{
    public struct Vertex
    {
        public Color Color;
        public Vector2d TexCoord;
        public Vector2d LightmapCoord;
        public Vector3d Normal;
        public Vector3d Position;
    }
}
