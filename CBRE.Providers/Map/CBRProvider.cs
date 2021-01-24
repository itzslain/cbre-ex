using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using CBRE.DataStructures.GameData;
using CBRE.DataStructures.Geometric;
using CBRE.DataStructures.MapObjects;

namespace CBRE.Providers.Map {
    public class CBRProvider : MapProvider {
        private const uint revision = 0;

        protected override IEnumerable<MapFeature> GetFormatFeatures() {
            return new[] {
                MapFeature.Solids,
                MapFeature.Entities,
                MapFeature.Displacements, // TODO
                MapFeature.Groups,
                MapFeature.SingleVisgroups,
                MapFeature.MultipleVisgroups,
            };
        }

        protected override bool IsValidForFileName(string filename) {
            return filename.EndsWith("cbr", StringComparison.OrdinalIgnoreCase);
        }

        protected override DataStructures.MapObjects.Map GetFromStream(Stream stream, IEnumerable<string> textureDirs, IEnumerable<string> modelDirs) {
            BinaryReader reader = new BinaryReader(stream);

            if (reader.ReadFixedLengthString(Encoding.ASCII, 3) != "CBR") {
                throw new ProviderException("CBR file is corrupted/invalid!");
            }
            uint revision = reader.ReadUInt32();

            // Texture dictionary
            int texSize = reader.ReadInt32();
            string[] textures = new string[texSize];
            for (int i = 0; i < texSize; i++) {
                textures[i] = reader.ReadNullTerminatedString();
            }

            DataStructures.MapObjects.Map map = new DataStructures.MapObjects.Map();
            map.WorldSpawn = new World(map.IDGenerator.GetNextObjectID());

            // Solids
            int solidCount = reader.ReadInt32();
            for (int i = 0; i < solidCount; i++) {
                Solid s = new Solid(map.IDGenerator.GetNextObjectID());
                int faceCount = reader.ReadInt32();
                for (int j = 0; j < faceCount; j++) {
                    Face f = new Face(map.IDGenerator.GetNextFaceID());
                    f.Texture.Name = textures[reader.ReadInt32()];
                    f.Texture.UAxis = reader.ReadCoordinate();
                    f.Texture.VAxis = reader.ReadCoordinate();
                    f.Texture.XShift = reader.ReadDecimal();
                    f.Texture.YShift = reader.ReadDecimal();
                    f.Texture.XScale = reader.ReadDecimal();
                    f.Texture.YScale = reader.ReadDecimal();
                    f.Texture.Rotation = reader.ReadDecimal();
                    int vertexCount = reader.ReadInt32();
                    for (int k = 0; k < vertexCount; k++) {
                        Vertex v = new Vertex(reader.ReadCoordinate(), f);
                        f.Vertices.Add(v);
                    }
                    f.Plane = new Plane(f.Vertices[0].Location, f.Vertices[1].Location, f.Vertices[2].Location);
                    f.Parent = s;
                    s.Faces.Add(f);
                    f.UpdateBoundingBox();
                }
                s.SetParent(map.WorldSpawn, false);
                s.UpdateBoundingBox(false);
            }

            // Entity dictionary
            List<Tuple<string, List<Tuple<string, int>>>> entityTypes = new List<Tuple<string, List<Tuple<string, int>>>>();
            string read;
            while ((read = reader.ReadNullTerminatedString()) != "") {
                List<Tuple<string, int>> properties = new List<Tuple<string, int>>();
                byte propertyType;
                while ((propertyType = reader.ReadByte()) != 255) {
                    properties.Add(new Tuple<string, int>(reader.ReadNullTerminatedString(), propertyType));
                }
                entityTypes.Add(new Tuple<string, List<Tuple<string, int>>>(read, properties));
            }

            // Entities
            foreach (Tuple<string, List<Tuple<string, int>>> entityType in entityTypes) {
                int entitiesOfType = reader.ReadInt32();
                for (int i = 0; i < entitiesOfType; i++) {
                    Entity e = new Entity(map.IDGenerator.GetNextObjectID());
                    Console.WriteLine(entityType.Item1);
                    e.ClassName = entityType.Item1;
                    e.EntityData.Name = entityType.Item1;
                    foreach (Tuple<string, int> property in entityType.Item2) {
                        string propertyVal;
                        switch ((VariableType)property.Item2) {
                            case VariableType.Bool:
                                propertyVal = reader.ReadBoolean() ? "Yes" : "No";
                                break;
                            case VariableType.Color255:
                                propertyVal = DataStructures.MapObjects.Property.FromColor(reader.ReadRGBAColour());
                                break;
                            case VariableType.Float:
                                propertyVal = reader.ReadSingle().ToString();
                                break;
                            case VariableType.Integer:
                                propertyVal = reader.ReadInt32().ToString();
                                break;
                            case VariableType.String:
                                propertyVal = reader.ReadNullTerminatedString();
                                break;
                            case VariableType.Vector:
                                propertyVal = DataStructures.MapObjects.Property.FromCoordinate(reader.ReadCoordinate());
                                break;
                            case VariableType.Choices:
                                // TODO: Bullshit
                                throw new NotImplementedException();
                            default:
                                propertyVal = "";
                                break;
                        }
                        e.EntityData.SetPropertyValue(property.Item1, propertyVal);
                    }
                    e.SetParent(map.WorldSpawn);
                    e.UpdateBoundingBox();
                }
            }

            stream.Close();
            return map;
        }

