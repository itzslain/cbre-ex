using System.Runtime.Serialization;
using CBRE.DataStructures.Geometric;

namespace CBRE.DataStructures.Transformations {
    public interface IUnitTransformation : ISerializable {
        Coordinate Transform(Coordinate c);
        CoordinateF Transform(CoordinateF c);
    }
}
