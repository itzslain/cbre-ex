using CBRE.DataStructures.Models;
using CBRE.DataStructures.Geometric;
using CBRE.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using Assimp;
using Assimp.Configs;

namespace CBRE.Providers.Model
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

        protected static void AddNode(Scene scene, Node node, DataStructures.Models.Model model, DataStructures.Models.Texture tex, Matrix4x4 parentMatrix)
        {
            Matrix4x4 selfMatrix = node.Transform * parentMatrix;
            foreach (var meshIndex in node.MeshIndices)
            {
                DataStructures.Models.Mesh sledgeMesh = AddMesh(model, scene.Meshes[meshIndex], selfMatrix);
                foreach (var v in sledgeMesh.Vertices)
                {
                    v.TextureU *= tex.Width;
                    v.TextureV *= tex.Height;
                }
                model.AddMesh("mesh", 0, sledgeMesh);
            }

            foreach (var subNode in node.Children)
            {
                AddNode(scene, subNode, model, tex, selfMatrix);
            }
        }

        protected static DataStructures.Models.Mesh AddMesh(DataStructures.Models.Model sledgeModel, Assimp.Mesh assimpMesh, Matrix4x4 selfMatrix)
        {
            var sledgeMesh = new DataStructures.Models.Mesh(0);
            List<MeshVertex> vertices = new List<MeshVertex>();

            for (int i=0;i<assimpMesh.VertexCount;i++)
            {
                var assimpVertex = assimpMesh.Vertices[i];
                assimpVertex = selfMatrix * assimpVertex;
                var assimpNormal = assimpMesh.Normals[i];
                assimpNormal = selfMatrix * assimpNormal;
                var assimpUv = assimpMesh.TextureCoordinateChannels[0][i];

                vertices.Add(new MeshVertex(new CoordinateF(assimpVertex.X, -assimpVertex.Z, assimpVertex.Y),
                                            new CoordinateF(assimpNormal.X, -assimpNormal.Z, assimpNormal.Y),
                                            sledgeModel.Bones[0], assimpUv.X, -assimpUv.Y));
            }

            foreach (var face in assimpMesh.Faces)
            {
                var triInds = face.Indices;
                for (var i = 1; i < triInds.Count - 1; i++)
                {
                    sledgeMesh.Vertices.Add(new MeshVertex(vertices[triInds[0]].Location, vertices[triInds[0]].Normal, vertices[triInds[0]].BoneWeightings, vertices[triInds[0]].TextureU, vertices[triInds[0]].TextureV));
                    sledgeMesh.Vertices.Add(new MeshVertex(vertices[triInds[i + 1]].Location, vertices[triInds[i + 1]].Normal, vertices[triInds[2]].BoneWeightings, vertices[triInds[i + 1]].TextureU, vertices[triInds[i + 1]].TextureV));
                    sledgeMesh.Vertices.Add(new MeshVertex(vertices[triInds[i]].Location, vertices[triInds[i]].Normal, vertices[triInds[i]].BoneWeightings, vertices[triInds[i]].TextureU, vertices[triInds[i]].TextureV));
                }
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
                for (int i=0;i<scene.MaterialCount;i++)
                {
                    if (string.IsNullOrEmpty(scene.Materials[i].TextureDiffuse.FilePath)) { continue; }
                    string path = Path.Combine(Path.GetDirectoryName(file.FullPathName), scene.Materials[i].TextureDiffuse.FilePath);
                    if (!File.Exists(path)) { path = scene.Materials[i].TextureDiffuse.FilePath; }
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
                    break;
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

            AddNode(scene, scene.RootNode, model, tex, Matrix4x4.Identity);

            return model;
        }
    }
}
