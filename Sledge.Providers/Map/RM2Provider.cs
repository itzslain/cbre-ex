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
        private class LightmapGroup
        {
            public Plane Plane;
            public Box BoundingBox;
            public List<Face> Faces;
        }

        private static decimal GetGroupTextureWidth(LightmapGroup group)
        {
            var direction = group.Plane.GetClosestAxisToNormal();

            var tempV = direction == Coordinate.UnitZ ? -Coordinate.UnitY : -Coordinate.UnitZ;
            var uAxis = group.Plane.Normal.Cross(tempV).Normalise();
            var vAxis = uAxis.Cross(group.Plane.Normal).Normalise();

            decimal? minTotalX = null; decimal? maxTotalX = null;
            decimal? minTotalY = null; decimal? maxTotalY = null;

            foreach (Face face in group.Faces)
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

            if ((maxTotalX - minTotalX) < (maxTotalY - minTotalY))
            {
                decimal maxSwap = maxTotalX.Value; decimal minSwap = minTotalX.Value;
                maxTotalX = maxTotalY; minTotalX = minTotalY;
                maxTotalY = maxSwap; minTotalY = minSwap;

                Coordinate swapAxis = uAxis;
                uAxis = vAxis;
                vAxis = swapAxis;
            }

            return (maxTotalY - minTotalY).Value;
        }

        private static LightmapGroup FindCoplanar(List<LightmapGroup> coplanarFaces, Face otherFace)
        {
            foreach (LightmapGroup group in coplanarFaces)
            {
                if ((group.Plane.Normal - otherFace.Plane.Normal).LengthSquared() < 0.1m)
                {
                    Plane plane2 = new Plane(otherFace.Plane.Normal, otherFace.Vertices[0].Location);
                    if (Math.Abs(plane2.EvalAtPoint((group.Plane.PointOnPlane))) > 4.0m) continue;
                    Box faceBox = new Box(otherFace.BoundingBox.Start - new Coordinate(3, 3, 3), otherFace.BoundingBox.End + new Coordinate(3, 3, 3));
                    if (faceBox.IntersectsWith(group.BoundingBox)) return group;
                }
            }
            return null;
        }

        public static void SaveToFile(string filename, Sledge.DataStructures.MapObjects.Map map)
        {
            List<LightmapGroup> coplanarFaces = new List<LightmapGroup>();

            //get faces
            foreach (Solid solid in map.WorldSpawn.Find(x => x is Solid).OfType<Solid>())
            {
                foreach (Face face in solid.Faces)
                {
                    face.UpdateBoundingBox();
                    if (face.Texture.Name.ToLower() == "tooltextures/remove_face") continue;
                    LightmapGroup group = FindCoplanar(coplanarFaces, face);
                    Box faceBox = new Box(face.BoundingBox.Start - new Coordinate(3, 3, 3), face.BoundingBox.End + new Coordinate(3, 3, 3));
                    if (group == null)
                    {
                        group = new LightmapGroup();
                        group.BoundingBox = faceBox;
                        group.Faces = new List<Face>();
                        group.Plane = new Plane(face.Plane.Normal,face.Vertices[0].Location);
                        coplanarFaces.Add(group);
                    }
                    group.Faces.Add(face);
                    group.Plane = new Plane(group.Plane.Normal, (face.Vertices[0].Location+group.Plane.PointOnPlane)/2);
                    group.BoundingBox = new Box(new Box[] { group.BoundingBox, faceBox });
                }
            }

            for (int i=0;i<coplanarFaces.Count;i++)
            {
                for (int j=i+1;j<coplanarFaces.Count;j++)
                {
                    if ((coplanarFaces[i].Plane.Normal - coplanarFaces[j].Plane.Normal).LengthSquared() < 0.1m &&
                        coplanarFaces[i].BoundingBox.IntersectsWith(coplanarFaces[j].BoundingBox))
                    {
                        coplanarFaces[i].Faces.AddRange(coplanarFaces[j].Faces);
                        coplanarFaces[i].BoundingBox = new Box(new Box[] { coplanarFaces[i].BoundingBox, coplanarFaces[j].BoundingBox });
                        coplanarFaces.RemoveAt(j);
                        j = i+1;
                    }
                }
            }

            Random rand = new Random();

            //sort faces
            foreach (LightmapGroup group in coplanarFaces)
            {
                var direction = group.Plane.GetClosestAxisToNormal();

                var tempV = direction == Coordinate.UnitZ ? -Coordinate.UnitY : -Coordinate.UnitZ;
                var uAxis = group.Plane.Normal.Cross(tempV).Normalise();
                var vAxis = uAxis.Cross(group.Plane.Normal).Normalise();
                
                System.Drawing.Color color = System.Drawing.Color.FromArgb(rand.Next() % 60+20, rand.Next() % 60+20, rand.Next() % 60+20);

                foreach (Face face in group.Faces)
                {
                    face.Colour = color;
                }
            }
            
            //put the faces into a file
            Bitmap bitmap = new Bitmap(2048, 2048, PixelFormat.Format24bppRgb);

            const int downscaleFactor = 10;

            //BitmapData data = bitmap.LockBits(new Rectangle(0, 0, 2048, 2048), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
            
            coplanarFaces.Sort((x, y) =>
            {
                if (x == y) return 0;

                if (GetGroupTextureWidth(x) < GetGroupTextureWidth(y)) return 1;
                return -1;
            });

            int writeX = 0; int writeY = 0; int writeMaxX = 0;
            foreach (LightmapGroup group in coplanarFaces)
            {
                var direction = group.Plane.GetClosestAxisToNormal();

                var tempV = direction == Coordinate.UnitZ ? -Coordinate.UnitY : -Coordinate.UnitZ;
                var uAxis = group.Plane.Normal.Cross(tempV).Normalise();
                var vAxis = uAxis.Cross(group.Plane.Normal).Normalise();

                decimal? minTotalX = null; decimal? maxTotalX = null;
                decimal? minTotalY = null; decimal? maxTotalY = null;

                foreach (Face face in group.Faces)
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

                if ((maxTotalX-minTotalX)>(maxTotalY-minTotalY))
                {
                    decimal maxSwap = maxTotalX.Value; decimal minSwap = minTotalX.Value;
                    maxTotalX = maxTotalY; minTotalX = minTotalY;
                    maxTotalY = maxSwap; minTotalY = minSwap;

                    Coordinate swapAxis = uAxis;
                    uAxis = vAxis;
                    vAxis = swapAxis;
                }

                if (writeY + (int)(maxTotalY-minTotalY) / downscaleFactor + 3 >= 2048)
                {
                    writeY = 0;
                    writeX += writeMaxX;
                    writeMaxX = 0;
                }

                foreach (Face face in group.Faces)
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

                writeY += (int)(maxTotalY - minTotalY)/downscaleFactor + 3;
                if ((int)(maxTotalX - minTotalX)/downscaleFactor + 3 > writeMaxX) writeMaxX = (int)(maxTotalX - minTotalX) / downscaleFactor + 3;
            }
            bitmap.Save("D:/repos/asd.bmp");
        }
    }
}