        protected override void SaveToStream(Stream stream, DataStructures.MapObjects.Map map, DataStructures.GameData.GameData gameData) {
            BinaryWriter writer = new BinaryWriter(stream);

            writer.WriteFixedLengthString(Encoding.ASCII, 3, "CBR");
            writer.Write(revision);

            // Texture dictionary
            Dictionary<string, int> texDic = new Dictionary<string, int>();
            StringBuilder texBuilder = new StringBuilder();
            int texCount = 0;
            IEnumerator<string> textures = map.GetAllTextures().GetEnumerator();
            while (textures.MoveNext()) {
                texBuilder.Append(textures.Current);
                texBuilder.Append('\0');
                texDic.Add(textures.Current, texCount);
                texCount++;
            }
            writer.Write(texCount);
            writer.WriteFixedLengthString(Encoding.UTF8, texBuilder.Length, texBuilder.ToString());

            // Solids
            List<MapObject> solids = map.WorldSpawn.Find(x => x is Solid);
            writer.Write(solids.Count);
            foreach (Solid s in solids) {
                writer.Write(s.Faces.Count);
                foreach (Face f in s.Faces) {
                    writer.Write(texDic[f.Texture.Name]);
                    writer.WriteCoordinate(f.Texture.UAxis);
                    writer.WriteCoordinate(f.Texture.VAxis);
                    writer.Write(f.Texture.XShift);
                    writer.Write(f.Texture.YShift);
                    writer.Write(f.Texture.XScale);
                    writer.Write(f.Texture.YScale);
                    writer.Write(f.Texture.Rotation);
                    writer.Write(f.Vertices.Count);
                    foreach (Vertex v in f.Vertices) {
                        writer.WriteCoordinate(v.Location);
                    }
                }
            }

            // Entity dictionary
            int entityTypeCount = 0;
            Dictionary<string, Tuple<int, GameDataObject>> entityTypes = new Dictionary<string, Tuple<int, GameDataObject>>();
            List<MapObject> entites = map.WorldSpawn.Find(x => x is Entity && x.ClassName != "");
            foreach (Entity e in entites) {
                if (!entityTypes.ContainsKey(e.ClassName)) {
                    GameDataObject gdo = gameData.Classes.Find(x => x.Name == e.ClassName);
                    writer.WriteNullTerminatedString(e.ClassName);
                    foreach (DataStructures.GameData.Property p in gdo.Properties) {
                        writer.Write((byte)p.VariableType);
                        writer.WriteNullTerminatedString(p.Name);
                    }
                    writer.Write((byte)255); // Property end byte
                    entityTypes.Add(e.ClassName, new Tuple<int, GameDataObject>(entityTypeCount, gdo));
                    entityTypeCount++;
                }
            }
            writer.Write((byte)0); // Entity dictionary end byte

            // Entities
            foreach (KeyValuePair<string, Tuple<int, GameDataObject>> entityType in entityTypes) {
                List<MapObject> entitiesOfType = entites.FindAll(x => x.ClassName == entityType.Key);
                writer.Write(entitiesOfType.Count);
                foreach (Entity e in entitiesOfType) {
                    for (int i = 0; i < entityType.Value.Item2.Properties.Count; i++) {
                        DataStructures.MapObjects.Property property;
                        if (i < e.EntityData.Properties.Count && entityType.Value.Item2.Properties[i].Name == e.EntityData.Properties[i].Key) {
                            property = e.EntityData.Properties[i];
                        } else {
                            property = e.EntityData.Properties.Find(x => x.Key == entityType.Value.Item2.Properties[i].Name);
                            if (property == null) {
                                property = new DataStructures.MapObjects.Property();
                                property.Key = entityType.Value.Item2.Properties[i].Name;
                                property.Value = entityType.Value.Item2.Properties[i].DefaultValue;
                            }
                        }
                        switch (entityType.Value.Item2.Properties[i].VariableType) {
                            case VariableType.Bool:
                                writer.Write(property.Value == "Yes");
                                break;
                            case VariableType.Color255:
                                writer.WriteRGBAColour(property.GetColour(Color.Black));
                                break;
                            case VariableType.Float:
                                writer.Write(float.Parse(property.Value));
                                break;
                            case VariableType.Integer:
                                writer.Write(int.Parse(property.Value));
                                break;
                            case VariableType.String:
                                writer.WriteNullTerminatedString(property.Value);
                                break;
                            case VariableType.Vector:
                                writer.WriteCoordinate(property.GetCoordinate(Coordinate.Zero));
                                break;
                            case VariableType.Choices:
                                bool found = false;
                                for (int j = 0; j < entityType.Value.Item2.Properties[i].Options.Count; j++) {
                                    if (property.Value == entityType.Value.Item2.Properties[i].Options[j].Key) {
                                        writer.Write((byte)j);
                                        found = true;
                                        break;
                                    }
                                }
                                if (!found) {
                                    writer.Write((byte)255);
                                }
                                break;
                        }
                    }
                }
            }

            stream.Close();
        }
    }
}
