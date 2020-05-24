using System.Collections.Generic;
using System.Linq;
using System.Text;
using CBRE.DataStructures.MapObjects;
using CBRE.Editor.Documents;

namespace CBRE.Editor.Actions.MapObjects.Operations.EditOperations
{
    public interface IEditOperation
    {
        void PerformOperation(MapObject mo);
    }
}
