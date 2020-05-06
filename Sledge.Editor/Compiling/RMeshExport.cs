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
using System.Windows.Forms;
using Sledge.Editor.Documents;
using Sledge.Settings;
using Sledge.Editor.Compiling.Lightmap;

namespace Sledge.Editor.Compiling
{
    public class RMeshExport
    {
        public class Waypoint
        {
            public Waypoint(Entity ent)
            {
                Location = new CoordinateF(ent.Origin);
            }

            public CoordinateF Location;
        }

        enum RMeshLoadFlags
        {
            COLOR = 1,
            ALPHA = 2
        };

        enum RMeshBlendFlags
        {
            NORMAL = 0,
            DIFFUSE = 1,
            LM = 2
        };

        private static void WriteByteString(BinaryWriter writer,string str)
        {
            writer.Write((byte)str.Length);
            for (int i=0;i<str.Length;i++)
            {
                writer.Write((byte)str[i]);
            }
        }
        
        public static void SaveToFile(string filename,Document document,ExportForm form)
        {
            var map = document.Map;
            string filepath = System.IO.Path.GetDirectoryName(filename);
            filename = System.IO.Path.GetFileName(filename);
            filename = System.IO.Path.GetFileNameWithoutExtension(filename)+".rmesh";
            string lmPath = System.IO.Path.GetFileNameWithoutExtension(filename) + "_lm";

            List<Lightmap.LMFace> faces;
            List<Lightmap.Light> lights;
            Lightmap.Lightmapper.Render(document, form.ProgressBar, form.ProgressLog, out faces);
            Lightmap.Light.FindLights(map, out lights);

            IEnumerable<Face> transparentFaces = map.WorldSpawn.Find(x => x is Solid).OfType<Solid>().SelectMany(x => x.Faces).Where(x =>
            {
                if (!x.Texture.Texture.HasTransparency()) return false;
                if (x.Texture.Name.Contains("tooltextures")) return false;

                return true;
            });

            IEnumerable<Face> invisibleCollisionFaces = map.WorldSpawn.Find(x => x is Solid).OfType<Solid>().SelectMany(x => x.Faces).Where(x => x.Texture.Name=="tooltextures/invisible_collision");

            string dir = Sledge.Settings.Directories.TextureDir;
            if (dir.Last() != '/' && dir.Last() != '\\') dir += "/";
            Lightmap.Lightmapper.SaveLightmaps(document, filepath + "/" + lmPath, false);
            lmPath = System.IO.Path.GetFileName(lmPath);

            List<Waypoint> waypoints = map.WorldSpawn.Find(x => x.ClassName!=null && x.ClassName.ToLower() == "waypoint").OfType<Entity>().Select(x => new Waypoint(x)).ToList();

            IEnumerable<Entity> soundEmitters = map.WorldSpawn.Find(x => x.ClassName != null && x.ClassName.ToLower() == "soundemitter").OfType<Entity>();

            IEnumerable<Entity> props = map.WorldSpawn.Find(x => x.ClassName != null && x.ClassName.ToLower() == "model").OfType<Entity>();

            FileStream stream = new FileStream(filepath + "/" + filename, FileMode.Create);
            BinaryWriter br = new BinaryWriter(stream);

            //header
            br.Write((Int32)8);
            br.Write((byte)'R');
            br.Write((byte)'o');
            br.Write((byte)'o');
            br.Write((byte)'m');
            br.Write((byte)'M');
            br.Write((byte)'e');
            br.Write((byte)'s');
            br.Write((byte)'h');

            //textures
            string texDir = Directories.TextureDir;
            if (texDir[texDir.Length - 1] != '/' && texDir[texDir.Length - 1] != '\\') texDir += "/";

            List<Tuple<string, RMeshLoadFlags, RMeshBlendFlags, byte>> textures = new List<Tuple<string, RMeshLoadFlags, RMeshBlendFlags, byte>>();
            RMeshLoadFlags loadFlag = RMeshLoadFlags.COLOR; RMeshBlendFlags blendFlag = RMeshBlendFlags.DIFFUSE;
            foreach (LMFace face in faces)
            {
                if (!textures.Any(x => x.Item1 == face.Texture)) textures.Add(new Tuple<string, RMeshLoadFlags, RMeshBlendFlags, byte>(face.Texture, loadFlag, blendFlag, 0));
            }
            loadFlag = RMeshLoadFlags.ALPHA; blendFlag = RMeshBlendFlags.NORMAL;
            foreach (Face face in transparentFaces)
            {
                if (!textures.Any(x => x.Item1 == face.Texture.Name)) textures.Add(new Tuple<string, RMeshLoadFlags, RMeshBlendFlags, byte>(face.Texture.Name, loadFlag, blendFlag, 0));
            }
            loadFlag = RMeshLoadFlags.COLOR; blendFlag = RMeshBlendFlags.LM;
            textures.Add(new Tuple<string, RMeshLoadFlags, RMeshBlendFlags, byte>(lmPath, loadFlag, blendFlag, 1));

            //mesh

            int vertCount;
            int vertOffset;
            int triCount;

            //TODO: find a clever way of splitting up meshes with the same texture
            //into several for collision optimization.
            //Making each face its own collision object is too slow, and merging all of
            //them together is not optimal either.

            br.Write((Int32)(textures.Count - 1));

            for (int i = 0; i < textures.Count - 1; i++)
            {
                IEnumerable<LMFace> tLmFaces = faces.FindAll(x => x.Texture == textures[i].Item1);
                IEnumerable<Face> tTrptFaces = transparentFaces.Where(x => x.Texture.Name == textures[i].Item1);
                vertCount = 0;
                vertOffset = 0;
                triCount = 0;

                string texName = "";
                if (File.Exists(texDir + textures[i].Item1 + ".png")) texName = textures[i].Item1 + ".png";
                if (File.Exists(texDir + textures[i].Item1 + ".jpg")) texName = textures[i].Item1 + ".jpg";

                if (tLmFaces.Count() > 0)
                {
                    foreach (LMFace face in tLmFaces)
                    {
                        vertCount += face.Vertices.Count;
                        triCount += face.GetTriangleIndices().Count() / 3;
                    }

                    byte flag = 1;
                    br.Write(flag);
                    br.Write((Int32)(textures[textures.Count - 1].Item1 + ".png").Length);
                    for (int k = 0; k < (textures[textures.Count - 1].Item1 + ".png").Length; k++)
                    {
                        br.Write((byte)(textures[textures.Count - 1].Item1 + ".png")[k]);
                    }
                    flag = 1;
                    br.Write(flag);
                    br.Write((Int32)texName.Length);
                    for (int k = 0; k < texName.Length; k++)
                    {
                        br.Write((byte)texName[k]);
                    }

                    if (vertCount > short.MaxValue) throw new Exception("Vertex overflow!");
                    br.Write((Int32)vertCount);
                    foreach (LMFace face in tLmFaces)
                    {
                        for (int j = 0; j < face.Vertices.Count; j++)
                        {
                            br.Write(face.Vertices[j].Location.X);
                            br.Write(face.Vertices[j].Location.Z);
                            br.Write(face.Vertices[j].Location.Y);

                            br.Write(face.Vertices[j].DiffU);
                            br.Write(face.Vertices[j].DiffV);
                            br.Write(face.Vertices[j].LMU);
                            br.Write(face.Vertices[j].LMV);

                            br.Write((byte)255); //r
                            br.Write((byte)255); //g
                            br.Write((byte)255); //b
                        }
                    }
                    br.Write((Int32)triCount);
                    foreach (LMFace face in tLmFaces)
                    {
                        foreach (uint ind in face.GetTriangleIndices())
                        {
                            br.Write((Int32)(ind + vertOffset));
                        }

                        vertOffset += face.Vertices.Count;
                    }
                }
                else if (tTrptFaces.Count() > 0)
                {
                    foreach (Face face in tTrptFaces)
                    {
                        vertCount += face.Vertices.Count;
                        triCount += face.GetTriangleIndices().Count() / 3;
                    }

                    byte flag = 0;
                    br.Write(flag);
                    flag = 3;
                    br.Write(flag);
                    br.Write((Int32)texName.Length);
                    for (int k = 0; k < texName.Length; k++)
                    {
                        br.Write((byte)texName[k]);
                    }

                    if (vertCount > short.MaxValue) throw new Exception("Vertex overflow!");
                    br.Write((Int32)vertCount);
                    foreach (Face face in tTrptFaces)
                    {
                        for (int j = 0; j < face.Vertices.Count; j++)
                        {
                            br.Write((float)face.Vertices[j].Location.X);
                            br.Write((float)face.Vertices[j].Location.Z);
                            br.Write((float)face.Vertices[j].Location.Y);

                            br.Write(0.0f);
                            br.Write(0.0f);
                            br.Write((float)face.Vertices[j].TextureU);
                            br.Write((float)face.Vertices[j].TextureV);

                            br.Write((byte)255); //r
                            br.Write((byte)255); //g
                            br.Write((byte)255); //b
                        }
                    }
                    br.Write((Int32)triCount);
                    foreach (Face face in tTrptFaces)
                    {
                        foreach (uint ind in face.GetTriangleIndices())
                        {
                            br.Write((Int32)(ind + vertOffset));
                        }

                        vertOffset += face.Vertices.Count;
                    }
                }
            }

            vertCount = 0;
            vertOffset = 0;
            triCount = 0;
            if (invisibleCollisionFaces.Count() > 0)
            {
                br.Write((Int32)1);

                foreach (Face face in invisibleCollisionFaces)
                {
                    vertCount += face.Vertices.Count;
                    triCount += face.GetTriangleIndices().Count() / 3;
                }

                if (vertCount > short.MaxValue) throw new Exception("Vertex overflow!");
                br.Write((Int32)vertCount);
                foreach (Face face in invisibleCollisionFaces)
                {
                    for (int j = 0; j < face.Vertices.Count; j++)
                    {
                        br.Write(face.Vertices[j].Location.X);
                        br.Write(face.Vertices[j].Location.Z);
                        br.Write(face.Vertices[j].Location.Y);
                    }
                }
                br.Write((Int32)triCount);
                foreach (Face face in invisibleCollisionFaces)
                {
                    foreach (uint ind in face.GetTriangleIndices())
                    {
                        br.Write((Int32)(ind + vertOffset));
                    }

                    vertOffset += face.Vertices.Count;
                }
            }
            else
            {
                br.Write((Int32)0);
            }

            br.Write((Int32)(lights.Count + waypoints.Count + soundEmitters.Count() + props.Count()));

            foreach (Light light in lights)
            {
                br.Write((Int32)5);
                br.Write((byte)'l');
                br.Write((byte)'i');
                br.Write((byte)'g');
                br.Write((byte)'h');
                br.Write((byte)'t');

                br.Write(light.Origin.X);
                br.Write(light.Origin.Z);
                br.Write(light.Origin.Y);

                br.Write(light.Range);

                string lcolor = light.Color.X + " " + light.Color.Y + " " + light.Color.Z;
                br.Write((Int32)lcolor.Length);
                for (int k = 0; k < lcolor.Length; k++)
                {
                    br.Write((byte)lcolor[k]);
                }

                br.Write(1.0f); //intensity
            }

            foreach (Waypoint wp in waypoints)
            {
                br.Write((Int32)8);
                br.Write((byte)'w');
                br.Write((byte)'a');
                br.Write((byte)'y');
                br.Write((byte)'p');
                br.Write((byte)'o');
                br.Write((byte)'i');
                br.Write((byte)'n');
                br.Write((byte)'t');

                br.Write(wp.Location.X);
                br.Write(wp.Location.Z);
                br.Write(wp.Location.Y);
            }

            foreach (Entity soundEmitter in soundEmitters)
            {
                br.Write((Int32)12);
                br.Write((byte)'s');
                br.Write((byte)'o');
                br.Write((byte)'u');
                br.Write((byte)'n');
                br.Write((byte)'d');
                br.Write((byte)'e');
                br.Write((byte)'m');
                br.Write((byte)'i');
                br.Write((byte)'t');
                br.Write((byte)'t');
                br.Write((byte)'e');
                br.Write((byte)'r');

                br.Write((float)soundEmitter.Origin.X);
                br.Write((float)soundEmitter.Origin.Z);
                br.Write((float)soundEmitter.Origin.Y);

                br.Write((Int32)int.Parse(soundEmitter.EntityData.GetPropertyValue("sound")));

                br.Write(float.Parse(soundEmitter.EntityData.GetPropertyValue("range")));
            }

            foreach (Entity prop in props)
            {
                br.Write((Int32)5);
                br.Write((byte)'m');
                br.Write((byte)'o');
                br.Write((byte)'d');
                br.Write((byte)'e');
                br.Write((byte)'l');

                string modelName = prop.EntityData.GetPropertyValue("file") + ".x";
                br.Write((Int32)modelName.Length);
                for (int k = 0; k < modelName.Length; k++)
                {
                    br.Write((byte)modelName[k]);
                }

                br.Write((float)prop.Origin.X);
                br.Write((float)prop.Origin.Z);
                br.Write((float)prop.Origin.Y);

                Coordinate rotation = prop.EntityData.GetPropertyCoordinate("angles");
                br.Write((float)rotation.X);
                br.Write((float)rotation.Y);
                br.Write((float)rotation.Z);

                Coordinate scale = prop.EntityData.GetPropertyCoordinate("scale");
                br.Write((float)scale.X);
                br.Write((float)scale.Z);
                br.Write((float)scale.Y);
            }

            br.Dispose();
            stream.Dispose();

            form.ProgressLog.Invoke((MethodInvoker)(() => form.ProgressLog.AppendText("\nDone!")));
            form.ProgressBar.Invoke((MethodInvoker)(() => form.ProgressBar.Value = 10000));
        }
    }
}
