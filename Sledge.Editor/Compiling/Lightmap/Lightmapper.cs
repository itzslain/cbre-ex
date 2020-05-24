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
using Sledge.Editor.Documents;
using Sledge.Graphics.Helpers;
using System.Windows.Forms;

namespace Sledge.Editor.Compiling.Lightmap
{
    static class Lightmapper
    {
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

        public static List<Thread> FaceRenderThreads { get; private set; } 
        private static List<LMThreadException> threadExceptions;

        private static void UpdateProgress(ProgressBar progressBar, RichTextBox progressLog, string msg, float progress)
        {
            progressLog.Invoke((MethodInvoker)(() => progressLog.AppendText("\n" + msg)));
            progressBar.Invoke((MethodInvoker)(() => progressBar.Value = (int)(progress * 10000)));
        }


        private static void CalculateUV(List<LightmapGroup> lmGroups, Rectangle area, out int usedWidth, out int usedHeight)
        {
            usedWidth = 0;
            usedHeight = 0;
            if (lmGroups.Count <= 0) { return; }

            for (int i = 0; i < lmGroups.Count; i++)
            {
                LightmapGroup lmGroup = lmGroups[i];

                if ((area.Width <= area.Height) != (lmGroup.Width <= lmGroup.Height))
                {
                    lmGroup.SwapUV();
                }

                for (int j = 0; j < 2; j++)
                {
                    int downscaledWidth = (int)Math.Ceiling(lmGroup.Width / Config.DownscaleFactor);
                    int downscaledHeight = (int)Math.Ceiling(lmGroup.Height / Config.DownscaleFactor);

                    if (downscaledWidth <= area.Width && downscaledHeight <= area.Height)
                    {
                        usedWidth += downscaledWidth;
                        usedHeight += downscaledHeight;
                        lmGroups.RemoveAt(i);
                        lmGroup.writeX = area.Left;
                        lmGroup.writeY = area.Top;

                        int subWidth = -1; int subHeight = -1;
                        if (downscaledWidth < area.Width)
                        {
                            int subUsedWidth = 0;
                            while (subWidth != 0)
                            {
                                CalculateUV(lmGroups, new Rectangle(area.Left + subUsedWidth + downscaledWidth + Config.PlaneMargin,
                                                                    area.Top,
                                                                    area.Width - subUsedWidth - downscaledWidth - Config.PlaneMargin,
                                                                    downscaledHeight),
                                            out subWidth, out subHeight);
                                subUsedWidth += subWidth + Config.PlaneMargin;
                            }

                            usedWidth += subUsedWidth;
                            subWidth = -1; subHeight = -1;
                        }

                        if (downscaledHeight < area.Height)
                        {
                            int subUsedHeight = 0;
                            while (subHeight != 0)
                            {
                                CalculateUV(lmGroups, new Rectangle(area.Left,
                                                                    area.Top + subUsedHeight + downscaledHeight + Config.PlaneMargin,
                                                                    downscaledWidth,
                                                                    area.Height - subUsedHeight - downscaledHeight - Config.PlaneMargin),
                                            out subWidth, out subHeight);
                                subUsedHeight += subHeight + Config.PlaneMargin;
                            }

                            usedHeight += subUsedHeight;
                        }

                        if (downscaledWidth < area.Width && downscaledHeight < area.Height)
                        {
                            Rectangle remainder = new Rectangle(area.Left + downscaledWidth + Config.PlaneMargin,
                                                            area.Top + downscaledHeight + Config.PlaneMargin,
                                                            area.Width - downscaledWidth - Config.PlaneMargin,
                                                            area.Height - downscaledHeight - Config.PlaneMargin);

                            CalculateUV(lmGroups, remainder,
                                            out subWidth, out subHeight);

                            usedWidth += subWidth;
                            usedHeight += subHeight;
                        }

                        return;
                    }

                    lmGroup.SwapUV();
                }
            }
        }

        public static void Render(Document document, ProgressBar progressBar, RichTextBox progressLog, out List<LMFace> faces)
        {
            var textureCollection = document.TextureCollection;

            progressBar.Invoke((MethodInvoker)(() => progressBar.Maximum = 10000));
            var map = document.Map;

            faces = new List<LMFace>();
            var lightEntities = new List<Light>();
            lock (textureCollection.Lightmaps)
            {
                for (int i = 0; i < 4; i++)
                {
                    textureCollection.Lightmaps[i]?.Dispose();
                    textureCollection.Lightmaps[i] = new Bitmap(Config.TextureDims, Config.TextureDims);
                }
            }

            threadExceptions = new List<LMThreadException>();

            List<LightmapGroup> lmGroups = new List<LightmapGroup>();
            List<LMFace> exclusiveBlockers = new List<LMFace>();

            //get faces
            UpdateProgress(progressBar, progressLog, "Determining UV coordinates...", 0);
            LMFace.FindFacesAndGroups(map, out faces, out lmGroups);

            if (!lmGroups.Any()) { throw new Exception("No lightmap groups!"); }

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

                if (x.Width * x.Height < y.Width * y.Height) { return 1; }
                return -1;
            });

