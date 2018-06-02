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
using Sledge.Common;

namespace Sledge.Editor.Compiling
{
    class Lightmapper
    {
        public class LMLight
        {
            public CoordinateF Color;
            public CoordinateF Origin;
            public float Range;
        }

        public class LMFace
        {
            public PlaneF Plane { get; set; }

            public class Vertex
            {
                public Vertex(CoordinateF location, float u, float v)
                {
                    Location = location;
                    DiffU = u; DiffV = v;
                }
                public CoordinateF Location;
                public float DiffU; public float DiffV;
                public float LMU; public float LMV;
            };

            public List<Vertex> Vertices { get; set; }

            public BoxF BoundingBox { get; set; }

            public string Texture;

            public LMFace(Face face)
            {
                Plane = new PlaneF(face.Plane);

                Vertices = face.Vertices.Select(x => new Vertex(new CoordinateF(x.Location), (float)x.TextureU, (float)x.TextureV)).ToList();

                Texture = face.Texture.Name;

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
                    yield return new LineF(Vertices[i].Location, Vertices[(i + 1) % Vertices.Count].Location);
                }
            }

            public virtual IEnumerable<Vertex> GetIndexedVertices()
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

            public virtual IEnumerable<Vertex[]> GetTriangles()
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
                BoundingBox = new BoxF(Vertices.Select(x => x.Location));
            }

            /// <summary>
            /// Returns the point that this line intersects with this face.
            /// </summary>
            /// <param name="line">The intersection line</param>
            /// <returns>The point of intersection between the face and the line.
            /// Returns null if the line does not intersect this face.</returns>
            public virtual CoordinateF GetIntersectionPoint(LineF line)
            {
                return GetIntersectionPoint(this, line);
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
                return box.GetBoxLines().Any(x => GetIntersectionPoint(this, x, true) != null);
            }

