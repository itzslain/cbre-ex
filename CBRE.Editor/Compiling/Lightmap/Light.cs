using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;
using CBRE.DataStructures.Transformations;
using CBRE.Extensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace CBRE.Editor.Compiling.Lightmap
{
    class Light
    {
        public CoordinateF Color;
        public float Intensity;
        public bool HasSprite;
        public CoordinateF Origin;
        public float Range;

        public CoordinateF Direction;
        public float? innerCos;
        public float? outerCos;

        public static void FindLights(Map map, out List<Light> lightEntities)
        {
            lightEntities = new List<Light>();
            lightEntities.AddRange(map.WorldSpawn.Find(q => q.ClassName == "light").OfType<Entity>()
                .Select(x =>
                {
                    float range = 100.0f;
                    float.TryParse(x.EntityData.GetPropertyValue("range"), NumberStyles.Float, CultureInfo.InvariantCulture, out range);
                    float intensity = 1.0f;
                    float.TryParse(x.EntityData.GetPropertyValue("intensity"), NumberStyles.Float, CultureInfo.InvariantCulture, out intensity);
                    bool hasSprite = true;
                    bool.TryParse(x.EntityData.GetPropertyValue("hassprite") ?? "true", out hasSprite);

                    return new Light()
                    {
                        Origin = new CoordinateF(x.Origin),
                        Range = range,
                        Color = new CoordinateF(x.EntityData.GetPropertyCoordinate("color")),
                        Intensity = intensity,
                        HasSprite = hasSprite,
                        Direction = null,
                        innerCos = null,
                        outerCos = null
                    };
                }));
            lightEntities.AddRange(map.WorldSpawn.Find(q => q.ClassName == "spotlight").OfType<Entity>()
                .Select(x =>
                {
                    float range = 100.0f;
                    float.TryParse(x.EntityData.GetPropertyValue("range"), NumberStyles.Float, CultureInfo.InvariantCulture, out range);
                    float intensity = 1.0f;
                    float.TryParse(x.EntityData.GetPropertyValue("intensity"), NumberStyles.Float, CultureInfo.InvariantCulture, out intensity);
                    bool hasSprite = true;
                    bool.TryParse(x.EntityData.GetPropertyValue("hassprite") ?? "true", out hasSprite);
                    float innerCos = 0.5f;
                    float.TryParse(x.EntityData.GetPropertyValue("innerconeangle"), NumberStyles.Float, CultureInfo.InvariantCulture, out innerCos);
                    innerCos = (float)Math.Cos(innerCos*(float)Math.PI / 180.0f);
                    float outerCos = 0.75f;
                    float.TryParse(x.EntityData.GetPropertyValue("outerconeangle"), NumberStyles.Float, CultureInfo.InvariantCulture, out outerCos);
                    outerCos = (float)Math.Cos(outerCos * (float)Math.PI / 180.0f);

                    Light light = new Light()
                    {
                        Origin = new CoordinateF(x.Origin),
                        Range = range,
                        Color = new CoordinateF(x.EntityData.GetPropertyCoordinate("color")),
                        Intensity = intensity,
                        HasSprite = hasSprite,
                        Direction = null,
                        innerCos = innerCos,
                        outerCos = outerCos
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
