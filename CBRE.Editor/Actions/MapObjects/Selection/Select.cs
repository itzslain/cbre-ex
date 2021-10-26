using System.Collections.Generic;
using CBRE.DataStructures.MapObjects;

namespace CBRE.Editor.Actions.MapObjects.Selection {
    public class Select : ChangeSelection {
        public Select(IEnumerable<MapObject> objects) : base(objects, new MapObject[0]) {
        }

        public Select(params MapObject[] objects) : base(objects, new MapObject[0]) {
        }
    }
}