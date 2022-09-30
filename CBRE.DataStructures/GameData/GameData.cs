using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CBRE.DataStructures.GameData
{
    public class GameData
    {
        public int MapSizeLow { get; set; }
        public int MapSizeHigh { get; set; }
        public List<GameDataObject> Classes { get; private set; }
        public List<string> Includes { get; private set; }
        public List<string> MaterialExclusions { get; private set; }
        public List<AutoVisgroupSection> AutoVisgroups { get; private set; }

        public GameData()
        {
            MapSizeHigh = 16384;
            MapSizeLow = -16384;
            Classes = new List<GameDataObject>();

            IEnumerable<string> jsonFiles = Directory.EnumerateFiles("Entities", "*.json");
            foreach (string jsonFile in jsonFiles)
            {
                string jsonContent = File.ReadAllText(jsonFile);
                CustomEntity customEntity = JsonConvert.DeserializeObject<CustomEntity>(jsonContent);

                if (customEntity == null) continue;

                GameDataObject gameDataObj = new GameDataObject(customEntity.Name, customEntity.Description, ClassType.Point, true);

                foreach (CustomEntityProperty customProperty in customEntity.Properties)
                {
                    if (!Enum.TryParse(customProperty.Type, out VariableType varType)) continue;

                    Property actualProperty = new Property(customProperty.Name, varType)
                    {
                        ShortDescription = customProperty.SmartEditName,
                        DefaultValue = customProperty.DefaultValue,
                        Description = customProperty.HelpText
                    };

                    gameDataObj.Properties.Add(actualProperty);
                }

                if(!string.IsNullOrWhiteSpace(customEntity.Sprite)) gameDataObj.Behaviours.Add(new Behaviour("sprite", customEntity.Sprite));
                if(customEntity.UseModelRendering) gameDataObj.Behaviours.Add(new Behaviour("useModels"));

                Classes.Add(gameDataObj);
            }

            GameDataObject lightDataObj = new GameDataObject("light", "Point light source.", ClassType.Point);
            lightDataObj.Properties.Add(new Property("color", VariableType.Color255) { ShortDescription = "Color", DefaultValue = "255 255 255" });
            lightDataObj.Properties.Add(new Property("intensity", VariableType.Float) { ShortDescription = "Intensity", DefaultValue = "1.0" });
            lightDataObj.Properties.Add(new Property("range", VariableType.Float) { ShortDescription = "Range", DefaultValue = "1.0" });
            lightDataObj.Properties.Add(new Property("hassprite", VariableType.Bool) { ShortDescription = "Has sprite", DefaultValue = "Yes" });
            lightDataObj.Behaviours.Add(new Behaviour("sprite", "sprites/lightbulb"));
            Classes.Add(lightDataObj);

            GameDataObject spotlightDataObj = new GameDataObject("spotlight", "Self-explanatory.", ClassType.Point);
            spotlightDataObj.Properties.Add(new Property("color", VariableType.Color255) { ShortDescription = "Color", DefaultValue = "255 255 255" });
            spotlightDataObj.Properties.Add(new Property("intensity", VariableType.Float) { ShortDescription = "Intensity", DefaultValue = "1.0" });
            spotlightDataObj.Properties.Add(new Property("range", VariableType.Float) { ShortDescription = "Range", DefaultValue = "1.0" });
            spotlightDataObj.Properties.Add(new Property("hassprite", VariableType.Bool) { ShortDescription = "Has sprite", DefaultValue = "Yes" });
            spotlightDataObj.Properties.Add(new Property("innerconeangle", VariableType.Float) { ShortDescription = "Inner cone angle", DefaultValue = "45" });
            spotlightDataObj.Properties.Add(new Property("outerconeangle", VariableType.Float) { ShortDescription = "Outer cone angle", DefaultValue = "90" });
            spotlightDataObj.Properties.Add(new Property("angles", VariableType.Vector) { ShortDescription = "Rotation", DefaultValue = "0 0 0" });
            spotlightDataObj.Behaviours.Add(new Behaviour("sprite", "sprites/spotlight"));
            Classes.Add(spotlightDataObj);

            GameDataObject waypointDataObj = new GameDataObject("waypoint", "AI waypoint.", ClassType.Point);
            waypointDataObj.Behaviours.Add(new Behaviour("sprite", "sprites/waypoint"));
            Classes.Add(waypointDataObj);

            GameDataObject soundEmitterDataObj = new GameDataObject("soundemitter", "Self-explanatory.", ClassType.Point);
            soundEmitterDataObj.Properties.Add(new Property("sound", VariableType.Integer) { ShortDescription = "Ambience index", DefaultValue = "1" });
            soundEmitterDataObj.Behaviours.Add(new Behaviour("sprite", "sprites/speaker"));
            Classes.Add(soundEmitterDataObj);

            GameDataObject modelDataObj = new GameDataObject("model", "Self-explanatory.", ClassType.Point);
            modelDataObj.Properties.Add(new Property("file", VariableType.String) { ShortDescription = "File", DefaultValue = "" });
            modelDataObj.Properties.Add(new Property("angles", VariableType.Vector) { ShortDescription = "Rotation", DefaultValue = "0 0 0" });
            modelDataObj.Properties.Add(new Property("scale", VariableType.Vector) { ShortDescription = "Scale", DefaultValue = "1 1 1" });
            Classes.Add(modelDataObj);

            GameDataObject screenDataObj = new GameDataObject("screen", "Savescreen.", ClassType.Point);
            screenDataObj.Properties.Add(new Property("imgpath", VariableType.String) { ShortDescription = "Image Path", DefaultValue = "" });
            screenDataObj.Behaviours.Add(new Behaviour("sprite", "sprites/screen"));
            Classes.Add(screenDataObj);

            GameDataObject noShadowObj = new GameDataObject("noshadow", "Disables shadow casting for this brush.", ClassType.Solid);
            Classes.Add(noShadowObj);

            Property p = new Property("position", VariableType.Vector) { ShortDescription = "Position", DefaultValue = "0 0 0" };
            foreach (GameDataObject gdo in Classes)
            {
                if (gdo.ClassType != ClassType.Solid)
                {
                    gdo.Properties.Add(p);
                }
            }

            Includes = new List<string>();
            MaterialExclusions = new List<string>();
            AutoVisgroups = new List<AutoVisgroupSection>();
        }

        public void CreateDependencies()
        {
            List<string> resolved = new List<string>();
            List<GameDataObject> unresolved = new List<GameDataObject>(Classes);
            while (unresolved.Any())
            {
                List<GameDataObject> resolve = unresolved.Where(x => x.BaseClasses.All(resolved.Contains)).ToList();
                if (!resolve.Any()) throw new Exception("Circular dependencies: " + String.Join(", ", unresolved.Select(x => x.Name)));
                resolve.ForEach(x => x.Inherit(Classes.Where(y => x.BaseClasses.Contains(y.Name))));
                unresolved.RemoveAll(resolve.Contains);
                resolved.AddRange(resolve.Select(x => x.Name));
            }
        }

        public void RemoveDuplicates()
        {
            foreach (IGrouping<string, GameDataObject> g in Classes.Where(x => x.ClassType != ClassType.Base).GroupBy(x => x.Name.ToLowerInvariant()).Where(g => g.Count() > 1).ToList())
            {
                foreach (GameDataObject obj in g.Skip(1)) Classes.Remove(obj);
            }
        }
    }
}