            float[][] buffers = new float[4][];
            lock (textureCollection.Lightmaps)
            {
                buffers[0] = new float[textureCollection.Lightmaps[0].Width * textureCollection.Lightmaps[0].Height * Bitmap.GetPixelFormatSize(PixelFormat.Format32bppArgb) / 8];
                buffers[1] = new float[textureCollection.Lightmaps[1].Width * textureCollection.Lightmaps[1].Height * Bitmap.GetPixelFormatSize(PixelFormat.Format32bppArgb) / 8];
                buffers[2] = new float[textureCollection.Lightmaps[2].Width * textureCollection.Lightmaps[2].Height * Bitmap.GetPixelFormatSize(PixelFormat.Format32bppArgb) / 8];
                buffers[3] = new float[textureCollection.Lightmaps[3].Width * textureCollection.Lightmaps[3].Height * Bitmap.GetPixelFormatSize(PixelFormat.Format32bppArgb) / 8];
            }

            FaceRenderThreads = new List<Thread>();

            Light.FindLights(map, out lightEntities);

            List<LMFace> allFaces = lmGroups.Select(q => q.Faces).SelectMany(q => q).Union(exclusiveBlockers).ToList();
            int faceCount = 0;

            List<LightmapGroup> uvCalcFaces = new List<LightmapGroup>(lmGroups);
            CalculateUV(uvCalcFaces, new Rectangle(1, 1, Config.TextureDims - 2, Config.TextureDims - 2), out _, out _);

            if (uvCalcFaces.Count > 0)
            {
                throw new Exception("UV coordinates out of bounds: try increasing downscale factor or increasing texture dimensions");
            }

            foreach (LightmapGroup group in lmGroups)
            {
                foreach (LMFace face in group.Faces)
                {
                    faceCount++;
                    Thread newThread = CreateLightmapRenderThread(buffers, lightEntities, group.writeX, group.writeY, group, face, allFaces);
                    FaceRenderThreads.Add(newThread);
                }
            }

