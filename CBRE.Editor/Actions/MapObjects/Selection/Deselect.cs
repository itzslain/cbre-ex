using System.Collections.Generic;
using CBRE.DataStructures.MapObjects;

namespace CBRE.Editor.Actions.MapObjects.Selection {
    public class Deselect : ChangeSelection {
        public Deselect(IEnumerable<MapObject> objects) : base(new MapObject[0], objects) {
        }
    }
}