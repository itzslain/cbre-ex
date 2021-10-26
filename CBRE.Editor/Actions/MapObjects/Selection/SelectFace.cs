using System.Collections.Generic;
using CBRE.DataStructures.MapObjects;

namespace CBRE.Editor.Actions.MapObjects.Selection {
    public class SelectFace : ChangeFaceSelection {
        public SelectFace(IEnumerable<Face> objects) : base(objects, new Face[0]) {
        }

        public SelectFace(params Face[] objects) : base(objects, new Face[0]) {
        }
    }
}