            int faceNum = 0;
            UpdateProgress(progressBar, progressLog, "Started calculating brightness levels...", 0.05f);
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
                        UpdateProgress(progressBar, progressLog, faceNum.ToString() + "/" + faceCount.ToString() + " faces complete", 0.05f + ((float)faceNum / (float)faceCount) * 0.85f);
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
            UpdateProgress(progressBar, progressLog, "Blurring lightmap...", 0.95f);
            float[] blurBuffer = new float[buffers[0].Length];
            for (int k = 0; k < 4; k++)
            {
                foreach (LightmapGroup group in lmGroups)
                {
                    int downscaledWidth = (int)Math.Ceiling(group.Width / Config.DownscaleFactor);
                    int downscaledHeight = (int)Math.Ceiling(group.Height / Config.DownscaleFactor);

                    float ambientMultiplier = (group.Plane.Normal.Dot(Config.AmbientNormal) + 1.5f) * 0.4f;
                    CoordinateF mAmbientColor = new CoordinateF((Config.AmbientColor.B * ambientMultiplier / 255.0f),
                                                            (Config.AmbientColor.G * ambientMultiplier / 255.0f),
                                                            (Config.AmbientColor.R * ambientMultiplier / 255.0f));
                    for (int y = group.writeY; y < group.writeY + downscaledHeight; y++)
                    {
                        if (y < 0 || y >= Config.TextureDims) continue;
                        for (int x = group.writeX; x < group.writeX + downscaledWidth; x++)
                        {
                            if (x < 0 || x >= Config.TextureDims) continue;
                            int offset = (x + y * Config.TextureDims) * System.Drawing.Image.GetPixelFormatSize(PixelFormat.Format32bppArgb) / 8;

                            float accumRed = 0;
                            float accumGreen = 0;
                            float accumBlue = 0;
                            int sampleCount = 0;
                            for (int j = -Config.BlurRadius; j <= Config.BlurRadius; j++)
                            {
                                if (y + j < 0 || y + j >= Config.TextureDims) continue;
                                if (y + j < group.writeY || y + j >= group.writeY + downscaledHeight) continue;
                                for (int i = -Config.BlurRadius; i <= Config.BlurRadius; i++)
                                {
                                    if (i * i + j * j > Config.BlurRadius * Config.BlurRadius) continue;
                                    if (x + i < 0 || x + i >= Config.TextureDims) continue;
                                    if (x + i < group.writeX || x + i >= group.writeX + downscaledWidth) continue;
                                    int sampleOffset = ((x + i) + (y + j) * Config.TextureDims) * System.Drawing.Image.GetPixelFormatSize(PixelFormat.Format32bppArgb) / 8;
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
                if (i % 4 == 3)
                {
                    buffers[0][i] = 1.0f;
                    buffers[1][i] = 1.0f;
                    buffers[2][i] = 1.0f;
                    buffers[3][i] = 1.0f;
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

            for (int k = 0; k < 4; k++)
            {
                byte[] byteBuffer = new byte[buffers[k].Length];
                for (int i = 0; i < buffers[k].Length; i++)
                {
                    byteBuffer[i] = (byte)Math.Max(Math.Min(buffers[k][i] * 255.0f, 255.0f), 0.0f);
                }
                lock (textureCollection.Lightmaps)
                {
                    UpdateProgress(progressBar, progressLog, "Copying bitmap data...", 0.99f);
                    BitmapData bitmapData2 = textureCollection.Lightmaps[k].LockBits(new Rectangle(0, 0, Config.TextureDims, Config.TextureDims), ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    Marshal.Copy(byteBuffer, 0, bitmapData2.Scan0, byteBuffer.Length);
                    textureCollection.Lightmaps[k].UnlockBits(bitmapData2);
                }
            }

            faces.Clear();
            faces.AddRange(lmGroups.SelectMany(g => g.Faces));

            lock (textureCollection.Lightmaps)
            {
                document.TextureCollection.LightmapTextureOutdated = true;
            }

            UpdateProgress(progressBar, progressLog, "Lightmapping complete!", 1.0f);
        }

        public static void SaveLightmaps(Document document, string path, bool threeBasisModel)
        {
            lock (document.TextureCollection.Lightmaps)
            {
                if (threeBasisModel)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        document.TextureCollection.Lightmaps[i].Save(path + i.ToString() + ".png");
                    }
                }
                else
                {
                    document.TextureCollection.Lightmaps[3].Save(path + ".png");
                }
            }
        }

        private static Thread CreateLightmapRenderThread(float[][] bitmaps, List<Light> lights, int writeX, int writeY, LightmapGroup group, LMFace targetFace, List<LMFace> blockerFaces)
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

        private static void RenderLightOntoFace(float[][] bitmaps, List<Light> lights, int writeX, int writeY, LightmapGroup group, LMFace targetFace, List<LMFace> blockerFaces)
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

            minX -= Config.DownscaleFactor; minY -= Config.DownscaleFactor;
            maxX += Config.DownscaleFactor; maxY += Config.DownscaleFactor;

            minX /= Config.DownscaleFactor; minX = (float)Math.Ceiling(minX.Value); minX *= Config.DownscaleFactor;
            minY /= Config.DownscaleFactor; minY = (float)Math.Ceiling(minY.Value); minY *= Config.DownscaleFactor;
            maxX /= Config.DownscaleFactor; maxX = (float)Math.Ceiling(maxX.Value); maxX *= Config.DownscaleFactor;
            maxY /= Config.DownscaleFactor; maxY = (float)Math.Ceiling(maxY.Value); maxY *= Config.DownscaleFactor;

            foreach (LMFace.Vertex vert in targetFace.Vertices)
            {
                float x = vert.Location.Dot(group.uAxis);
                float y = vert.Location.Dot(group.vAxis);

                float u = (writeX + 0.5f + (x - group.minTotalX.Value) / Config.DownscaleFactor) / Config.TextureDims;
                float v = (writeY + 0.5f + (y - group.minTotalY.Value) / Config.DownscaleFactor) / Config.TextureDims;

                vert.LMU = u; vert.LMV = v;
                vert.OriginalVertex.LMU = u; vert.OriginalVertex.LMV = v;
            }

            float centerX = (maxX.Value + minX.Value) / 2;
            float centerY = (maxY.Value + minY.Value) / 2;

            int iterX = (int)Math.Ceiling((maxX.Value - minX.Value) / Config.DownscaleFactor);
            int iterY = (int)Math.Ceiling((maxY.Value - minY.Value) / Config.DownscaleFactor);

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

            foreach (Light light in lights)
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

                for (int y = 0; y < iterY; y++)
                {
                    for (int x = 0; x < iterX; x++)
                    {
                        int tX = (int)(writeX + x + (int)(minX - group.minTotalX) / Config.DownscaleFactor);
                        int tY = (int)(writeY + y + (int)(minY - group.minTotalY) / Config.DownscaleFactor);

                        if (tX >= 0 && tY >= 0 && tX < Config.TextureDims && tY < Config.TextureDims)
                        {
                            int offset = (tX + tY * Config.TextureDims) * Bitmap.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format32bppArgb) / 8;
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
                        float ttX = minX.Value + (x * Config.DownscaleFactor);
                        float ttY = minY.Value + (y * Config.DownscaleFactor);
                        CoordinateF pointOnPlane = (ttX - centerX) * group.uAxis + (ttY - centerY) * group.vAxis + targetFace.BoundingBox.Center;

                        int tX = (int)(writeX + x + (int)(minX - group.minTotalX) / Config.DownscaleFactor);
                        int tY = (int)(writeY + y + (int)(minY - group.minTotalY) / Config.DownscaleFactor);

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

                            if (tX >= 0 && tY >= 0 && tX < Config.TextureDims && tY < Config.TextureDims)
                            {
                                int offset = (tX + tY * Config.TextureDims) * Bitmap.GetPixelFormatSize(System.Drawing.Imaging.PixelFormat.Format32bppArgb) / 8;
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
