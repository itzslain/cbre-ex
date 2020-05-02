using Sledge.DataStructures.Geometric;

namespace Sledge.Editor.Compiling.Lightmap
{
    class Light
    {
        public CoordinateF Color;
        public CoordinateF Origin;
        public float Range;

        public CoordinateF Direction;
        public float? innerCos;
        public float? outerCos;
    }
}
