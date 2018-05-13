using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Sledge.DataStructures.Geometric;
using Sledge.DataStructures.MapObjects;

namespace Sledge.Providers.Map
{
    public class RM2Provider
    {
        private static Plane FindCoplanar(Dictionary<Plane, List<Face>> coplanarFaces, Face otherFace)
        {
            foreach (Plane plane in coplanarFaces.Keys)
            {
                if ((plane.Normal - otherFace.Plane.Normal).LengthSquared() < 0.3m)
                {
                    Plane plane2 = new Plane(otherFace.Plane.Normal, otherFace.Vertices[0].Location);
                    if (Math.Abs(plane2.EvalAtPoint(plane.PointOnPlane)) < 5.0m) return plane;
                }
            }
            return null;
        }

        public static void SaveToFile(string filename, Sledge.DataStructures.MapObjects.Map map)
        {
            Dictionary<Plane, List<Face>> coplanarFaces = new Dictionary<Plane, List<Face>>();

            //get faces
            foreach (Solid solid in map.WorldSpawn.Find(x => x is Solid).OfType<Solid>())
            {
                foreach (Face face in solid.Faces)
                {
                    if (face.Texture.Name.ToLower() == "tooltextures/remove_face") continue;
                    Plane plane = FindCoplanar(coplanarFaces, face);
                    List<Face> planeFaces;
                    if (plane == null)
                    {
                        planeFaces = new List<Face>();
                        plane = new Plane(face.Plane.Normal, face.Vertices[0].Location);
                        coplanarFaces.Add(plane, planeFaces);
                    }
                    else
                    {
                        planeFaces = coplanarFaces[plane];
                    }
                    planeFaces.Add(face);
                }
            }

            Random rand = new Random();

            //sort faces
            foreach (Plane plane in coplanarFaces.Keys)
            {
                var direction = plane.GetClosestAxisToNormal();

                var tempV = direction == Coordinate.UnitZ ? -Coordinate.UnitY : -Coordinate.UnitZ;
                var uAxis = plane.Normal.Cross(tempV).Normalise();
                var vAxis = uAxis.Cross(plane.Normal).Normalise();

                coplanarFaces[plane].Sort((x, y) =>
                {
                    var vDiff = x.BoundingBox.Center.Dot(vAxis) - y.BoundingBox.Center.Dot(vAxis);

                    if (Math.Abs(vDiff) < 0.0001m)
                    {
                        var uDiff = x.BoundingBox.Center.Dot(uAxis) - y.BoundingBox.Center.Dot(uAxis);
                        if (uDiff < 0.0m)
                        {
                            return -1;
                        }
                        else if (uDiff > 0.0m)
                        {
                            return 1;
                        }
                        return 0;
                    }

                    if (vDiff < 0.0m)
                    {
                        return -1;
                    }
                    else if (vDiff > 0.0m)
                    {
                        return 1;
                    }
                    return 0;
                });

                System.Drawing.Color color = System.Drawing.Color.FromArgb(rand.Next() % 256, rand.Next() % 256, rand.Next() % 256);

                foreach (Face face in coplanarFaces[plane])
                {
                    face.Colour = color;
                }
            }
            
            //put the faces into a file
            Bitmap bitmap = new Bitmap(2048, 2048, PixelFormat.Format24bppRgb);

            const int downscaleFactor = 16;

            //BitmapData data = bitmap.LockBits(new Rectangle(0, 0, 2048, 2048), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            int writeX = 0; int writeY = 0; int writeMaxY = 0;
            foreach (Plane plane in coplanarFaces.Keys)
            {
                var direction = plane.GetClosestAxisToNormal();

                var tempV = direction == Coordinate.UnitZ ? -Coordinate.UnitY : -Coordinate.UnitZ;
                var uAxis = plane.Normal.Cross(tempV).Normalise();
                var vAxis = uAxis.Cross(plane.Normal).Normalise();

                decimal? minTotalX = null; decimal? maxTotalX = null;
                decimal? minTotalY = null; decimal? maxTotalY = null;

                foreach (Face face in coplanarFaces[plane])
                {
                    foreach (Coordinate coord in face.Vertices.Select(x => x.Location))
                    {
                        decimal x = coord.Dot(uAxis);
                        decimal y = coord.Dot(vAxis);

                        if (minTotalX == null || x < minTotalX) minTotalX = x;
                        if (minTotalY == null || y < minTotalY) minTotalY = y;
                        if (maxTotalX == null || x > maxTotalX) maxTotalX = x;
                        if (maxTotalY == null || y > maxTotalY) maxTotalY = y;
                    }
                }

                if (writeX + (int)(maxTotalX-minTotalX) / downscaleFactor + 1 >= 2048)
                {
                    writeX = 0;
                    writeY += writeMaxY;
                    writeMaxY = 0;
                }

                foreach (Face face in coplanarFaces[plane])
                {
                    decimal? minX = null; decimal? maxX = null;
                    decimal? minY = null; decimal? maxY = null;

                    foreach (Coordinate coord in face.Vertices.Select(x => x.Location))
                    {
                        decimal x = coord.Dot(uAxis);
                        decimal y = coord.Dot(vAxis);

                        if (minX == null || x < minX) minX = x;
                        if (minY == null || y < minY) minY = y;
                        if (maxX == null || x > maxX) maxX = x;
                        if (maxY == null || y > maxY) maxY = y;
                    }
                    
                    for (int y = 0; y < (maxY - minY) / downscaleFactor; y++)
                    {
                        for (int x = 0; x < (maxX - minX) / downscaleFactor; x++)
                        {
                            int tX = writeX + x + (int)(minX - minTotalX) / downscaleFactor;
                            int tY = writeY + y + (int)(minY - minTotalY) / downscaleFactor;
                            if (tX>=0 && tY>=0 && tX < 2048 && tY < 2048)
                            {
                                bitmap.SetPixel(tX, tY, face.Colour);
                            }
                        }
                    }
                }

                writeX += (int)(maxTotalX - minTotalX)/downscaleFactor + 1;
                if ((int)(maxTotalY - minTotalY)/downscaleFactor + 1 > writeMaxY) writeMaxY = (int)(maxTotalY - minTotalY) / downscaleFactor + 1;
            }
        }
    }
}
