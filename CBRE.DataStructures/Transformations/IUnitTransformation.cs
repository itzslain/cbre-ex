using CBRE.DataStructures.Geometric;
using System.Runtime.Serialization;

namespace CBRE.DataStructures.Transformations {
    public interface IUnitTransformation : ISerializable {
        Coordinate Transform(Coordinate c);
        CoordinateF Transform(CoordinateF c);
    }
}
