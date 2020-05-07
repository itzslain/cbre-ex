using Sledge.DataStructures.Geometric;
using Sledge.DataStructures.MapObjects;
using Sledge.DataStructures.Transformations;
using Sledge.Extensions;
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

                    Coordinate angles = x.EntityData.GetPropertyCoordinate("angles");

                    Matrix pitch = Matrix.Rotation(Quaternion.EulerAngles(DMath.DegreesToRadians(angles.X), 0, 0));
                    Matrix yaw = Matrix.Rotation(Quaternion.EulerAngles(0, 0, -DMath.DegreesToRadians(angles.Y)));
                    Matrix roll = Matrix.Rotation(Quaternion.EulerAngles(0, DMath.DegreesToRadians(angles.Z), 0));

                    var m = new UnitMatrixMult(yaw * roll * pitch);

                    light.Direction = new CoordinateF(m.Transform(Coordinate.UnitY));
                    //TODO: make sure this matches 3dws

                    return light;
                }));
        }
    }
}
