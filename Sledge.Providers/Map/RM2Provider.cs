using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using Sledge.DataStructures.Geometric;
using Sledge.DataStructures.MapObjects;
using System.Runtime.InteropServices;
using System.Threading;

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


        private const int downscaleFactor = 10;

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
            
            coplanarFaces.Sort((x, y) =>
            {
                if (x == y) return 0;

                if (GetGroupTextureWidth(x) < GetGroupTextureWidth(y)) return 1;
                return -1;
            });

            int writeX = 0; int writeY = 0; int writeMaxX = 0;

            var buffer = new byte[bitmap.Width * bitmap.Height * Bitmap.GetPixelFormatSize(PixelFormat.Format24bppRgb) / 8];

            List<Thread> threads = new List<Thread>();

            List<Entity> lightEntities = map.WorldSpawn.Find(q => q.ClassName == "light").OfType<Entity>().ToList();

            List<Face> allFaces = coplanarFaces.Select(q => q.Faces).SelectMany(q => q).ToList();

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
                    Thread newThread = CreateLightmapRenderThread(buffer, lightEntities, uAxis, vAxis, writeX, writeY, minTotalX.Value, minTotalY.Value, face, allFaces);
                    threads.Add(newThread);
                }
                
                writeY += (int)(maxTotalY - minTotalY)/downscaleFactor + 3;
                if ((int)(maxTotalX - minTotalX)/downscaleFactor + 3 > writeMaxX) writeMaxX = (int)(maxTotalX - minTotalX) / downscaleFactor + 3;
            }

            int a = 0;
            while (threads.Count > 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (i >= threads.Count) break;
                    if (threads[i].ThreadState == ThreadState.Unstarted)
                    {
                        threads[i].Start();
                    }
                    else if (threads[i].ThreadState == ThreadState.Stopped)
                    {
                        threads.RemoveAt(i);
                        i--;
                    }
                }
                a++; Thread.Sleep(100);

                if (a>=10)
                {
                    a -= 10;

                    BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, 2048, 2048), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    Marshal.Copy(buffer, 0, bitmapData.Scan0, buffer.Length);
                    bitmap.UnlockBits(bitmapData);

                    try
                    {
                        bitmap.Save("D:/repos/asd.bmp");
                    }
                    catch
                    {
                        //i don't care about this exception
                    }
                }
            }

        }

        private static Thread CreateLightmapRenderThread(byte[] bitmapData, List<Entity> lights, Coordinate uAxis, Coordinate vAxis, int writeX, int writeY, decimal minTotalX, decimal minTotalY, Face targetFace, List<Face> blockerFaces)
        {
            return new Thread(() => RenderLightOntoFace(bitmapData, lights, uAxis, vAxis, writeX, writeY, minTotalX, minTotalY, targetFace, blockerFaces));
        }

        private static void RenderLightOntoFace(byte[] bitmapData, List<Entity> lights, Coordinate uAxis, Coordinate vAxis, int writeX, int writeY, decimal minTotalX, decimal minTotalY, Face targetFace,List<Face> blockerFaces)
        {
            decimal? minX = null; decimal? maxX = null;
            decimal? minY = null; decimal? maxY = null;

            foreach (Coordinate coord in targetFace.Vertices.Select(x => x.Location))
            {
                decimal x = coord.Dot(uAxis);
                decimal y = coord.Dot(vAxis);

                if (minX == null || x < minX) minX = x;
                if (minY == null || y < minY) minY = y;
                if (maxX == null || x > maxX) maxX = x;
                if (maxY == null || y > maxY) maxY = y;
            }

            decimal centerX = (maxX.Value + minX.Value) / 2;
            decimal centerY = (maxY.Value + minY.Value) / 2;

            for (int y = 0; y < (maxY - minY) / downscaleFactor; y++)
            {
                for (int x = 0; x < (maxX - minX) / downscaleFactor; x++)
                {
                    decimal ttX = minX.Value + (x * downscaleFactor);
                    decimal ttY = minY.Value + (y * downscaleFactor);
                    Coordinate pointOnPlane = (ttX - centerX) * uAxis + (ttY - centerY) * vAxis + targetFace.BoundingBox.Center;
                    
                    Color luxelColor = Color.Black;

                    foreach (Entity light in lights)
                    {
                        Coordinate lightPos = light.Origin;
                        decimal lightRange = decimal.Parse(light.EntityData.GetPropertyValue("range"));
                        Coordinate lightColor = light.EntityData.GetPropertyCoordinate("color", new Coordinate(255, 255, 255));

                        decimal dotToLight = (lightPos - pointOnPlane).Normalise().Dot(targetFace.Plane.Normal);
                        bool illuminated = false;
                        if (dotToLight > 0.0m && (pointOnPlane - lightPos).LengthSquared() < lightRange * lightRange)
                        {
                            Line lineTester = new Line(lightPos, pointOnPlane);
                            illuminated = true;
                            foreach (Face otherFace in blockerFaces)
                            {
                                Coordinate hit = otherFace.GetIntersectionPoint(lineTester);
                                if (hit != null && (hit - pointOnPlane).LengthSquared() > 5.0m)
                                {
                                    illuminated = false;
                                    break;
                                }
                            }
                        }
                        
                        if (illuminated)
                        {
                            int r = luxelColor.R;
                            int g = luxelColor.G;
                            int b = luxelColor.B;

                            decimal brightness = dotToLight * (lightRange - (pointOnPlane - lightPos).VectorMagnitude()) / lightRange;

                            r += (int)(lightColor.X * brightness); if (r > 255) r = 255;
                            g += (int)(lightColor.Y * brightness); if (g > 255) g = 255;
                            b += (int)(lightColor.Z * brightness); if (b > 255) b = 255;

                            luxelColor = Color.FromArgb(r, g, b);
                        }
                    }

                    int tX = writeX + x + (int)(minX - minTotalX) / downscaleFactor;
                    int tY = writeY + y + (int)(minY - minTotalY) / downscaleFactor;
                    if (tX >= 0 && tY >= 0 && tX < 2048 && tY < 2048)
                    {
                        bitmapData[(tX + tY * 2048) * Bitmap.GetPixelFormatSize(PixelFormat.Format24bppRgb) / 8] = luxelColor.R;
                        bitmapData[(tX + tY * 2048) * Bitmap.GetPixelFormatSize(PixelFormat.Format24bppRgb) / 8 + 1] = luxelColor.G;
                        bitmapData[(tX + tY * 2048) * Bitmap.GetPixelFormatSize(PixelFormat.Format24bppRgb) / 8 + 2] = luxelColor.B;
                    }
                }
            }
        }
    }
}
