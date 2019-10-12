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

            public CoordinateF Direction;
            public float? innerCos;
            public float? outerCos;
        }

        public class LMFace
        {
            public PlaneF Plane { get; set; }
            public CoordinateF Normal;
            public CoordinateF Tangent;
            public CoordinateF Bitangent;

            public CoordinateF LightBasis0;
            public CoordinateF LightBasis1;
            public CoordinateF LightBasis2;

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

                Normal = Plane.Normal;

                Vertices = face.Vertices.Select(x => new Vertex(new CoordinateF(x.Location), (float)x.TextureU, (float)x.TextureV)).ToList();

                int i1 = 0;
                int i2 = 1;
                int i3 = 2;

                CoordinateF v1 = Vertices[i1].Location;
                CoordinateF v2 = Vertices[i2].Location;
                CoordinateF v3 = Vertices[i3].Location;

                float w1x = Vertices[i1].DiffU; float w1y = Vertices[i1].DiffV;
                float w2x = Vertices[i2].DiffU; float w2y = Vertices[i2].DiffV;
                float w3x = Vertices[i3].DiffU; float w3y = Vertices[i3].DiffV;
                
                float x1 = v2.X - v1.X;
                float x2 = v3.X - v1.X;
                float y1 = v2.Y - v1.Y;
                float y2 = v3.Y - v1.Y;
                float z1 = v2.Z - v1.Z;
                float z2 = v3.Z - v1.Z;

                float s1 = w2x - w1x;
                float s2 = w3x - w1x;
                float t1 = w2y - w1y;
                float t2 = w3y - w1y;

                float r = 1.0f / (s1 * t2 - s2 * t1);
                CoordinateF sdir = new CoordinateF((t2* x1 -t1 * x2) *r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                CoordinateF tdir = new CoordinateF((s1* x2 -s2 * x1) *r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                Tangent = (sdir - Normal * Normal.Dot(sdir)).Normalise();
                Bitangent = (tdir - Normal * Normal.Dot(tdir)).Normalise();

                LightBasis0 = Tangent * (-1.0f / (float)Math.Sqrt(6.0)) + Bitangent * (-1.0f / (float)Math.Sqrt(2.0)) + Normal * (1.0f / (float)Math.Sqrt(3.0));
                LightBasis1 = Tangent * (-1.0f / (float)Math.Sqrt(6.0)) + Bitangent * (1.0f / (float)Math.Sqrt(2.0)) + Normal * (1.0f / (float)Math.Sqrt(3.0));
                LightBasis2 = Tangent * ((float)Math.Sqrt(2.0/3.0)) + Normal * (1.0f / (float)Math.Sqrt(3.0));

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

        public static IEnumerable<Tuple<string, float>> Render(Map map, Bitmap[] bitmaps, List<LMFace> faces, List<LMLight> lightEntities)
        {
            threadExceptions = new List<LMThreadException>();

            List<LightmapGroup> lmGroups = new List<LightmapGroup>();
            List<LMFace> exclusiveBlockers = new List<LMFace>();

            //get faces
            yield return new Tuple<string, float>("Determining UV coordinates...", 0);
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

            float[][] buffers = new float[4][];
            buffers[0] = new float[bitmaps[0].Width * bitmaps[0].Height * Bitmap.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format32bppArgb) / 8];
            buffers[1] = new float[bitmaps[1].Width * bitmaps[1].Height * Bitmap.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format32bppArgb) / 8];
            buffers[2] = new float[bitmaps[2].Width * bitmaps[2].Height * Bitmap.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format32bppArgb) / 8];
            buffers[3] = new float[bitmaps[2].Width * bitmaps[2].Height * Bitmap.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format32bppArgb) / 8];

            FaceRenderThreads = new List<Thread>();

            lightEntities.Clear(); lightEntities.AddRange(map.WorldSpawn.Find(q => q.ClassName == "light").OfType<Entity>()
                .Select(x => new LMLight()
                {
                    Origin = new CoordinateF(x.Origin),
                    Range = float.Parse(x.EntityData.GetPropertyValue("range")),
                    Color = new CoordinateF(x.EntityData.GetPropertyCoordinate("color")),
                    Direction = null,
                    innerCos = null,
                    outerCos = null
                }));
            lightEntities.AddRange(map.WorldSpawn.Find(q => q.ClassName == "spotlight").OfType<Entity>()
                .Select(x => {
                    LMLight light = new LMLight()
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
                    Thread newThread = CreateLightmapRenderThread(buffers, lightEntities, writeX, writeY, group, face, allFaces);
                    FaceRenderThreads.Add(newThread);
                }
                group.writeX = writeX;
                group.writeY = writeY;

                writeY += (int)(maxTotalY - minTotalY) / DownscaleFactor + PlaneMargin;
                if ((int)(maxTotalX - minTotalX) / DownscaleFactor + PlaneMargin > writeMaxX) writeMaxX = (int)(maxTotalX - minTotalX) / DownscaleFactor + PlaneMargin;
            }

            int faceNum = 0;
            yield return new Tuple<string, float>("Started calculating brightness levels...", 0.05f);
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
                        yield return new Tuple<string, float>(faceNum.ToString() + "/" + faceCount.ToString() + " faces complete", 0.05f + ((float)faceNum / (float)faceCount) * 0.85f);
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
                    throw new Exception(threadExceptions[0].Message + "\n" + threadExceptions[0].StackTrace);
                }
                Thread.Yield();
            }

            //blur the lightmap so it doesn't look too pixellated
            yield return new Tuple<string, float>("Blurring lightmap...", 0.95f);
            float[] blurBuffer = new float[buffers[0].Length];
            for (int k = 0; k < 4; k++)
            {
                foreach (LightmapGroup group in lmGroups)
                {
                    float ambientMultiplier = (group.Plane.Normal.Dot(AmbientNormal) + 1.5f) * 0.4f;
                    CoordinateF mAmbientColor = new CoordinateF((AmbientColor.B * ambientMultiplier / 255.0f),
                                                         (AmbientColor.G * ambientMultiplier / 255.0f),
                                                         (AmbientColor.R * ambientMultiplier / 255.0f));
                    for (int y = group.writeY; y < group.writeY + (group.maxTotalY - group.minTotalY) / DownscaleFactor; y++)
                    {
                        if (y < 0 || y >= TextureDims) continue;
                        for (int x = group.writeX; x < group.writeX + (group.maxTotalX - group.minTotalX) / DownscaleFactor; x++)
                        {
                            if (x < 0 || x >= TextureDims) continue;
                            int offset = (x + y * TextureDims) * System.Drawing.Image.GetPixelFormatSize(PixelFormat.Format32bppArgb) / 8;

                            float accumRed = 0;
                            float accumGreen = 0;
                            float accumBlue = 0;
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
                                    if (buffers[k][sampleOffset + 3] < 1.0f) continue;
                                    sampleCount++;
                                    accumRed += buffers[k][sampleOffset + 0];
                                    accumGreen += buffers[k][sampleOffset + 1];
                                    accumBlue += buffers[k][sampleOffset + 2];
                                }
                            }

                            if (sampleCount < 1) sampleCount = 1;
                            accumRed /= sampleCount;
                            accumGreen /= sampleCount;
                            accumBlue /= sampleCount;

                            accumRed = mAmbientColor.X + (accumRed * (1.0f - mAmbientColor.X));
                            accumGreen = mAmbientColor.Y + (accumGreen * (1.0f - mAmbientColor.Y));
                            accumBlue = mAmbientColor.Z + (accumBlue * (1.0f - mAmbientColor.Z));

                            if (accumRed > 1.0f) accumRed = 1.0f;
                            if (accumGreen > 1.0f) accumGreen = 1.0f;
                            if (accumBlue > 1.0f) accumBlue = 1.0f;

                            blurBuffer[offset + 0] = accumRed;
                            blurBuffer[offset + 1] = accumGreen;
                            blurBuffer[offset + 2] = accumBlue;
                            blurBuffer[offset + 3] = 1.0f;
                        }
                    }
                }

                blurBuffer.CopyTo(buffers[k], 0);
            }

            for (int i = 0; i < buffers[0].Length; i++)
            {
                if (i%4==3) {
                    buffers[0][i] = 1.0f;
                    buffers[1][i] = 1.0f;
                    buffers[2][i] = 1.0f;
                }
                else
                {
                    float brightnessAdd = (buffers[0][i] + buffers[1][i] + buffers[2][i]) / (float)Math.Sqrt(3.0);
                    if (brightnessAdd > 0.0f) //normalize brightness to remove artifacts when adding together
                    {
                        buffers[0][i] *= buffers[3][i] / brightnessAdd;
                        buffers[1][i] *= buffers[3][i] / brightnessAdd;
                        buffers[2][i] *= buffers[3][i] / brightnessAdd;
                    }
                }
            }

            for (int k = 0; k < 3; k++)
            {
                byte[] byteBuffer = new byte[buffers[k].Length];
                for (int i = 0; i < buffers[k].Length; i++)
                {
                    byteBuffer[i] = (byte)Math.Max(Math.Min(buffers[k][i] * 255.0f,255.0f),0.0f);
                }
                yield return new Tuple<string, float>("Copying bitmap data...", 0.99f);
                BitmapData bitmapData2 = bitmaps[k].LockBits(new Rectangle(0, 0, TextureDims, TextureDims), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Marshal.Copy(byteBuffer, 0, bitmapData2.Scan0, byteBuffer.Length);
                bitmaps[k].UnlockBits(bitmapData2);
            }

            faces.Clear();
            faces.AddRange(lmGroups.SelectMany(g => g.Faces));
            yield return new Tuple<string, float>("Lightmapping complete!",1.0f);
        }

        private static Thread CreateLightmapRenderThread(float[][] bitmaps, List<LMLight> lights, int writeX, int writeY, LightmapGroup group, LMFace targetFace, List<LMFace> blockerFaces)
        {
            return new Thread(() => {
                try
                {
                    RenderLightOntoFace(bitmaps, lights, writeX, writeY, group, targetFace, blockerFaces);
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

        private static void RenderLightOntoFace(float[][] bitmaps, List<LMLight> lights, int writeX, int writeY, LightmapGroup group, LMFace targetFace, List<LMFace> blockerFaces)
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

            float[][,] r = new float[4][,];
            r[0] = new float[iterX, iterY];
            r[1] = new float[iterX, iterY];
            r[2] = new float[iterX, iterY];
            r[3] = new float[iterX, iterY];
            float[][,] g = new float[4][,];
            g[0] = new float[iterX, iterY];
            g[1] = new float[iterX, iterY];
            g[2] = new float[iterX, iterY];
            g[3] = new float[iterX, iterY];
            float[][,] b = new float[4][,];
            b[0] = new float[iterX, iterY];
            b[1] = new float[iterX, iterY];
            b[2] = new float[iterX, iterY];
            b[3] = new float[iterX, iterY];

            foreach (LMLight light in lights)
            {
                CoordinateF lightPos = light.Origin;
                float lightRange = light.Range;
                CoordinateF lightColor = light.Color*(1.0f/255.0f);

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
                            bitmaps[0][offset + 3] = 1.0f;
                            bitmaps[1][offset + 3] = 1.0f;
                            bitmaps[2][offset + 3] = 1.0f;
                            bitmaps[3][offset + 3] = 1.0f;
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

                        CoordinateF luxelColor0 = new CoordinateF(r[0][x, y], g[0][x, y], b[0][x, y]);
                        CoordinateF luxelColor1 = new CoordinateF(r[1][x, y], g[1][x, y], b[1][x, y]);
                        CoordinateF luxelColor2 = new CoordinateF(r[2][x, y], g[2][x, y], b[2][x, y]);
                        CoordinateF luxelColorNorm = new CoordinateF(r[3][x, y], g[3][x, y], b[3][x, y]);

                        float dotToLight0 = Math.Max((lightPos - pointOnPlane).Normalise().Dot(targetFace.LightBasis0), 0.0f);
                        float dotToLight1 = Math.Max((lightPos - pointOnPlane).Normalise().Dot(targetFace.LightBasis1), 0.0f);
                        float dotToLight2 = Math.Max((lightPos - pointOnPlane).Normalise().Dot(targetFace.LightBasis2), 0.0f);
                        float dotToLightNorm = Math.Max((lightPos - pointOnPlane).Normalise().Dot(targetFace.Normal), 0.0f);

                        if (illuminated[x, y] && (pointOnPlane - lightPos).LengthSquared() < lightRange * lightRange)
                        {
#if TRUE
                            LineF lineTester = new LineF(lightPos, pointOnPlane);
                            for (int i = 0; i < applicableBlockerFaces.Count; i++)
                            {
                                LMFace otherFace = applicableBlockerFaces[i];
                                CoordinateF hit = otherFace.GetIntersectionPoint(lineTester);
                                if (hit != null && (hit - pointOnPlane).LengthSquared() > 25.0f && Math.Abs((hit - pointOnPlane).Dot(targetFace.Plane.Normal)) > 15.0f)
                                {
                                    applicableBlockerFaces.RemoveAt(i);
                                    applicableBlockerFaces.Insert(0, otherFace);
                                    illuminated[x, y] = false;
                                    i++;
                                    break;
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

                            if (light.Direction != null)
                            {
                                float directionDot = light.Direction.Dot((pointOnPlane-lightPos).Normalise());
                                
                                if (directionDot < light.innerCos)
                                {
                                    if (directionDot < light.outerCos)
                                    {
                                        brightness = 0.0f;
                                    }
                                    else
                                    {
                                        brightness *= (directionDot - light.outerCos.Value) / (light.innerCos.Value - light.outerCos.Value);
                                    }
                                }
                            }

                            float brightness0 = dotToLight0 * brightness * brightness;
                            float brightness1 = dotToLight1 * brightness * brightness;
                            float brightness2 = dotToLight2 * brightness * brightness;
                            float brightnessNorm = dotToLightNorm * brightness * brightness;

                            /*float brightnessAdd = (brightness0 + brightness1 + brightness2)/(float)Math.Sqrt(3.0);
                            if (brightnessAdd > 0.0f) //normalize brightness to remove artifacts when adding together
                            {
                                brightness0 *= brightnessNorm / brightnessAdd;
                                brightness1 *= brightnessNorm / brightnessAdd;
                                brightness2 *= brightnessNorm / brightnessAdd;
                            }*/

                            brightness0 += ((float)rand.NextDouble() - 0.5f) * 0.005f;
                            brightness1 += ((float)rand.NextDouble() - 0.5f) * 0.005f;
                            brightness2 += ((float)rand.NextDouble() - 0.5f) * 0.005f;
                            brightnessNorm += ((float)rand.NextDouble() - 0.5f) * 0.005f;

                            r[0][x, y] += lightColor.Z * brightness0; if (r[0][x, y] > 1.0f) r[0][x, y] = 1.0f; if (r[0][x, y] < 0) r[0][x, y] = 0;
                            g[0][x, y] += lightColor.Y * brightness0; if (g[0][x, y] > 1.0f) g[0][x, y] = 1.0f; if (g[0][x, y] < 0) g[0][x, y] = 0;
                            b[0][x, y] += lightColor.X * brightness0; if (b[0][x, y] > 1.0f) b[0][x, y] = 1.0f; if (b[0][x, y] < 0) b[0][x, y] = 0;

                            r[1][x, y] += lightColor.Z * brightness1; if (r[1][x, y] > 1.0f) r[1][x, y] = 1.0f; if (r[1][x, y] < 0) r[1][x, y] = 0;
                            g[1][x, y] += lightColor.Y * brightness1; if (g[1][x, y] > 1.0f) g[1][x, y] = 1.0f; if (g[1][x, y] < 0) g[1][x, y] = 0;
                            b[1][x, y] += lightColor.X * brightness1; if (b[1][x, y] > 1.0f) b[1][x, y] = 1.0f; if (b[1][x, y] < 0) b[1][x, y] = 0;

                            r[2][x, y] += lightColor.Z * brightness2; if (r[2][x, y] > 1.0f) r[2][x, y] = 1.0f; if (r[2][x, y] < 0) r[2][x, y] = 0;
                            g[2][x, y] += lightColor.Y * brightness2; if (g[2][x, y] > 1.0f) g[2][x, y] = 1.0f; if (g[2][x, y] < 0) g[2][x, y] = 0;
                            b[2][x, y] += lightColor.X * brightness2; if (b[2][x, y] > 1.0f) b[2][x, y] = 1.0f; if (b[2][x, y] < 0) b[2][x, y] = 0;

                            r[3][x, y] += lightColor.Z * brightnessNorm; if (r[3][x, y] > 1.0f) r[3][x, y] = 1.0f; if (r[3][x, y] < 0) r[3][x, y] = 0;
                            g[3][x, y] += lightColor.Y * brightnessNorm; if (g[3][x, y] > 1.0f) g[3][x, y] = 1.0f; if (g[3][x, y] < 0) g[3][x, y] = 0;
                            b[3][x, y] += lightColor.X * brightnessNorm; if (b[3][x, y] > 1.0f) b[3][x, y] = 1.0f; if (b[3][x, y] < 0) b[3][x, y] = 0;

                            luxelColor0 = new CoordinateF(r[0][x, y], g[0][x, y], b[0][x, y]);
                            luxelColor1 = new CoordinateF(r[1][x, y], g[1][x, y], b[1][x, y]);
                            luxelColor2 = new CoordinateF(r[2][x, y], g[2][x, y], b[2][x, y]);
                            luxelColorNorm = new CoordinateF(r[3][x, y], g[3][x, y], b[3][x, y]);

                            if (tX >= 0 && tY >= 0 && tX < TextureDims && tY < TextureDims)
                            {
                                int offset = (tX + tY * TextureDims) * Bitmap.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format32bppArgb) / 8;
                                if (luxelColor0.X + luxelColor0.Y + luxelColor0.Z > bitmaps[0][offset + 2] + bitmaps[0][offset + 1] + bitmaps[0][offset + 0])
                                {
                                    bitmaps[0][offset + 0] = luxelColor0.X;
                                    bitmaps[0][offset + 1] = luxelColor0.Y;
                                    bitmaps[0][offset + 2] = luxelColor0.Z;
                                }
                                if (luxelColor1.X + luxelColor1.Y + luxelColor1.Z > bitmaps[1][offset + 2] + bitmaps[1][offset + 1] + bitmaps[1][offset + 0])
                                {
                                    bitmaps[1][offset + 0] = luxelColor1.X;
                                    bitmaps[1][offset + 1] = luxelColor1.Y;
                                    bitmaps[1][offset + 2] = luxelColor1.Z;
                                }
                                if (luxelColor2.X + luxelColor2.Y + luxelColor2.Z > bitmaps[2][offset + 2] + bitmaps[2][offset + 1] + bitmaps[2][offset + 0])
                                {
                                    bitmaps[2][offset + 0] = luxelColor2.X;
                                    bitmaps[2][offset + 1] = luxelColor2.Y;
                                    bitmaps[2][offset + 2] = luxelColor2.Z;
                                }
                                if (luxelColorNorm.X + luxelColorNorm.Y + luxelColorNorm.Z > bitmaps[3][offset + 2] + bitmaps[3][offset + 1] + bitmaps[3][offset + 0])
                                {
                                    bitmaps[3][offset + 0] = luxelColorNorm.X;
                                    bitmaps[3][offset + 1] = luxelColorNorm.Y;
                                    bitmaps[3][offset + 2] = luxelColorNorm.Z;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
