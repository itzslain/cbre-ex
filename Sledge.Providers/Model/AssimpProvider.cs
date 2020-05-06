using Sledge.DataStructures.Models;
using Sledge.DataStructures.Geometric;
using Sledge.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using Assimp;
using Assimp.Configs;

namespace Sledge.Providers.Model
{
    public class AssimpProvider : ModelProvider
    {
        protected static AssimpContext importer = null;

        protected override bool IsValidForFile(IFile file)
        {
            return file.Extension.ToLowerInvariant() == "b3d" ||
                   file.Extension.ToLowerInvariant() == "fbx" ||
                   file.Extension.ToLowerInvariant() == "x";
        }

        protected static DataStructures.Models.Mesh AddMesh(DataStructures.Models.Model sledgeModel, Assimp.Mesh assimpMesh)
        {
            var sledgeMesh = new DataStructures.Models.Mesh(0);
            List<MeshVertex> vertices = new List<MeshVertex>();

            for (int i=0;i<assimpMesh.VertexCount;i++)
            {
                var assimpVertex = assimpMesh.Vertices[i];
                var assimpNormal = assimpMesh.Normals[i];
                var assimpUv = assimpMesh.TextureCoordinateChannels[0][i];

                vertices.Add(new MeshVertex(new CoordinateF(assimpVertex.X, -assimpVertex.Z, assimpVertex.Y),
                                            new CoordinateF(assimpNormal.X, -assimpNormal.Z, assimpNormal.Y),
                                            sledgeModel.Bones[0], assimpUv.X, -assimpUv.Y));
            }

            foreach (var face in assimpMesh.Faces)
            {
                var triInds = face.Indices;
                sledgeMesh.Vertices.Add(new MeshVertex(vertices[triInds[0]].Location, vertices[triInds[0]].Normal, vertices[triInds[0]].BoneWeightings, vertices[triInds[0]].TextureU, vertices[triInds[0]].TextureV));
                sledgeMesh.Vertices.Add(new MeshVertex(vertices[triInds[2]].Location, vertices[triInds[2]].Normal, vertices[triInds[2]].BoneWeightings, vertices[triInds[2]].TextureU, vertices[triInds[2]].TextureV));
                sledgeMesh.Vertices.Add(new MeshVertex(vertices[triInds[1]].Location, vertices[triInds[1]].Normal, vertices[triInds[1]].BoneWeightings, vertices[triInds[1]].TextureU, vertices[triInds[1]].TextureV));
            }

            return sledgeMesh;
        }

        protected override DataStructures.Models.Model LoadFromFile(IFile file)
        {
            if (importer == null)
            {
                importer = new AssimpContext();
                //importer.SetConfig(new NormalSmoothingAngleConfig(66.0f));
            }

            DataStructures.Models.Model model = new DataStructures.Models.Model();
            DataStructures.Models.Bone bone = new DataStructures.Models.Bone(0, -1, null, "rootBone", CoordinateF.Zero, CoordinateF.Zero, CoordinateF.One, CoordinateF.One);
            model.Bones.Add(bone);

            Scene scene = importer.ImportFile(file.FullPathName);

            DataStructures.Models.Texture tex = null;

            if (scene.MaterialCount > 0)
            {
                //TODO: handle several textures
                string path = Path.Combine(Path.GetDirectoryName(file.FullPathName), scene.Materials[0].TextureDiffuse.FilePath);
                if (!File.Exists(path)) { path = scene.Materials[0].TextureDiffuse.FilePath; }
                if (File.Exists(path))
                {
                    Bitmap bmp = new Bitmap(path);
                    tex = new DataStructures.Models.Texture
                    {
                        Name = path,
                        Index = 0,
                        Width = bmp.Width,
                        Height = bmp.Height,
                        Flags = 0,
                        Image = bmp
                    };
                }
            }

            if (tex == null)
            {
                Bitmap bmp = new Bitmap(64, 64);
                for (int i = 0; i < 64; i++)
                {
                    for (int j = 0; j < 64; j++)
                    {
                        bmp.SetPixel(i, j, Color.DarkGray);
                    }
                }
                tex = new DataStructures.Models.Texture
                {
                    Name = "blank",
                    Index = 0,
                    Width = 64,
                    Height = 64,
                    Flags = 0,
                    Image = bmp
                };
            }

            model.Textures.Add(tex);

            foreach (var mesh in scene.Meshes)
            {
                DataStructures.Models.Mesh sledgeMesh = AddMesh(model, mesh);
                foreach (var v in sledgeMesh.Vertices)
                {
                    v.TextureU *= tex.Width;
                    v.TextureV *= tex.Height;
                }
                model.AddMesh("mesh", 0, sledgeMesh);
            }

            return model;
        }
    }
}
