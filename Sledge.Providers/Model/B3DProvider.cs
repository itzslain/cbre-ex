using Sledge.DataStructures.Models;
using Sledge.DataStructures.Geometric;
using Sledge.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sledge.Providers.Model
{
    public class B3DProvider : ModelProvider
    {
        protected override bool IsValidForFile(IFile file)
        {
            return file.Extension.ToLowerInvariant() == "b3d";
        }

        protected static string ReadChunk(BinaryReader reader, DataStructures.Models.Model model)
        {
            string header = reader.ReadFixedLengthString(Encoding.ASCII,4);
            int size = reader.ReadInt32();
            int initialPos = (int)reader.BaseStream.Position;

            if (header == "NODE")
            {
                string name = reader.ReadNullTerminatedString();
                float posX = reader.ReadSingle(); float posY = reader.ReadSingle(); float posZ = reader.ReadSingle();
                float scaleX = reader.ReadSingle(); float scaleY = reader.ReadSingle(); float scaleZ = reader.ReadSingle();
                float rotX = reader.ReadSingle(); float rotY = reader.ReadSingle(); float rotZ = reader.ReadSingle(); float rotW = reader.ReadSingle();

                ReadChunk(reader, model);

                reader.ReadBytes(size - ((int)reader.BaseStream.Position - initialPos));
            }
            else if (header == "MESH")
            {
                int brushID = reader.ReadInt32();

                string vertsHeader = reader.ReadFixedLengthString(Encoding.ASCII, 4);
                int vertsSize = reader.ReadInt32();

                int initialVertPos = (int)reader.BaseStream.Position;

                int vertFlags = reader.ReadInt32();
                int tex_coord_sets = reader.ReadInt32();
                int tex_coord_set_size = reader.ReadInt32();

                Mesh mesh = new Mesh(0);
                List<MeshVertex> vertices = new List<MeshVertex>();

                while (reader.BaseStream.Position - initialVertPos < vertsSize)
                {
                    float x = reader.ReadSingle(); float y = reader.ReadSingle(); float z = reader.ReadSingle();
                    float normalX = 0.0f; float normalY = 1.0f; float normalZ = 0.0f;
                    if ((vertFlags&1) != 0)
                    {
                        normalX = reader.ReadSingle(); normalY = reader.ReadSingle(); normalZ = reader.ReadSingle();
                    }
                    float r; float g; float b; float a;
                    if ((vertFlags&2) != 0)
                    {
                        r = reader.ReadSingle(); g = reader.ReadSingle(); b = reader.ReadSingle(); a = reader.ReadSingle();
                    }

                    float u = 0.0f; float v = 0.0f;
                    if (tex_coord_sets>0)
                    {
                        u = reader.ReadSingle(); v = reader.ReadSingle();
                        for (int j = 0;j < tex_coord_set_size-2;j++)
                        {
                            reader.ReadSingle();
                        }
                        for (int i = 0; i < tex_coord_sets - 1; i++)
                        {
                            for (int j = 0; j < tex_coord_set_size; j++)
                            {
                                reader.ReadSingle();
                            }
                        }
                    }

                    vertices.Add(new MeshVertex(new CoordinateF(x, y, z), new CoordinateF(normalX, normalY, normalZ), model.Bones[0], u, v));
                }

                while (reader.BaseStream.Position - initialPos < size)
                {
                    string trisHeader = reader.ReadFixedLengthString(Encoding.ASCII, 4);
                    int trisSize = reader.ReadInt32();

                    int initialTriPos = (int)reader.BaseStream.Position;

                    int brushID2 = reader.ReadInt32(); //wtf???

                    while (reader.BaseStream.Position - initialTriPos < trisSize)
                    {
                        int ind = reader.ReadInt32();
                        mesh.Vertices.Add(new MeshVertex(vertices[ind].Location, vertices[ind].Normal, vertices[ind].BoneWeightings, vertices[ind].TextureU, vertices[ind].TextureV));
                    }
                }
                model.AddMesh("mesh", 0, mesh);
            }
            else
            {
                reader.ReadBytes(size);
            }

            return header;
        }

        protected override DataStructures.Models.Model LoadFromFile(IFile file)
        {
            DataStructures.Models.Model model = new DataStructures.Models.Model();
            Bone bone = new Bone(0, -1, null, "rootBone", CoordinateF.Zero, CoordinateF.Zero, CoordinateF.One, CoordinateF.One);
            model.Bones.Add(bone);

            FileStream stream = new FileStream(file.FullPathName, FileMode.Open);
            BinaryReader reader = new BinaryReader(stream);

            string header = reader.ReadFixedLengthString(Encoding.ASCII, 4);
            if (header != "BB3D")
            {
                reader.Dispose();
                stream.Dispose();
                return null;
            }

            int fileLength = reader.ReadInt32();

            int version = reader.ReadInt32();

            for (int i=0;i<3;i++)
            {
                if (ReadChunk(reader, model) == "NODE") break;
            }

            reader.Dispose();
            stream.Dispose();

            return model;
        }
    }
}
