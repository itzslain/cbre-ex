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
using Sledge.DataStructures.Transformations;
using System.IO;

namespace Sledge.Providers.Map
{
    public class RM2Provider
    {
        private class LMLight
        {
            public CoordinateF Color;
            public CoordinateF Origin;
            public float Range;
        }

        private class LMFace
        {
            public PlaneF Plane { get; set; }
            
            public List<CoordinateF> Vertices { get; set; }
            
            public BoxF BoundingBox { get; set; }

            public LMFace(Face face)
            {
                Plane = new PlaneF(face.Plane);

                Vertices = face.Vertices.Select(x => new CoordinateF(x.Location)).ToList();

                UpdateBoundingBox();
            }

            public virtual IEnumerable<LineF> GetLines()
            {
                return GetEdges();
            }

            public virtual IEnumerable<LineF> GetEdges()
            {
                for (var i = 0; i < Vertices.Count; i++)
                {
                    yield return new LineF(Vertices[i], Vertices[(i + 1) % Vertices.Count]);
                }
            }

            public virtual IEnumerable<CoordinateF> GetIndexedVertices()
            {
                return Vertices;
            }

            public virtual IEnumerable<uint> GetTriangleIndices()
            {
                for (uint i = 1; i < Vertices.Count - 1; i++)
                {
                    yield return 0;
                    yield return i;
                    yield return i + 1;
                }
            }

            public virtual IEnumerable<CoordinateF[]> GetTriangles()
            {
                for (var i = 1; i < Vertices.Count - 1; i++)
                {
                    yield return new[]
                                     {
                                     Vertices[0],
                                     Vertices[i],
                                     Vertices[i + 1]
                                 };
                }
            }
            
            public virtual void UpdateBoundingBox()
            {
                BoundingBox = new BoxF(Vertices);
            }
            
            /// <summary>
            /// Returns the point that this line intersects with this face.
            /// </summary>
            /// <param name="line">The intersection line</param>
            /// <returns>The point of intersection between the face and the line.
            /// Returns null if the line does not intersect this face.</returns>
            public virtual CoordinateF GetIntersectionPoint(LineF line)
            {
                return GetIntersectionPoint(Vertices, line);
            }

            /// <summary>
            /// Test all the edges of this face against a bounding box to see if they intersect.
            /// </summary>
            /// <param name="box">The box to intersect</param>
            /// <returns>True if one of the face's edges intersects with the box.</returns>
            public bool IntersectsWithLine(BoxF box)
            {
                // Shortcut through the bounding box to avoid the line computations if they aren't needed
                return BoundingBox.IntersectsWith(box) && GetLines().Any(box.IntersectsWith);
            }

            /// <summary>
            /// Test this face to see if the given bounding box intersects with it
            /// </summary>
            /// <param name="box">The box to test against</param>
            /// <returns>True if the box intersects</returns>
            public bool IntersectsWithBox(BoxF box)
            {
                var verts = Vertices.ToList();
                return box.GetBoxLines().Any(x => GetIntersectionPoint(verts, x, true) != null);
            }
            
            protected static CoordinateF GetIntersectionPoint(IList<CoordinateF> coordinates, LineF line, bool ignoreDirection = false)
            {
                var plane = new PlaneF(coordinates[0], coordinates[1], coordinates[2]);
                var intersect = plane.GetIntersectionPoint(line, ignoreDirection);
                if (intersect == null) return null;

                // http://paulbourke.net/geometry/insidepoly/

                // The angle sum will be 2 * PI if the point is inside the face
                double sum = 0;
                for (var i = 0; i < coordinates.Count; i++)
                {
                    var i1 = i;
                    var i2 = (i + 1) % coordinates.Count;

                    // Translate the vertices so that the intersect point is on the origin
                    var v1 = coordinates[i1] - intersect;
                    var v2 = coordinates[i2] - intersect;

                    var m1 = (double)v1.LengthSquared();
                    var m2 = (double)v2.LengthSquared();
                    var nom = m1 * m2;
                    if (nom < 0.00001d)
                    {
                        // intersection is at a vertex
                        return intersect;
                    }
                    nom = Math.Sqrt(nom);
                    sum += Math.Acos((double)(v1.Dot(v2)) / nom);
                }

                var delta = Math.Abs(sum - Math.PI * 2);
                return (delta < 0.001d) ? intersect : null;
            }
        }

