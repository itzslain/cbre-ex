using CBRE.DataStructures.MapObjects;
using CBRE.DataStructures.Transformations;

namespace CBRE.Editor.Actions.MapObjects.Operations.EditOperations
{
    public class SnapToGridEditOperation : IEditOperation
    {
        private readonly decimal _gridSpacing;
        private readonly TransformFlags _transformFlags;

        public SnapToGridEditOperation(decimal gridSpacing, TransformFlags transformFlags)
        {
            _gridSpacing = gridSpacing;
            _transformFlags = transformFlags;
        }

        public void PerformOperation(MapObject mo)
        {
            DataStructures.Geometric.Box box = mo.BoundingBox;
            DataStructures.Geometric.Coordinate offset = box.Start.Snap(_gridSpacing) - box.Start;
            UnitTranslate transform = new UnitTranslate(offset);
            mo.Transform(transform, _transformFlags);
        }
    }
}