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

        // Hierarchy control bytes
        // bytes to avoid enum cast
        private const byte HIERARCHY_PROCCEED = 0;
        private const byte HIERARCHY_DOWN = 1;
        private const byte HIERARCHY_UP = 2;

        private struct EntityType {
            public string name;
            public List<Property> properties;
        }
        
        private struct Property {
            public string name;
            public VariableType type;
        }

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
            List<MapObject> solids = new List<MapObject>();
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
                solids.Add(s);
            }

            // Entity dictionary
            List<EntityType> entityTypes = new List<EntityType>();
            string read;
            while ((read = reader.ReadNullTerminatedString()) != "") {
                List<Property> properties = new List<Property>();
                byte propertyType;
                while ((propertyType = reader.ReadByte()) != 255) {
                    properties.Add(new Property() {
                        name = reader.ReadNullTerminatedString(),
                        type = (VariableType)propertyType
                    });
                }
                entityTypes.Add(new EntityType() {
                    name = read,
                    properties = properties
                });
            }

            // Entities
            List<MapObject> entities = new List<MapObject>(0);
            foreach (EntityType entityType in entityTypes) {
                int entitiesOfType = reader.ReadInt32();
                entities.Capacity += entitiesOfType;
                for (int i = 0; i < entitiesOfType; i++) {
                    Entity e = new Entity(map.IDGenerator.GetNextObjectID());
                    e.ClassName = entityType.name;
                    e.EntityData.Name = entityType.name;
                    foreach (Property property in entityType.properties) {
                        string propertyVal;
                        switch ((VariableType)property.type) {
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
                        e.EntityData.SetPropertyValue(property.name, propertyVal);
                    }
                    e.SetParent(map.WorldSpawn);
                    e.UpdateBoundingBox();
                    entities.Add(e);
                }
            }

            // Visgroup dictionary
            Visgroup currentParent = null;
            while (true) {
                byte hierarchyControl;
                Visgroup newGroup = null;
                while ((hierarchyControl = reader.ReadByte()) == HIERARCHY_PROCCEED) {
                    newGroup = new Visgroup();
                    newGroup.ID = reader.ReadInt32();
                    newGroup.Name = reader.ReadNullTerminatedString();
                    if (currentParent != null) {
                        newGroup.Parent = currentParent;
                        currentParent.Children.Add(newGroup);
                    } else {
                        map.Visgroups.Add(newGroup);
                    }
                }
                if (hierarchyControl == HIERARCHY_DOWN) {
                    currentParent = newGroup;
                } else if (currentParent != null) {
                    currentParent = currentParent.Parent;
                } else {
                    break;
                }
            }

            // Solid visgroups
            foreach (Solid mo in solids) {
                ReadVisgroups(reader, mo);
            }

            // Entity visgroups
            foreach (Entity e in entities) {
                ReadVisgroups(reader, e);
            }

            stream.Close();
            return map;
        }

        private void ReadVisgroups(BinaryReader reader, MapObject mo) {
            int visNum = reader.ReadInt32();
            for (int i = 0; i < visNum; i++) {
                mo.Visgroups.Add(reader.ReadInt32());
            }
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
            writer.WriteNullTerminatedString("");

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
                                // TODO: Implement dictionary.
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

            // Visgroup dictionary
            Stack<IEnumerator<Visgroup>> visStack = new Stack<IEnumerator<Visgroup>>();
            visStack.Push(map.Visgroups.GetEnumerator());
            while (visStack.Count > 0) {
                IEnumerator<Visgroup> v = visStack.Pop();
                while (v.MoveNext()) {
                    if (!v.Current.IsAutomatic) {
                        writer.Write(HIERARCHY_PROCCEED);
                        writer.Write(v.Current.ID);
                        writer.WriteNullTerminatedString(v.Current.Name);
                        if (v.Current.Children.Count != 0) {
                            writer.Write(HIERARCHY_DOWN);
                            visStack.Push(v);
                            v = v.Current.Children.GetEnumerator();
                        }
                    }
                }
                writer.Write(HIERARCHY_UP);
            }
            
            // Solid visgroups
            foreach (Solid s in map.WorldSpawn.FindAll().OfType<Solid>()) {
                WriteVisgroups(writer, s);
            }

            // Entity visgroups
            foreach (KeyValuePair<string, Tuple<int, GameDataObject>> entityType in entityTypes) {
                foreach (Entity e in entites.FindAll(x => x.ClassName == entityType.Key)) {
                    WriteVisgroups(writer, e);
                }
            }

            stream.Close();
        }

        private void WriteVisgroups(BinaryWriter writer, MapObject mo) {
            IEnumerable<int> visgroupIDs = mo.Visgroups.Except(mo.AutoVisgroups);
            writer.Write(visgroupIDs.Count());
            foreach (int v in visgroupIDs) {
                writer.Write(v);
            }
        }
    }
}