        private class LightmapGroup
        {
            public PlaneF Plane;
            public BoxF BoundingBox;
            public List<LMFace> Faces;
        }

        private static float GetGroupTextureWidth(LightmapGroup group)
        {
            var direction = group.Plane.GetClosestAxisToNormal();

            var tempV = direction == CoordinateF.UnitZ ? -CoordinateF.UnitY : -CoordinateF.UnitZ;
            var uAxis = group.Plane.Normal.Cross(tempV).Normalise();
            var vAxis = uAxis.Cross(group.Plane.Normal).Normalise();

            float? minTotalX = null; float? maxTotalX = null;
            float? minTotalY = null; float? maxTotalY = null;

            foreach (LMFace face in group.Faces)
            {
                foreach (CoordinateF coord in face.Vertices)
                {
                    float x = coord.Dot(uAxis);
                    float y = coord.Dot(vAxis);

                    if (minTotalX == null || x < minTotalX) minTotalX = x;
                    if (minTotalY == null || y < minTotalY) minTotalY = y;
                    if (maxTotalX == null || x > maxTotalX) maxTotalX = x;
                    if (maxTotalY == null || y > maxTotalY) maxTotalY = y;
                }
            }

            if ((maxTotalX - minTotalX) < (maxTotalY - minTotalY))
            {
                float maxSwap = maxTotalX.Value; float minSwap = minTotalX.Value;
                maxTotalX = maxTotalY; minTotalX = minTotalY;
                maxTotalY = maxSwap; minTotalY = minSwap;

                CoordinateF swapAxis = uAxis;
                uAxis = vAxis;
                vAxis = swapAxis;
            }

            return (maxTotalY - minTotalY).Value;
        }

