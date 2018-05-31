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

namespace Sledge.Editor.Compiling
{
    public class RM2Export
    {
        public class Waypoint
        {
            public Waypoint(Entity ent)
            {
                Location = new CoordinateF(ent.Origin);
                Connections = new List<int>();
            }

            public List<int> Connections;
            public CoordinateF Location;
        }

        enum RM2Parts
        {
            TEXTURES = 1,
            OPAQUE = 2,
            ALPHA = 3,
            INVISIBLE = 4,
            SCREEN = 5,
            WAYPOINT = 6,
            POINTLIGHT = 7,
            SPOTLIGHT = 8,
            SOUNDEMITTER = 9,
            PROP = 10,
        };
        
        enum RM2LoadFlags
        {
            COLOR = 1,
            ALPHA = 2
        };
        
        enum RM2BlendFlags
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
        
        public static void SaveToFile(string filename,Map map,RM2ExportForm form)
        {
            filename = System.IO.Path.GetFileNameWithoutExtension(filename)+".rm2";
            string lmPath = System.IO.Path.GetFileNameWithoutExtension(filename) + "_lm";

            List<Lightmapper.LMFace> faces = new List<Lightmapper.LMFace>();
            List<Lightmapper.LMLight> lights = new List<Lightmapper.LMLight>();
            Bitmap bitmap = new Bitmap(Lightmapper.TextureDims,Lightmapper.TextureDims);
            form.ProgressBar.Invoke((MethodInvoker)(() => form.ProgressBar.Maximum = 10000));
            foreach (Tuple<string,float> progress in Lightmapper.Render(map,bitmap,faces,lights))
            {
                form.ProgressLabel.Invoke((MethodInvoker)(() => form.ProgressLabel.Text = progress.Item1));
                form.ProgressBar.Invoke((MethodInvoker)(() => form.ProgressBar.Value = (int)(progress.Item2 * 9000)));
            }

            string dir = Sledge.Settings.Directories.TextureDir;
            if (dir.Last() != '/' && dir.Last() != '\\') dir += "/";
            bitmap.Save(lmPath+".png");
            lmPath = System.IO.Path.GetFileName(lmPath);

            List<Waypoint> waypoints = map.WorldSpawn.Find(x => x.ClassName!=null && x.ClassName.ToLower() == "waypoint").OfType<Entity>().Select(x => new Waypoint(x)).ToList();

            FileStream stream = new FileStream(filename, FileMode.Create);
            BinaryWriter br = new BinaryWriter(stream);

            //header
            br.Write((byte)'.');
            br.Write((byte)'R');
            br.Write((byte)'M');
            br.Write((byte)'2');

            //non-lightmapped faces
            IEnumerable<Face> nonLMFaces = map.WorldSpawn.Find(x => x is Face).OfType<Face>();

            //textures
            List<string> textures = new List<string>();
            foreach (Lightmapper.LMFace face in faces)
            {
                if (!textures.Contains(face.Texture)) textures.Add(face.Texture);
            }
            textures.Add(lmPath);

            br.Write((byte)RM2Parts.TEXTURES);
            br.Write((byte)textures.Count);
            foreach (string tex in textures)
            {
                WriteByteString(br, tex);
                if (tex == lmPath)
                {
                    br.Write((byte)(((int)(RM2LoadFlags.COLOR) << 4) | (int)RM2BlendFlags.LM));
                    br.Write((byte)1);
                }
                else
                {
                    br.Write((byte)(((int)(RM2LoadFlags.COLOR) << 4) | (int)RM2BlendFlags.DIFFUSE));
                    br.Write((byte)0);
                }
            }

            //mesh
            foreach (Lightmapper.LMFace face in faces)
            {
                br.Write((byte)RM2Parts.OPAQUE);
                int texInd = textures.FindIndex(x => x==face.Texture);
                br.Write((byte)textures.Count);
                br.Write((byte)(texInd + 1));

                br.Write((short)face.Vertices.Count);
                for (int i=0;i<face.Vertices.Count;i++)
                {
                    br.Write(face.Vertices[i].Location.X);
                    br.Write(face.Vertices[i].Location.Z);
                    br.Write(face.Vertices[i].Location.Y);

                    br.Write((byte)255); //r
                    br.Write((byte)255); //g
                    br.Write((byte)255); //b

                    br.Write(face.Vertices[i].DiffU);
                    br.Write(face.Vertices[i].DiffV);
                    br.Write(face.Vertices[i].LMU);
                    br.Write(face.Vertices[i].LMV);
                }

                List<uint> indices = face.GetTriangleIndices().ToList();
                br.Write((short)(indices.Count / 3));
                for (int i=0;i<indices.Count;i++)
                {
                    br.Write((short)indices[i]);
                }
            }

            br.Dispose();
            stream.Dispose();

            form.ProgressLabel.Invoke((MethodInvoker)(() => form.ProgressLabel.Text = "Done!"));
            form.ProgressBar.Invoke((MethodInvoker)(() => form.ProgressBar.Value = 10000));
        }
    }
}
