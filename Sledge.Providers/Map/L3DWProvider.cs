using System.Collections.Generic;
using System.Linq;
using System.IO;
using Sledge.DataStructures.MapObjects;
using System;
using System.Globalization;
using Sledge.DataStructures.Geometric;
using Sledge.Common;
using Sledge.DataStructures.Transformations;

namespace Sledge.Providers.Map
{
    public class L3DWProvider : MapProvider
    {
        private static readonly List<MapProvider> RegisteredProviders;
        
        protected override DataStructures.MapObjects.Map GetFromFile(string filename)
        {
            using (var strm = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                return GetFromStream(strm);
            }
        }

        protected override void SaveToFile(string filename, DataStructures.MapObjects.Map map)
        {
            using (var strm = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                SaveToStream(strm, map);
            }
        }

        protected override bool IsValidForFileName(string filename)
        {
            return filename.EndsWith(".3dw", true, CultureInfo.InvariantCulture);
        }

        protected override DataStructures.MapObjects.Map GetFromStream(Stream stream)
        {
            var map = new DataStructures.MapObjects.Map();
            BinaryReader br = new BinaryReader(stream);

            //header
            UInt16 mapVersion = br.ReadUInt16();
            byte mapFlags = br.ReadByte();
            Int32 nameCount = br.ReadInt32();
            Int32 nameOffset = br.ReadInt32();
            Int32 objectCount = br.ReadInt32();
            Int32 objectOffset = br.ReadInt32();

            //get names, needed to understand the objects
            List<string> names = new List<string>();
            br.BaseStream.Seek(nameOffset, SeekOrigin.Begin);
            for (int i=0;i<nameCount;i++)
            {
                string name = br.ReadNullTerminatedString();
                names.Add(name);
            }

            //now we can parse the object table
            List<string> materials = new List<string>();
            List<Tuple<int,string>> meshReferences = new List<Tuple<int, string>>();
            br.BaseStream.Seek(objectOffset, SeekOrigin.Begin);
            long objectStartPos = br.BaseStream.Position;
            for (int i = 0; i < objectCount; i++)
            {
                int index = br.ReadInt32() - 1;
                int size = br.ReadInt32();
                if (index < 0 || index >= names.Count)
                {
                    throw new Exception(i.ToString() + " " + index.ToString());
                }
                string name = names[index];

                if (name == "meshreference")
                {
                    byte flags = br.ReadByte();

                    Int32 groupNameInd = br.ReadInt32()-1;
                    Int32 objectNameInd = br.ReadInt32()-1;

                    byte limbCount = br.ReadByte();

                    meshReferences.Add(new Tuple<int, string>(i,names[objectNameInd]));
                }
                else if (name == "material")
                {
                    byte materialFlags = br.ReadByte();
                    Int32 groupIndex = br.ReadInt32();
                    string objectName = names[br.ReadInt32() - 1];
                    Int32 extensionNameIndex = -1;
                    if ((materialFlags & 2) != 0)
                    {
                        extensionNameIndex = br.ReadInt32(); //TODO: what the heck is this
                    }
                    materials.Add(objectName);
                }
                else
                {
                    br.BaseStream.Seek(size, SeekOrigin.Current);
                }
            }
            br.BaseStream.Position = objectStartPos;
            for (int i = 0; i < objectCount; i++)
            {
                int index = br.ReadInt32() - 1;
                int size = br.ReadInt32();
                if (index < 0 || index >= names.Count)
                {
                    throw new Exception(i.ToString() + " " + index.ToString());
                }
                string name = names[index];
                if (name == "mesh")
                {
                    long startPos = br.BaseStream.Position;

                    byte flags = br.ReadByte();

                    Entity entity = new Entity(map.IDGenerator.GetNextObjectID());
                    entity.ClassName = "model";
                    entity.Colour = Colour.GetDefaultEntityColour();
                    
                    Int32 keyCount = br.ReadInt32();
                    for (int j = 0; j < keyCount; j++)
                    {
                        Int32 keyNameInd = br.ReadInt32() - 1;
                        Int32 keyValueInd = br.ReadInt32() - 1;
                        if (names[keyNameInd] == "classname")
                        {
                            //entity.ClassName = names[keyValueInd];
                            //entity.EntityData.Name = names[keyValueInd];
                        }
                        else
                        {
                            Property newProperty = new Property();
                            newProperty.Key = names[keyNameInd];
                            newProperty.Value = names[keyValueInd];

                            if (newProperty.Key == "file")
                            {
                                newProperty.Value = System.IO.Path.GetFileNameWithoutExtension(newProperty.Value);
                            }

                            entity.EntityData.Properties.Add(newProperty);
                        }
                    }
                    Int32 group = br.ReadInt32();
                    Int32 visgroup = br.ReadInt32();

                    byte red = br.ReadByte(); byte green = br.ReadByte(); byte blue = br.ReadByte();

                    Int32 meshRefIndex = br.ReadInt32()-1;
                    
                    float x = br.ReadSingle();
                    float z = br.ReadSingle();
                    float y = br.ReadSingle();
                    if (entity!=null) entity.Origin = new Coordinate((decimal)x, (decimal)y, (decimal)z);
                    
                    if (entity.EntityData.GetPropertyValue("file") == null)
                    {
                        Property newProperty = new Property();
                        newProperty.Key = "file";
                        newProperty.Value = meshReferences.Find(q => q.Item1 == meshRefIndex).Item2;

                        entity.EntityData.Properties.Add(newProperty);
                    }

                    //TODO: figure out rotation, wtf
                    float pitch = br.ReadSingle();
                    float yaw = br.ReadSingle();
                    float roll = br.ReadSingle();

                    //TODO: figure out scale as well wtf
                    if ((flags&1)==0)
                    {
                        float xScale = br.ReadSingle();
                        float yScale = br.ReadSingle();
                        float zScale = br.ReadSingle();
                    }

                    br.BaseStream.Position += size - (br.BaseStream.Position-startPos);

                    if (entity != null)
                    {
                        entity.UpdateBoundingBox();
                        entity.SetParent(map.WorldSpawn);
                    }
                }
                else if (name == "entity")
                {
                    byte flags = br.ReadByte();
                    float x = br.ReadSingle();
                    float z = br.ReadSingle();
                    float y = br.ReadSingle();

                    Entity entity = new Entity(map.IDGenerator.GetNextObjectID());
                    entity.Colour = Colour.GetDefaultEntityColour();
                    entity.Origin = new Coordinate((decimal)x, (decimal)y, (decimal)z);

                    Int32 keyCount = br.ReadInt32();
                    for (int j=0;j<keyCount;j++)
                    {
                        Int32 keyNameInd = br.ReadInt32()-1;
                        Int32 keyValueInd = br.ReadInt32()-1;
                        if (names[keyNameInd] == "classname")
                        {
                            entity.ClassName = names[keyValueInd];
                            entity.EntityData.Name = names[keyValueInd];
                        }
                        else
                        {
                            Property newProperty = new Property();
                            newProperty.Key = names[keyNameInd];
                            newProperty.Value = names[keyValueInd];
                            entity.EntityData.Properties.Add(newProperty);
                        }
                    }
                    Int32 group = br.ReadInt32();
                    Int32 visgroup = br.ReadInt32();

                    entity.UpdateBoundingBox();
                    entity.SetParent(map.WorldSpawn);
                }
                else if (name == "brush")
                {
                    byte brushFlags = br.ReadByte(); //TODO: ???
                    Int32 keys = br.ReadInt32();
                    for (int j=0;j<keys;j++)
                    {
                        Int32 keyNameInd = br.ReadInt32();
                        Int32 keyValueInd = br.ReadInt32();
                    }
                    Int32 groupIndex = br.ReadInt32();
                    Int32 visgroupIndex = br.ReadInt32();

                    byte red = br.ReadByte(); byte green = br.ReadByte(); byte blue = br.ReadByte();

                    List<Coordinate> vertices = new List<Coordinate>();
                    byte vertexCount = br.ReadByte();
                    for (int j=0;j<vertexCount;j++)
                    {
                        decimal x = (decimal)br.ReadSingle(); decimal z = (decimal)br.ReadSingle(); decimal y = (decimal)br.ReadSingle();
                        vertices.Add(new Coordinate(x, y, z));
                    }
                    List<Face> faces = new List<Face>();
                    byte faceCount = br.ReadByte();
                    for (int j=0;j<faceCount;j++)
                    {
                        byte faceFlags = br.ReadByte();

                        //TODO: maybe we need these unused bits for something idk
                        decimal planeEq0 = (decimal)br.ReadSingle(); decimal planeEq1 = (decimal)br.ReadSingle(); decimal planeEq2 = (decimal)br.ReadSingle(); decimal planeEq3 = (decimal)br.ReadSingle();
                        
                        decimal texPosX = (decimal)br.ReadSingle(); decimal texPosY = (decimal)br.ReadSingle();
                        decimal texScaleX = (decimal)br.ReadSingle(); decimal texScaleY = (decimal)br.ReadSingle();
                        float texRotX = br.ReadSingle(); float texRotY = br.ReadSingle();

                        decimal uTexPlane0 = (decimal)br.ReadSingle(); decimal uTexPlane1 = (decimal)br.ReadSingle(); decimal uTexPlane2 = (decimal)br.ReadSingle(); decimal uTexPlane3 = (decimal)br.ReadSingle();
                        decimal vTexPlane0 = (decimal)br.ReadSingle(); decimal vTexPlane1 = (decimal)br.ReadSingle(); decimal vTexPlane2 = (decimal)br.ReadSingle(); decimal vTexPlane3 = (decimal)br.ReadSingle();

                        float luxelSize = br.ReadSingle();

                        Int32 smoothGroupInd = br.ReadInt32();
                        Int32 materialInd = br.ReadInt32()-1;

                        Int32 lightmapInd = -1;
                        if ((faceFlags & 16) != 0)
                        {
                            lightmapInd = br.ReadInt32();
                        }
                        
                        byte indexCount = br.ReadByte();
                        List<byte> vertsInFace = new List<byte>();
                        for (int k=0;k<indexCount;k++)
                        {
                            byte vertIndex = br.ReadByte();
                            vertsInFace.Add(vertIndex);
                            
                            float texCoordX = br.ReadSingle(); float texCoordY = br.ReadSingle();
                            
                            float lmCoordX = 0.0f; float lmCoordY = 0.0f;
                            if ((faceFlags & 16) != 0)
                            {
                                lmCoordX = br.ReadSingle(); lmCoordY = br.ReadSingle();
                            }
                        }

                        Coordinate norm = new Coordinate(planeEq0, planeEq2, planeEq1);
                        
                        if (Math.Abs((float)norm.LengthSquared())>0.001f)
                        {
                            if (Math.Abs((double)norm.LengthSquared() - 1) > 0.001) throw new Exception(norm.LengthSquared().ToString());

                            Face newFace = new Face(map.IDGenerator.GetNextFaceID());
                            
                            foreach (byte vertInd in vertsInFace)
                            {
                                newFace.Vertices.Insert(0,new Vertex(vertices[vertInd], newFace));
                            }

                            newFace.Plane = new Plane(newFace.Vertices[0].Location, newFace.Vertices[1].Location, newFace.Vertices[2].Location);

                            newFace.UpdateBoundingBox();

                            Coordinate uNorm = new Coordinate(uTexPlane0, uTexPlane2, uTexPlane1);
                            Coordinate vNorm = new Coordinate(vTexPlane0, vTexPlane2, vTexPlane1);
                            if (Math.Abs((double)(uNorm.LengthSquared() - vNorm.LengthSquared()))>0.001) throw new Exception(uNorm.LengthSquared().ToString()+" "+vNorm.LengthSquared().ToString());

                            newFace.Texture.Name = (faceFlags & 4) != 0 ? "tooltextures/remove_face" : materials[materialInd];
                            newFace.AlignTextureToWorld();
                            if (texRotY != texRotX) throw new Exception((texRotX - texRotY).ToString());
                            
                            newFace.Texture.UAxis = uNorm * (decimal)Math.Cos(-texRotY * Math.PI / 180.0) + vNorm * (decimal)Math.Sin(-texRotY * Math.PI / 180.0);
                            newFace.Texture.VAxis = vNorm * (decimal)Math.Cos(-texRotY * Math.PI / 180.0) - uNorm * (decimal)Math.Sin(-texRotY * Math.PI / 180.0);
                            newFace.Texture.XScale = texScaleX/2;
                            newFace.Texture.YScale = texScaleY/2;
                            newFace.Texture.XShift = -texPosX*2/texScaleX;
                            newFace.Texture.YShift = texPosY*2/texScaleY;
                            newFace.Texture.Rotation = (decimal)texRotY;
                            
                            newFace.Transform(new UnitScale(Coordinate.One, newFace.BoundingBox.Center), TransformFlags.None);

                            faces.Add(newFace);
                        }
                    }

                    Solid newSolid = new Solid(map.IDGenerator.GetNextObjectID());
                    foreach (Face face in faces)
                    {
                        face.Parent = newSolid;
                        newSolid.Faces.Add(face);
                    }
                    newSolid.Colour = Colour.GetRandomBrushColour();
                    newSolid.SetParent(map.WorldSpawn);
                    newSolid.UpdateBoundingBox();

                    newSolid.Transform(new UnitScale(Coordinate.One, newSolid.BoundingBox.Center), TransformFlags.None);
                }
                else
                {
                    br.BaseStream.Seek(size, SeekOrigin.Current);
                }
            }
            
            return map;
        }

        protected override void SaveToStream(Stream stream, DataStructures.MapObjects.Map map)
        {
            throw new Exception("don't save to 3dw, ew");
        }

        protected override IEnumerable<MapFeature> GetFormatFeatures()
        {
            return new[]
            {
                MapFeature.Worldspawn,
                MapFeature.Solids,
                MapFeature.Entities
            };
        }
    }
}