        private static LightmapGroup FindCoplanar(List<LightmapGroup> coplanarFaces, LMFace otherFace)
        {
            foreach (LightmapGroup group in coplanarFaces)
            {
                if ((group.Plane.Normal - otherFace.Plane.Normal).LengthSquared() < 0.1f)
                {
                    PlaneF plane2 = new PlaneF(otherFace.Plane.Normal, otherFace.Vertices[0]);
                    if (Math.Abs(plane2.EvalAtPoint((group.Plane.PointOnPlane))) > 4.0f) continue;
                    BoxF faceBox = new BoxF(otherFace.BoundingBox.Start - new CoordinateF(3, 3, 3), otherFace.BoundingBox.End + new CoordinateF(3, 3, 3));
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
                foreach (Face tface in solid.Faces)
                {
                    tface.UpdateBoundingBox();
                    if (tface.Texture.Name.ToLower() == "tooltextures/remove_face") continue;
                    LMFace face = new LMFace(tface);
                    LightmapGroup group = FindCoplanar(coplanarFaces, face);
                    BoxF faceBox = new BoxF(face.BoundingBox.Start - new CoordinateF(3, 3, 3), face.BoundingBox.End + new CoordinateF(3, 3, 3));
                    if (group == null)
                    {
                        group = new LightmapGroup();
                        group.BoundingBox = faceBox;
                        group.Faces = new List<LMFace>();
                        group.Plane = new PlaneF(face.Plane.Normal, face.Vertices[0]);
                        coplanarFaces.Add(group);
                    }
                    group.Faces.Add(face);
                    group.Plane = new PlaneF(group.Plane.Normal, (face.Vertices[0] + group.Plane.PointOnPlane) / 2);
                    group.BoundingBox = new BoxF(new BoxF[] { group.BoundingBox, faceBox });
                }
            }

            for (int i = 0; i < coplanarFaces.Count; i++)
            {
                for (int j = i + 1; j < coplanarFaces.Count; j++)
                {
                    if ((coplanarFaces[i].Plane.Normal - coplanarFaces[j].Plane.Normal).LengthSquared() < 0.1f &&
                        coplanarFaces[i].BoundingBox.IntersectsWith(coplanarFaces[j].BoundingBox))
                    {
                        coplanarFaces[i].Faces.AddRange(coplanarFaces[j].Faces);
                        coplanarFaces[i].BoundingBox = new BoxF(new BoxF[] { coplanarFaces[i].BoundingBox, coplanarFaces[j].BoundingBox });
                        coplanarFaces.RemoveAt(j);
                        j = i + 1;
                    }
                }
            }

            Bitmap bitmap = new Bitmap(2048, 2048, PixelFormat.Format32bppArgb);
        }

        public static void SaveToFile_Old(string filename, Sledge.DataStructures.MapObjects.Map map)
        {
            List<LightmapGroup> coplanarFaces = new List<LightmapGroup>();

            //get faces
            foreach (Solid solid in map.WorldSpawn.Find(x => x is Solid).OfType<Solid>())
            {
                foreach (Face tface in solid.Faces)
                {
                    tface.UpdateBoundingBox();
                    if (tface.Texture.Name.ToLower() == "tooltextures/remove_face") continue;
                    LMFace face = new LMFace(tface);
                    LightmapGroup group = FindCoplanar(coplanarFaces, face);
                    BoxF faceBox = new BoxF(face.BoundingBox.Start - new CoordinateF(3, 3, 3), face.BoundingBox.End + new CoordinateF(3, 3, 3));
                    if (group == null)
                    {
                        group = new LightmapGroup();
                        group.BoundingBox = faceBox;
                        group.Faces = new List<LMFace>();
                        group.Plane = new PlaneF(face.Plane.Normal,face.Vertices[0]);
                        coplanarFaces.Add(group);
                    }
                    group.Faces.Add(face);
                    group.Plane = new PlaneF(group.Plane.Normal, (face.Vertices[0]+group.Plane.PointOnPlane)/2);
                    group.BoundingBox = new BoxF(new BoxF[] { group.BoundingBox, faceBox });
                }
            }

            for (int i=0;i<coplanarFaces.Count;i++)
            {
                for (int j=i+1;j<coplanarFaces.Count;j++)
                {
                    if ((coplanarFaces[i].Plane.Normal - coplanarFaces[j].Plane.Normal).LengthSquared() < 0.1f &&
                        coplanarFaces[i].BoundingBox.IntersectsWith(coplanarFaces[j].BoundingBox))
                    {
                        coplanarFaces[i].Faces.AddRange(coplanarFaces[j].Faces);
                        coplanarFaces[i].BoundingBox = new BoxF(new BoxF[] { coplanarFaces[i].BoundingBox, coplanarFaces[j].BoundingBox });
                        coplanarFaces.RemoveAt(j);
                        j = i+1;
                    }
                }
            }

            //Random rand = new Random();

            //sort faces
            /*foreach (LightmapGroup group in coplanarFaces)
            {
                var direction = group.Plane.GetClosestAxisToNormal();

                var tempV = direction == CoordinateF.UnitZ ? -CoordinateF.UnitY : -CoordinateF.UnitZ;
                var uAxis = group.Plane.Normal.Cross(tempV).Normalise();
                var vAxis = uAxis.Cross(group.Plane.Normal).Normalise();
                
                System.Drawing.Color color = System.Drawing.Color.FromArgb(rand.Next() % 60+20, rand.Next() % 60+20, rand.Next() % 60+20);

                foreach (LMFace face in group.Faces)
                {
                    face.Colour = color;
                }
            }*/
            
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

            List<LMLight> lightEntities = map.WorldSpawn.Find(q => q.ClassName == "light").OfType<Entity>()
                .Select(x => new LMLight() {
                    Origin = new CoordinateF(x.Origin),
                    Range = float.Parse(x.EntityData.GetPropertyValue("range")),
                    Color = new CoordinateF(x.EntityData.GetPropertyCoordinate("color"))
                }).ToList();

            List<LMFace> allFaces = coplanarFaces.Select(q => q.Faces).SelectMany(q => q).ToList();

            Stream meshStream = new FileStream("D:/repos/asd.mesh", FileMode.Create);
            BinaryWriter meshWriter = new BinaryWriter(meshStream);
            foreach (LightmapGroup group in coplanarFaces)
            {
                var direction = group.Plane.GetClosestAxisToNormal();

                var tempV = direction == CoordinateF.UnitZ ? -CoordinateF.UnitY : -CoordinateF.UnitZ;
                var uAxis = group.Plane.Normal.Cross(tempV).Normalise();
                var vAxis = uAxis.Cross(group.Plane.Normal).Normalise();

                float? minTotalX = null; float? maxTotalX = null;
                float? minTotalY = null; float? maxTotalY = null;

                foreach (LMFace face in group.Faces)
                {
                    foreach (CoordinateF coord in face.Vertices)
                    {
                        float x = coord.Dot(uAxis);
                        float y = coord.Dot(vAxis);

                        if (minTotalX == null || x < minTotalX) minTotalX = x;
                        if (minTotalY == null || y < minTotalY) minTotalY = y;
                        if (maxTotalX == null || x > maxTotalX) maxTotalX = x;
                        if (maxTotalY == null || y > maxTotalY) maxTotalY = y;
                    }
                }

                if ((maxTotalX-minTotalX)>(maxTotalY-minTotalY))
                {
                    float maxSwap = maxTotalX.Value; float minSwap = minTotalX.Value;
                    maxTotalX = maxTotalY; minTotalX = minTotalY;
                    maxTotalY = maxSwap; minTotalY = minSwap;

                    CoordinateF swapAxis = uAxis;
                    uAxis = vAxis;
                    vAxis = swapAxis;
                }

                if (writeY + (int)(maxTotalY-minTotalY) / downscaleFactor + 3 >= 2048)
                {
                    writeY = 0;
                    writeX += writeMaxX;
                    writeMaxX = 0;
                }

                foreach (LMFace face in group.Faces)
                {
                    /*meshWriter.Write((Int32)face.Vertices.Count);
                    foreach (CoordinateF vert in face.Vertices)
                    {
                        meshWriter.Write(vert.X);
                        meshWriter.Write(vert.Y);
                        meshWriter.Write(vert.Z);

                        meshWriter.Write((short)(writeX + (vert.Dot(uAxis) - minTotalX.Value) / downscaleFactor));
                        meshWriter.Write((short)(writeY + (vert.Dot(vAxis) - minTotalY.Value) / downscaleFactor));
                    }
                    List<uint> indices = face.GetTriangleIndices().ToList();
                    meshWriter.Write((Int32)indices.Count);
                    foreach (uint ind in indices)
                    {
                        meshWriter.Write((Int16)ind);
                    }*/
                    //Thread newThread = CreateLightmapRenderThread(buffer, lightEntities, uAxis, vAxis, writeX, writeY, minTotalX.Value, minTotalY.Value, face, allFaces);
                    //threads.Add(newThread);
                }
                
                writeY += (int)(maxTotalY - minTotalY)/downscaleFactor + 3;
                if ((int)(maxTotalX - minTotalX)/downscaleFactor + 3 > writeMaxX) writeMaxX = (int)(maxTotalX - minTotalX) / downscaleFactor + 3;
            }
            meshWriter.Dispose(); meshStream.Dispose();

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

        private static Thread CreateLightmapRenderThread(byte[] bitmapData, List<LMLight> lights, CoordinateF uAxis, CoordinateF vAxis, int writeX, int writeY, float minTotalX, float minTotalY, LMFace targetFace, List<LMFace> blockerFaces)
        {
            return new Thread(() => RenderLightOntoFace(bitmapData, lights, uAxis, vAxis, writeX, writeY, minTotalX, minTotalY, targetFace, blockerFaces));
        }

        private static void RenderLightOntoFace(byte[] bitmapData, List<LMLight> lights, CoordinateF uAxis, CoordinateF vAxis, int writeX, int writeY, float minTotalX, float minTotalY, LMFace targetFace,List<LMFace> blockerFaces)
        {
            lights = lights.FindAll(x =>
            {
                float range = x.Range;
                BoxF lightBox = new BoxF(x.Origin - new CoordinateF(range, range, range), x.Origin + new CoordinateF(range, range, range));
                return lightBox.IntersectsWith(targetFace.BoundingBox);
            });

            float? minX = null; float? maxX = null;
            float? minY = null; float? maxY = null;

            foreach (CoordinateF coord in targetFace.Vertices)
            {
                float x = coord.Dot(uAxis);
                float y = coord.Dot(vAxis);

                if (minX == null || x < minX) minX = x;
                if (minY == null || y < minY) minY = y;
                if (maxX == null || x > maxX) maxX = x;
                if (maxY == null || y > maxY) maxY = y;
            }

            float centerX = (maxX.Value + minX.Value) / 2;
            float centerY = (maxY.Value + minY.Value) / 2;

            int iterX = (int)Math.Ceiling((maxX.Value - minX.Value) / downscaleFactor);
            int iterY = (int)Math.Ceiling((maxY.Value - minY.Value) / downscaleFactor);

            int[,] r = new int[iterX, iterY];
            int[,] g = new int[iterX, iterY];
            int[,] b = new int[iterX, iterY];

            foreach (LMLight light in lights)
            {
                CoordinateF lightPos = light.Origin;
                float lightRange = light.Range;
                CoordinateF lightColor = light.Color;

                List<LMFace> applicableBlockerFaces = blockerFaces.FindAll(x =>
                {
                    if ((lightPos - x.BoundingBox.Center).Dot(targetFace.Plane.Normal) < 0.0f) return false;
                    if ((x.BoundingBox.Center - lightPos).Dot(targetFace.BoundingBox.Center - lightPos) > 0.0f) return true;
                    return false;
                });

                for (int y = 0; y < iterY; y++)
                {
                    for (int x = 0; x < iterX; x++)
                    {
                        float ttX = minX.Value + (x * downscaleFactor);
                        float ttY = minY.Value + (y * downscaleFactor);
                        CoordinateF pointOnPlane = (ttX - centerX) * uAxis + (ttY - centerY) * vAxis + targetFace.BoundingBox.Center;
                        
                        int tX = writeX + x + (int)(minX - minTotalX) / downscaleFactor;
                        int tY = writeY + y + (int)(minY - minTotalY) / downscaleFactor;
                        
                        Color luxelColor = Color.FromArgb(r[x,y],g[x,y],b[x, y]);

                        float dotToLight = (lightPos - pointOnPlane).Normalise().Dot(targetFace.Plane.Normal);
                        bool illuminated = false;
                        if (dotToLight > 0.0f && (pointOnPlane - lightPos).LengthSquared() < lightRange * lightRange)
                        {
                            LineF lineTester = new LineF(lightPos, pointOnPlane);
                            illuminated = true;
                            foreach (LMFace otherFace in applicableBlockerFaces)
                            {
                                CoordinateF hit = otherFace.GetIntersectionPoint(lineTester);
                                if (hit != null && (hit - pointOnPlane).LengthSquared() > 5.0f)
                                {
                                    illuminated = false;
                                    break;
                                }
                            }
                        }

                        if (illuminated)
                        {
                            float brightness = dotToLight * (lightRange - (pointOnPlane - lightPos).VectorMagnitude()) / lightRange;

                            r[x, y] += (int)(lightColor.X * brightness); if (r[x, y] > 255) r[x, y] = 255;
                            g[x, y] += (int)(lightColor.Y * brightness); if (g[x, y] > 255) g[x, y] = 255;
                            b[x, y] += (int)(lightColor.Z * brightness); if (b[x, y] > 255) b[x, y] = 255;

                            luxelColor = Color.FromArgb(r[x, y], g[x, y], b[x, y]);

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
    }
}
