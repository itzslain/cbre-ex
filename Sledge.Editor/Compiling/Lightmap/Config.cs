using System.Drawing;
using Sledge.DataStructures.Geometric;

namespace Sledge.Editor.Compiling.Lightmap
{
    static class Config
    {
        public static int DownscaleFactor = 10;
        public static int PlaneMargin = 5;
        public static int TextureDims = 2048;
        public static int BlurRadius = 2;

        public static Color AmbientColor = Color.FromArgb(30, 30, 30);
        public static CoordinateF AmbientNormal = new CoordinateF(1.0f, 2.0f, 3.0f).Normalise();
    }
}