            protected static CoordinateF GetIntersectionPoint(LMFace face, LineF line, bool ignoreDirection = false)
            {
                var plane = face.Plane;
                var intersect = plane.GetIntersectionPoint(line, ignoreDirection);
                List<CoordinateF> coordinates = face.Vertices.Select(x => x.Location).ToList();
                if (intersect == null) return null;
                BoxF bbox = new BoxF(face.BoundingBox.Start - new CoordinateF(0.5f, 0.5f, 0.5f), face.BoundingBox.End + new CoordinateF(0.5f, 0.5f, 0.5f));
                if (!bbox.CoordinateIsInside(intersect)) return null;

                CoordinateF centerPoint = face.BoundingBox.Center;
                for (var i = 0; i < coordinates.Count; i++)
                {
                    var i1 = i;
                    var i2 = (i + 1) % coordinates.Count;

                    var lineMiddle = (coordinates[i1] + coordinates[i2]) * 0.5f;
                    var middleToCenter = centerPoint - lineMiddle;
                    var v = coordinates[i1] - coordinates[i2];
                    var lineNormal = face.Plane.Normal.Cross(v);

                    if ((middleToCenter - lineNormal).LengthSquared() > (middleToCenter + lineNormal).LengthSquared())
                    {
                        lineNormal = -lineNormal;
                    }

                    if (lineNormal.Dot(intersect - lineMiddle) < 0.0f) return null;
                }
                return intersect;
            }
        }

        private class LightmapGroup
        {
            public PlaneF Plane;
            public BoxF BoundingBox;
            public List<LMFace> Faces;

            public CoordinateF uAxis;
            public CoordinateF vAxis;
            public float? minTotalX;
            public float? minTotalY;
            public float? maxTotalX;
            public float? maxTotalY;
            public int writeX;
            public int writeY;
        }

        private static float GetGroupTextureWidth(LightmapGroup group)
        {
            if (group.uAxis == null || group.vAxis == null)
            {
                var direction = group.Plane.GetClosestAxisToNormal();
                var tempV = direction == CoordinateF.UnitZ ? -CoordinateF.UnitY : -CoordinateF.UnitZ;
                var uAxis = group.Plane.Normal.Cross(tempV).Normalise();
                var vAxis = uAxis.Cross(group.Plane.Normal).Normalise();

                group.uAxis = uAxis; group.vAxis = vAxis;
            }

            if (group.minTotalX == null || group.minTotalY == null || group.maxTotalX == null || group.maxTotalY == null)
            {
                float? minTotalX = null; float? maxTotalX = null;
                float? minTotalY = null; float? maxTotalY = null;

                foreach (LMFace face in group.Faces)
                {
                    foreach (CoordinateF coord in face.Vertices.Select(x => x.Location))
                    {
                        float x = coord.Dot(group.uAxis);
                        float y = coord.Dot(group.vAxis);

                        if (minTotalX == null || x < minTotalX) minTotalX = x;
                        if (minTotalY == null || y < minTotalY) minTotalY = y;
                        if (maxTotalX == null || x > maxTotalX) maxTotalX = x;
                        if (maxTotalY == null || y > maxTotalY) maxTotalY = y;
                    }
                }

                minTotalX -= DownscaleFactor; minTotalY -= DownscaleFactor;
                maxTotalX += DownscaleFactor; maxTotalY += DownscaleFactor;

                minTotalX /= DownscaleFactor; minTotalX = (float)Math.Ceiling(minTotalX.Value); minTotalX *= DownscaleFactor;
                minTotalY /= DownscaleFactor; minTotalY = (float)Math.Ceiling(minTotalY.Value); minTotalY *= DownscaleFactor;
                maxTotalX /= DownscaleFactor; maxTotalX = (float)Math.Ceiling(maxTotalX.Value); maxTotalX *= DownscaleFactor;
                maxTotalY /= DownscaleFactor; maxTotalY = (float)Math.Ceiling(maxTotalY.Value); maxTotalY *= DownscaleFactor;

                if ((maxTotalX - minTotalX) > (maxTotalY - minTotalY))
                {
                    float maxSwap = maxTotalX.Value; float minSwap = minTotalX.Value;
                    maxTotalX = maxTotalY; minTotalX = minTotalY;
                    maxTotalY = maxSwap; minTotalY = minSwap;

                    CoordinateF swapAxis = group.uAxis;
                    group.uAxis = group.vAxis;
                    group.vAxis = swapAxis;
                }

                group.minTotalX = minTotalX; group.minTotalY = minTotalY;
                group.maxTotalX = maxTotalX; group.maxTotalY = maxTotalY;
            }

            return (group.maxTotalX - group.minTotalX).Value;
        }

        private static LightmapGroup FindCoplanar(List<LightmapGroup> lmGroups, LMFace otherFace)
        {
            foreach (LightmapGroup group in lmGroups)
            {
                if ((group.Plane.Normal - otherFace.Plane.Normal).LengthSquared() < 0.1f)
                {
                    PlaneF plane2 = new PlaneF(otherFace.Plane.Normal, otherFace.Vertices[0].Location);
                    if (Math.Abs(plane2.EvalAtPoint((group.Plane.PointOnPlane))) > 4.0f) continue;
                    BoxF faceBox = new BoxF(otherFace.BoundingBox.Start - new CoordinateF(3.0f, 3.0f, 3.0f), otherFace.BoundingBox.End + new CoordinateF(3.0f, 3.0f, 3.0f));
                    if (faceBox.IntersectsWith(group.BoundingBox)) return group;
                }
            }
            return null;
        }

        public static int DownscaleFactor = 10;
        public static int PlaneMargin = 5;
        public static int TextureDims = 2048;
        public static int BlurRadius = 2;

        public static Color AmbientColor = Color.FromArgb(30,30,30);
        public static CoordinateF AmbientNormal = new CoordinateF(1.0f, 2.0f, 3.0f).Normalise();

        struct LMThreadException
        {
            public LMThreadException(Exception e)
            {
                Message = e.Message;
                StackTrace = e.StackTrace;
            }

            public string Message;
            public string StackTrace;
        }

        public static List<Thread> FaceRenderThreads;
        private static List<LMThreadException> threadExceptions;
        
        public static IEnumerable<Tuple<string, float>> Render(Map map,Bitmap bitmap,List<LMFace> faces,List<LMLight> lightEntities)
        {
            threadExceptions = new List<LMThreadException>();
            
            List<LightmapGroup> lmGroups = new List<LightmapGroup>();
            List<LMFace> exclusiveBlockers = new List<LMFace>();
            
            //get faces
            yield return new Tuple<string,float>("Determining UV coordinates...",0);
            foreach (Solid solid in map.WorldSpawn.Find(x => x is Solid).OfType<Solid>())
            {
                foreach (Face tface in solid.Faces)
                {
                    tface.UpdateBoundingBox();
                    if (tface.Texture.Name.ToLower() == "tooltextures/invisible_collision") continue;
                    if (tface.Texture.Name.ToLower() == "tooltextures/remove_face") continue;
                    if (tface.Texture.Name.ToLower() == "tooltextures/block_light") continue;
                    if (tface.Texture.Texture.HasTransparency()) continue;
                    LMFace face = new LMFace(tface);
                    LightmapGroup group = FindCoplanar(lmGroups, face);
                    BoxF faceBox = new BoxF(face.BoundingBox.Start - new CoordinateF(3.0f, 3.0f, 3.0f), face.BoundingBox.End + new CoordinateF(3.0f, 3.0f, 3.0f));
                    if (group == null)
                    {
                        group = new LightmapGroup();
                        group.BoundingBox = faceBox;
                        group.Faces = new List<LMFace>();
                        group.Plane = new PlaneF(face.Plane.Normal, face.Vertices[0].Location);
                        lmGroups.Add(group);
                    }
                    group.Faces.Add(face);
                    group.Plane = new PlaneF(group.Plane.Normal, (face.Vertices[0].Location + group.Plane.PointOnPlane) / 2);
                    group.BoundingBox = new BoxF(new BoxF[] { group.BoundingBox, faceBox });
                }
            }

            foreach (Solid solid in map.WorldSpawn.Find(x => x is Solid).OfType<Solid>())
            {
                foreach (Face tface in solid.Faces)
                {
                    LMFace face = new LMFace(tface);
                    if (tface.Texture.Name.ToLower() != "tooltextures/block_light") continue;
                    exclusiveBlockers.Add(face);
                }
            }

            for (int i = 0; i < lmGroups.Count; i++)
            {
                for (int j = i + 1; j < lmGroups.Count; j++)
                {
                    if ((lmGroups[i].Plane.Normal - lmGroups[j].Plane.Normal).LengthSquared() < 0.1f &&
                        lmGroups[i].BoundingBox.IntersectsWith(lmGroups[j].BoundingBox))
                    {
                        lmGroups[i].Faces.AddRange(lmGroups[j].Faces);
                        lmGroups[i].BoundingBox = new BoxF(new BoxF[] { lmGroups[i].BoundingBox, lmGroups[j].BoundingBox });
                        lmGroups.RemoveAt(j);
                        j = i + 1;
                    }
                }
            }
            
            //put the faces into the bitmap
            lmGroups.Sort((x, y) =>
            {
                if (x == y) return 0;

                if (GetGroupTextureWidth(x) < GetGroupTextureWidth(y)) return 1;
                return -1;
            });

            int writeX = 1; int writeY = 1; int writeMaxX = 0;

            var buffer = new byte[bitmap.Width * bitmap.Height * Bitmap.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format32bppArgb) / 8];

            FaceRenderThreads = new List<Thread>();

            lightEntities.Clear(); lightEntities.AddRange(map.WorldSpawn.Find(q => q.ClassName == "light").OfType<Entity>()
                .Select(x => new LMLight()
                {
                    Origin = new CoordinateF(x.Origin),
                    Range = float.Parse(x.EntityData.GetPropertyValue("range")),
                    Color = new CoordinateF(x.EntityData.GetPropertyCoordinate("color"))
                }));

            List<LMFace> allFaces = lmGroups.Select(q => q.Faces).SelectMany(q => q).Union(exclusiveBlockers).ToList();
            int faceCount = 0;
            foreach (LightmapGroup group in lmGroups)
            {
                var uAxis = group.uAxis;
                var vAxis = group.vAxis;

                float minTotalX = group.minTotalX.Value;
                float maxTotalX = group.maxTotalX.Value;
                float minTotalY = group.minTotalY.Value;
                float maxTotalY = group.maxTotalY.Value;

                if (writeY + (int)(maxTotalY - minTotalY) / DownscaleFactor + PlaneMargin >= TextureDims)
                {
                    writeY = 0;
                    writeX += writeMaxX;
                    writeMaxX = 0;
                }

                foreach (LMFace face in group.Faces)
                {
                    faceCount++;
                    Thread newThread = CreateLightmapRenderThread(buffer, lightEntities, writeX, writeY, group, face, allFaces);
                    FaceRenderThreads.Add(newThread);
                }
                group.writeX = writeX;
                group.writeY = writeY;

                writeY += (int)(maxTotalY - minTotalY) / DownscaleFactor + PlaneMargin;
                if ((int)(maxTotalX - minTotalX) / DownscaleFactor + PlaneMargin > writeMaxX) writeMaxX = (int)(maxTotalX - minTotalX) / DownscaleFactor + PlaneMargin;
            }

            int faceNum = 0;
            yield return new Tuple<string, float>("Started calculating brightness levels...",0.05f);
            while (FaceRenderThreads.Count > 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (i >= FaceRenderThreads.Count) break;
                    if (FaceRenderThreads[i].ThreadState == ThreadState.Unstarted)
                    {
                        FaceRenderThreads[i].Start();
                    }
                    else if (!FaceRenderThreads[i].IsAlive)
                    {
                        FaceRenderThreads.RemoveAt(i);
                        i--;
                        faceNum++;
                        yield return new Tuple<string, float>(faceNum.ToString() + "/" + faceCount.ToString() + " faces complete",0.05f + ((float)faceNum/(float)faceCount)*0.85f);
                    }
                }

                if (threadExceptions.Count > 0)
                {
                    for (int i = 0; i < FaceRenderThreads.Count; i++)
                    {
                        if (FaceRenderThreads[i].IsAlive)
                        {
                            FaceRenderThreads[i].Abort();
                        }
                    }
                    throw new Exception(threadExceptions[0].Message);// + "\n" + threadExceptions[0].StackTrace);
                }
                Thread.Yield();
            }

            //blur the lightmap so it doesn't look too pixellated
            yield return new Tuple<string, float>("Blurring lightmap...",0.95f);
            byte[] blurBuffer = new byte[buffer.Length];
            foreach (LightmapGroup group in lmGroups)
            {
                float ambientMultiplier = (group.Plane.Normal.Dot(AmbientNormal) + 1.5f) * 0.4f;
                Color mAmbientColor = Color.FromArgb((int)(AmbientColor.R * ambientMultiplier),
                                                     (int)(AmbientColor.G * ambientMultiplier),
                                                     (int)(AmbientColor.B * ambientMultiplier));
                for (int y = group.writeY; y < group.writeY + (group.maxTotalY - group.minTotalY) / DownscaleFactor; y++)
                {
                    if (y < 0 || y >= TextureDims) continue;
                    for (int x = group.writeX; x < group.writeX + (group.maxTotalX - group.minTotalX) / DownscaleFactor; x++)
                    {
                        if (x < 0 || x >= TextureDims) continue;
                        int offset = (x + y * TextureDims) * System.Drawing.Image.GetPixelFormatSize(PixelFormat.Format32bppArgb) / 8;

                        int accumRed = 0;
                        int accumGreen = 0;
                        int accumBlue = 0;
                        int sampleCount = 0;
                        for (int j = -BlurRadius; j <= BlurRadius; j++)
                        {
                            if (y + j < 0 || y + j >= TextureDims) continue;
                            if (y + j < group.writeY || y + j >= group.writeY + (group.maxTotalY - group.minTotalY)) continue;
                            for (int i = -BlurRadius; i <= BlurRadius; i++)
                            {
                                if (i * i + j * j > BlurRadius * BlurRadius) continue;
                                if (x + i < 0 || x + i >= TextureDims) continue;
                                if (x + i < group.writeX || x + i >= group.writeX + (group.maxTotalX - group.minTotalX)) continue;
                                int sampleOffset = ((x + i) + (y + j) * TextureDims) * System.Drawing.Image.GetPixelFormatSize(PixelFormat.Format32bppArgb) / 8;
                                if (buffer[sampleOffset + 3] < 255) continue;
                                sampleCount++;
                                accumRed += buffer[sampleOffset + 0];
                                accumGreen += buffer[sampleOffset + 1];
                                accumBlue += buffer[sampleOffset + 2];
                            }
                        }

                        if (sampleCount < 1) sampleCount = 1;
                        accumRed /= sampleCount;
                        accumGreen /= sampleCount;
                        accumBlue /= sampleCount;

                        accumRed = mAmbientColor.R + (accumRed * (255 - mAmbientColor.R) / 255);
                        accumGreen = mAmbientColor.G + (accumGreen * (255 - mAmbientColor.G) / 255);
                        accumBlue = mAmbientColor.B + (accumBlue * (255 - mAmbientColor.B) / 255);

                        if (accumRed > 255) accumRed = 255;
                        if (accumGreen > 255) accumGreen = 255;
                        if (accumBlue > 255) accumBlue = 255;

                        blurBuffer[offset + 0] = (byte)accumRed;
                        blurBuffer[offset + 1] = (byte)accumGreen;
                        blurBuffer[offset + 2] = (byte)accumBlue;
                        blurBuffer[offset + 3] = 255;
                    }
                }
            }

            yield return new Tuple<string, float>("Copying bitmap data...",0.99f);
            BitmapData bitmapData2 = bitmap.LockBits(new Rectangle(0, 0, TextureDims, TextureDims), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Marshal.Copy(blurBuffer, 0, bitmapData2.Scan0, blurBuffer.Length);
            bitmap.UnlockBits(bitmapData2);

            faces.Clear();
            faces.AddRange(lmGroups.SelectMany(g => g.Faces));
            yield return new Tuple<string, float>("Lightmapping complete!",1.0f);
        }

        private static Thread CreateLightmapRenderThread(byte[] bitmapData, List<LMLight> lights, int writeX, int writeY, LightmapGroup group, LMFace targetFace, List<LMFace> blockerFaces)
        {
            return new Thread(() => {
                try
                {
                    RenderLightOntoFace(bitmapData, lights, writeX, writeY, group, targetFace, blockerFaces);
                }
                catch (ThreadAbortException e)
                {
                    //do nothing
                }
                catch (Exception e)
                {
                    threadExceptions.Add(new LMThreadException(e));
                }
            });
        }

        private static void RenderLightOntoFace(byte[] bitmapData, List<LMLight> lights, int writeX, int writeY, LightmapGroup group, LMFace targetFace, List<LMFace> blockerFaces)
        {
            Random rand = new Random();

            lights = lights.FindAll(x =>
            {
                float range = x.Range;
                BoxF lightBox = new BoxF(x.Origin - new CoordinateF(range, range, range), x.Origin + new CoordinateF(range, range, range));
                return lightBox.IntersectsWith(targetFace.BoundingBox);
            });

            float? minX = null; float? maxX = null;
            float? minY = null; float? maxY = null;

            foreach (CoordinateF coord in targetFace.Vertices.Select(x => x.Location))
            {
                float x = coord.Dot(group.uAxis);
                float y = coord.Dot(group.vAxis);

                if (minX == null || x < minX) minX = x;
                if (minY == null || y < minY) minY = y;
                if (maxX == null || x > maxX) maxX = x;
                if (maxY == null || y > maxY) maxY = y;
            }

            minX -= DownscaleFactor; minY -= DownscaleFactor;
            maxX += DownscaleFactor; maxY += DownscaleFactor;

            minX /= DownscaleFactor; minX = (float)Math.Ceiling(minX.Value); minX *= DownscaleFactor;
            minY /= DownscaleFactor; minY = (float)Math.Ceiling(minY.Value); minY *= DownscaleFactor;
            maxX /= DownscaleFactor; maxX = (float)Math.Ceiling(maxX.Value); maxX *= DownscaleFactor;
            maxY /= DownscaleFactor; maxY = (float)Math.Ceiling(maxY.Value); maxY *= DownscaleFactor;

            foreach (LMFace.Vertex vert in targetFace.Vertices)
            {
                float x = vert.Location.Dot(group.uAxis);
                float y = vert.Location.Dot(group.vAxis);

                float u = (writeX + 0.5f + (x - group.minTotalX.Value) / DownscaleFactor) / TextureDims;
                float v = (writeY + 0.5f + (y - group.minTotalY.Value) / DownscaleFactor) / TextureDims;

                vert.LMU = u; vert.LMV = v;
            }

            float centerX = (maxX.Value + minX.Value) / 2;
            float centerY = (maxY.Value + minY.Value) / 2;

            int iterX = (int)Math.Ceiling((maxX.Value - minX.Value) / DownscaleFactor);
            int iterY = (int)Math.Ceiling((maxY.Value - minY.Value) / DownscaleFactor);

            int[,] r = new int[iterX, iterY];
            int[,] g = new int[iterX, iterY];
            int[,] b = new int[iterX, iterY];

            foreach (LMLight light in lights)
            {
                CoordinateF lightPos = light.Origin;
                float lightRange = light.Range;
                CoordinateF lightColor = light.Color;

                BoxF lightBox = new BoxF(new BoxF[] { targetFace.BoundingBox, new BoxF(light.Origin - new CoordinateF(30.0f, 30.0f, 30.0f), light.Origin + new CoordinateF(30.0f, 30.0f, 30.0f)) });
                List<LMFace> applicableBlockerFaces = blockerFaces.FindAll(x =>
                {
                    if (x == targetFace) return false;
                    if (group.Faces.Contains(x)) return false;
                    //return true;
                    if (lightBox.IntersectsWith(x.BoundingBox)) return true;
                    return false;
                });

                bool[,] illuminated = new bool[iterX, iterY];

                for (int y = 0; y < iterY; y++)
                {
                    for (int x = 0; x < iterX; x++)
                    {
                        illuminated[x, y] = true;
                    }
                }

#if FALSE
                for (int ind=0;ind<applicableBlockerFaces.Count;ind++)
                {
                    LMFace face = applicableBlockerFaces[ind];
                    List<Tuple<int,int>> projectedVertices = new List<Tuple<int, int>>();
                    foreach (CoordinateF vertex in face.Vertices)
                    {
                        LineF lineTester = new LineF(lightPos, vertex);

                        float denominator = -face.Plane.Normal.Dot(vertex - lightPos);

                        if (denominator < 0.0f)
                        {
                            projectedVertices.Clear();
                            break;
                        }

                        CoordinateF hit = targetFace.Plane.GetIntersectionPoint(lineTester,false,true);

                        if (hit == null || (hit-lightPos).LengthSquared()-36<(vertex-lightPos).LengthSquared())
                        {
                            projectedVertices.Clear();
                            break;
                        }
                        int x = (int)((hit.Dot(uAxis)-minX.Value)/DownscaleFactor);
                        int y = (int)((hit.Dot(vAxis)-minY.Value)/DownscaleFactor);

                        projectedVertices.Add(new Tuple<int,int>(x,y));
                    }

                    if (projectedVertices.Count == 0) continue;
                    applicableBlockerFaces.RemoveAt(ind);
                    ind--;

                    List<uint> indices = face.GetTriangleIndices().ToList();
                    for (int i=0;i<indices.Count;i+=3)
                    {
                        int vert0 = (int)indices[i + 0];
                        int vert1 = (int)indices[i + 1];
                        int vert2 = (int)indices[i + 2];

                        CoordinateF coord0 = face.Vertices[vert0];
                        CoordinateF coord1 = face.Vertices[vert1];
                        CoordinateF coord2 = face.Vertices[vert2];

                        Tuple<int, int> topCoord = null;
                        Tuple<int, int> middleCoord = null;
                        Tuple<int, int> bottomCoord = null;

                        int leftX = projectedVertices[vert0].Item1;
                        if (projectedVertices[vert1].Item1 < leftX)
                        {
                            leftX = projectedVertices[vert1].Item1;
                        }
                        if (projectedVertices[vert2].Item1 < leftX)
                        {
                            leftX = projectedVertices[vert2].Item1;
                        }
                        if (leftX < 0) leftX = 0; if (leftX >= iterX) leftX = iterX - 1;
                        
                        int rightX = projectedVertices[vert0].Item1;
                        if (projectedVertices[vert1].Item1 > rightX)
                        {
                            rightX = projectedVertices[vert1].Item1;
                        }
                        if (projectedVertices[vert2].Item1 > rightX)
                        {
                            rightX = projectedVertices[vert2].Item1;
                        }
                        if (rightX < 0) rightX = 0; if (rightX >= iterX) rightX = iterX - 1;

                        int topY = projectedVertices[vert0].Item2; topCoord = projectedVertices[vert0];
                        if (projectedVertices[vert1].Item2 < topY)
                        {
                            topY = projectedVertices[vert1].Item2; topCoord = projectedVertices[vert1];
                        }
                        if (projectedVertices[vert2].Item2 < topY)
                        {
                            topY = projectedVertices[vert2].Item2; topCoord = projectedVertices[vert2];
                        }
                        
                        int bottomY = projectedVertices[vert0].Item2; bottomCoord = projectedVertices[vert0];
                        if (projectedVertices[vert1].Item2 > bottomY)
                        {
                            bottomY = projectedVertices[vert1].Item2; bottomCoord = projectedVertices[vert1];
                        }
                        if (projectedVertices[vert2].Item2 > bottomY)
                        {
                            bottomY = projectedVertices[vert2].Item2; bottomCoord = projectedVertices[vert2];
                        }
                        
                        if (topCoord != projectedVertices[vert0] && bottomCoord != projectedVertices[vert0])
                        {
                            middleCoord = projectedVertices[vert0];
                        }
                        else if (topCoord != projectedVertices[vert1] && bottomCoord != projectedVertices[vert1])
                        {
                            middleCoord = projectedVertices[vert1];
                        }
                        else
                        {
                            middleCoord = projectedVertices[vert2];
                        }
                        
                        Tuple<int, int> splitterCoord = new Tuple<int, int>(
                            (int)(topCoord.Item1 + ((float)(middleCoord.Item2 - topCoord.Item2) / (float)(bottomCoord.Item2 - topCoord.Item2)) * (bottomCoord.Item1 - topCoord.Item1)), middleCoord.Item2);

                        if (splitterCoord.Item1 < middleCoord.Item1)
                        {
                            Tuple<int, int> temp = splitterCoord;
                            splitterCoord = middleCoord;
                            middleCoord = temp;
                        }
                            
                        if (topCoord.Item2 != middleCoord.Item2)
                        {
                            float invslope1 = (float)(middleCoord.Item1 - topCoord.Item1) / (float)(middleCoord.Item2 - topCoord.Item2);
                            float invslope2 = (float)(splitterCoord.Item1 - topCoord.Item1) / (float)(splitterCoord.Item2 - topCoord.Item2);

                            float curx1 = topCoord.Item1;
                            float curx2 = topCoord.Item1;

                            for (int scanlineY = topCoord.Item2; scanlineY <= middleCoord.Item2; scanlineY++)
                            {
                                for (int scanlineX = (int)Math.Floor(curx1); scanlineX <= (int)Math.Ceiling(curx2); scanlineX++)
                                {
                                    if (scanlineY > iterY - 1) break;
                                    if (scanlineY < 0) break;
                                    if (scanlineX > iterX - 1) break;
                                    if (scanlineX < 0) continue;
                                    illuminated[scanlineX, scanlineY] = false;
                                }
                                curx1 += invslope1;
                                curx2 += invslope2;
                            }
                        }

                        if (bottomCoord.Item2 != middleCoord.Item2)
                        {
                            float invslope1 = (float)(bottomCoord.Item1 - middleCoord.Item1) / (float)(bottomCoord.Item2 - middleCoord.Item2);
                            float invslope2 = (float)(bottomCoord.Item1 - splitterCoord.Item1) / (float)(bottomCoord.Item2 - splitterCoord.Item2);

                            float curx1 = bottomCoord.Item1;
                            float curx2 = bottomCoord.Item1;

                            for (int scanlineY = bottomCoord.Item2; scanlineY > middleCoord.Item2; scanlineY--)
                            {
                                for (int scanlineX = (int)Math.Floor(curx1); scanlineX <= (int)Math.Ceiling(curx2); scanlineX++)
                                {
                                    if (scanlineY > iterY - 1) break;
                                    if (scanlineY < 0) break;
                                    if (scanlineX > iterX - 1) break;
                                    if (scanlineX < 0) continue;
                                    illuminated[scanlineX, scanlineY] = false;
                                }
                                curx1 -= invslope1;
                                curx2 -= invslope2;
                            }
                        }
                    }
                }
#endif

                for (int y = 0; y < iterY; y++)
                {
                    for (int x = 0; x < iterX; x++)
                    {
                        int tX = writeX + x + (int)(minX - group.minTotalX) / DownscaleFactor;
                        int tY = writeY + y + (int)(minY - group.minTotalY) / DownscaleFactor;

                        if (tX >= 0 && tY >= 0 && tX < TextureDims && tY < TextureDims)
                        {
                            int offset = (tX + tY * TextureDims) * Bitmap.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format32bppArgb) / 8;
                            bitmapData[offset + 3] = 255;
                        }
                    }
                }

                for (int y = 0; y < iterY; y++)
                {
                    for (int x = 0; x < iterX; x++)
                    {
                        float ttX = minX.Value + (x * DownscaleFactor);
                        float ttY = minY.Value + (y * DownscaleFactor);
                        CoordinateF pointOnPlane = (ttX - centerX) * group.uAxis + (ttY - centerY) * group.vAxis + targetFace.BoundingBox.Center;

                        int tX = writeX + x + (int)(minX - group.minTotalX) / DownscaleFactor;
                        int tY = writeY + y + (int)(minY - group.minTotalY) / DownscaleFactor;

                        Color luxelColor = Color.FromArgb(r[x, y], g[x, y], b[x, y]);

                        float dotToLight = (lightPos - pointOnPlane).Normalise().Dot(targetFace.Plane.Normal);

                        if (illuminated[x, y] && dotToLight > 0.0f && (pointOnPlane - lightPos).LengthSquared() < lightRange * lightRange)
                        {
#if TRUE
                            LineF lineTester = new LineF(lightPos, pointOnPlane);
                            for (int i = 0; i < applicableBlockerFaces.Count; i++)
                            {
                                LMFace otherFace = applicableBlockerFaces[i];
                                CoordinateF hit = otherFace.GetIntersectionPoint(lineTester);
                                if (hit != null)
                                {
                                    applicableBlockerFaces.RemoveAt(i);
                                    applicableBlockerFaces.Insert(0, otherFace);
                                    if ((hit - pointOnPlane).LengthSquared() > 25.0f && Math.Abs((hit - pointOnPlane).Dot(targetFace.Plane.Normal)) > 15.0f)
                                    {
                                        illuminated[x, y] = false;
                                        i++;
                                        break;
                                    }
                                }
                            }
#endif
                        }
                        else
                        {
                            illuminated[x, y] = false;
                        }

                        if (illuminated[x, y])
                        {
                            float brightness = (lightRange - (pointOnPlane - lightPos).VectorMagnitude()) / lightRange;
                            brightness *= dotToLight * brightness;
                            brightness += ((float)rand.NextDouble() - 0.5f) * 0.005f;

                            r[x, y] += (int)(lightColor.Z * brightness); if (r[x, y] > 255) r[x, y] = 255; if (r[x, y] < 0) r[x, y] = 0;
                            g[x, y] += (int)(lightColor.Y * brightness); if (g[x, y] > 255) g[x, y] = 255; if (g[x, y] < 0) g[x, y] = 0;
                            b[x, y] += (int)(lightColor.X * brightness); if (b[x, y] > 255) b[x, y] = 255; if (b[x, y] < 0) b[x, y] = 0;

                            luxelColor = Color.FromArgb(r[x, y], g[x, y], b[x, y]);

                            if (tX >= 0 && tY >= 0 && tX < TextureDims && tY < TextureDims)
                            {
                                int offset = (tX + tY * TextureDims) * Bitmap.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format32bppArgb) / 8;
                                if (luxelColor.R + luxelColor.G + luxelColor.B > bitmapData[offset + 2] + bitmapData[offset + 1] + bitmapData[offset + 0])
                                {
                                    bitmapData[offset + 0] = luxelColor.R;
                                    bitmapData[offset + 1] = luxelColor.G;
                                    bitmapData[offset + 2] = luxelColor.B;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
