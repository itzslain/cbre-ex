using Sledge.DataStructures.Geometric;
using Sledge.DataStructures.MapObjects;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static void FindLights(Map map, out List<Light> lightEntities)
        {
            lightEntities = new List<Light>();
            lightEntities.AddRange(map.WorldSpawn.Find(q => q.ClassName == "light").OfType<Entity>()
                .Select(x => new Light()
                {
                    Origin = new CoordinateF(x.Origin),
                    Range = float.Parse(x.EntityData.GetPropertyValue("range")),
                    Color = new CoordinateF(x.EntityData.GetPropertyCoordinate("color")),
                    Direction = null,
                    innerCos = null,
                    outerCos = null
                }));
            lightEntities.AddRange(map.WorldSpawn.Find(q => q.ClassName == "spotlight").OfType<Entity>()
                .Select(x =>
                {
                    Light light = new Light()
                    {
                        Origin = new CoordinateF(x.Origin),
                        Range = float.Parse(x.EntityData.GetPropertyValue("range")),
                        Color = new CoordinateF(x.EntityData.GetPropertyCoordinate("color")),
                        Direction = null,
                        innerCos = (float)Math.Cos(float.Parse(x.EntityData.GetPropertyValue("innerconeangle")) * (float)Math.PI / 360.0f),
                        outerCos = (float)Math.Cos(float.Parse(x.EntityData.GetPropertyValue("outerconeangle")) * (float)Math.PI / 360.0f)
                    };

                    CoordinateF eulerAngles = new CoordinateF(x.EntityData.GetPropertyCoordinate("angles"));
                    eulerAngles.X *= (float)Math.PI / 180.0f;
                    eulerAngles.Y *= (float)Math.PI / 180.0f;
                    eulerAngles.Z *= (float)Math.PI / 180.0f;

                    float swap = eulerAngles.Z;
                    eulerAngles.Z = eulerAngles.Y; eulerAngles.Y = swap;
                    swap = eulerAngles.Y;
                    eulerAngles.Y = eulerAngles.X; eulerAngles.X = swap;

                    MatrixF rot = MatrixF.Rotation(QuaternionF.EulerAngles(eulerAngles));
                    light.Direction = (new CoordinateF(0.0f, -1.0f, 0.0f)) * rot;
                    //TODO: make sure this matches 3dws

                    return light;
                }));
        }
    }
}
