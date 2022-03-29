using System.Collections.Generic;

namespace CBRE.DataStructures.GameData
{
    public class JsonGameDataObj
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Sprite { get; set; }
        public List<JsonGDProperty> Properties { get; set; }
    }

    public class JsonGDProperty
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string ShortDescription { get; set; }
        public string DefaultValue { get; set; }
    }
}
