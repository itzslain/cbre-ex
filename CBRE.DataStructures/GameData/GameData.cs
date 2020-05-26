using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            var lightDataObj = new GameDataObject("light", "Point light source.", ClassType.Point);
            lightDataObj.Properties.Add(new Property("color", VariableType.Color255) { DefaultValue = "255 255 255" });
            lightDataObj.Properties.Add(new Property("intensity", VariableType.Float) { DefaultValue = "1.0" });
            lightDataObj.Properties.Add(new Property("range", VariableType.Float) { DefaultValue = "1.0" });
            lightDataObj.Behaviours.Add(new Behaviour("sprite", "sprites/lightbulb.spr"));
            Classes.Add(lightDataObj);
            var spotlightDataObj = new GameDataObject("spotlight", "Self-explanatory.", ClassType.Point);
            spotlightDataObj.Properties.Add(new Property("color", VariableType.Color255) { DefaultValue = "255 255 255" });
            spotlightDataObj.Properties.Add(new Property("intensity", VariableType.Float) { DefaultValue = "1.0" });
            spotlightDataObj.Properties.Add(new Property("range", VariableType.Float) { DefaultValue = "1.0" });
            spotlightDataObj.Properties.Add(new Property("innerconeangle", VariableType.Float) { DefaultValue = "45" });
            spotlightDataObj.Properties.Add(new Property("outerconeangle", VariableType.Float) { DefaultValue = "90" });
            spotlightDataObj.Properties.Add(new Property("angles", VariableType.Vector) { DefaultValue = "0 0 0" });
            Classes.Add(spotlightDataObj);
            Classes.Add(new GameDataObject("waypoint", "AI waypoint.", ClassType.Point));
            Classes.Add(new GameDataObject("soundemitter", "Self-explanatory.", ClassType.Point));
            var modelDataObj = new GameDataObject("model", "Self-explanatory.", ClassType.Point);
            modelDataObj.Properties.Add(new Property("file", VariableType.Other) { DefaultValue = "" });
            modelDataObj.Properties.Add(new Property("angles", VariableType.Vector) { DefaultValue = "0 0 0" });
            modelDataObj.Properties.Add(new Property("scale", VariableType.Vector) { DefaultValue = "1 1 1" });
            Classes.Add(modelDataObj);

            var noShadowObj = new GameDataObject("noshadow", "Disables shadow casting for this brush.", ClassType.Solid);
            Classes.Add(noShadowObj);

            Includes = new List<string>();
            MaterialExclusions = new List<string>();
            AutoVisgroups = new List<AutoVisgroupSection>();
        }

        public void CreateDependencies()
        {
            var resolved = new List<string>();
            var unresolved = new List<GameDataObject>(Classes);
            while (unresolved.Any())
            {
                var resolve = unresolved.Where(x => x.BaseClasses.All(resolved.Contains)).ToList();
                if (!resolve.Any()) throw new Exception("Circular dependencies: " + String.Join(", ", unresolved.Select(x => x.Name)));
                resolve.ForEach(x => x.Inherit(Classes.Where(y => x.BaseClasses.Contains(y.Name))));
                unresolved.RemoveAll(resolve.Contains);
                resolved.AddRange(resolve.Select(x => x.Name));
            }
        }

        public void RemoveDuplicates()
        {
            foreach (var g in Classes.Where(x => x.ClassType != ClassType.Base).GroupBy(x => x.Name.ToLowerInvariant()).Where(g => g.Count() > 1).ToList())
            {
                foreach (var obj in g.Skip(1)) Classes.Remove(obj);
            }
        }
    }
}
