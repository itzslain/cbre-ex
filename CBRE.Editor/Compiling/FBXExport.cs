using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Documents;
using CBRE.Providers.Model;
using OpenTK;
using System.Collections.Generic;
using System.Linq;

namespace CBRE.Editor.Compiling
{
    class FBXExport
    {
        public static void SaveToFile(string filename,Document document,ExportForm form)
        {
            IEnumerable<Face> faces = document.Map.WorldSpawn.Find(x => x is Solid).OfType<Solid>().SelectMany(x => x.Faces).Where(x => x.Texture.Name != "tooltextures/remove_face");
            List<float> verts = new List<float>();
            foreach (Face f in faces) {
                foreach (Vertex i in f.Vertices)
                {
                    //verts.Add(i.x);
                    //verts.Add(i.y);
                    //verts.Add(i.z);
                    // vertex coords???
                }
            }
            AssimpProvider.SaveToFile(filename, verts.ToArray());
        }
    }
}
