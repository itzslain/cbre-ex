using CBRE.Extensions;
using System;
using System.Runtime.Serialization;

namespace CBRE.DataStructures.Geometric
{
    /// <summary>
    /// Represents a quaternion. Shamelessly taken in its entirety from OpenTK's Quaternion structure. http://www.opentk.com/
    /// </summary>
    [Serializable]
    public class Quaternion : ISerializable
    {
        public static Quaternion Identity
        {
            get { return new Quaternion(0, 0, 0, 1); }
        }

        public Coordinate Vector { get; private set; }
        public decimal Scalar { get; private set; }

        public decimal X { get { return Vector.X; } }
        public decimal Y { get { return Vector.Y; } }
        public decimal Z { get { return Vector.Z; } }
        public decimal W { get { return Scalar; } }

        public Quaternion(Coordinate vector, decimal scalar)
        {
            Vector = vector;
            Scalar = scalar;
        }

        public Quaternion(decimal x, decimal y, decimal z, decimal w)
        {
            Vector = new Coordinate(x, y, z);
            Scalar = w;
        }

        protected Quaternion(SerializationInfo info, StreamingContext context)
        {
            Vector = (Coordinate)info.GetValue("Vector", typeof(Coordinate));
            Scalar = info.GetDecimal("Scalar");
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Vector", Vector);
            info.AddValue("Scalar", Scalar);
        }

        public decimal Dot(Quaternion c)
        {
            return Vector.Dot(c.Vector) + Scalar * c.Scalar;
        }

        public decimal Magnitude()
        {
            return DMath.Sqrt(DMath.Pow(X, 2) + DMath.Pow(Y, 2) + DMath.Pow(Z, 2) + DMath.Pow(W, 2));
        }

        public Quaternion Normalise()
        {
            decimal len = Magnitude();
            return len == 0 ? new Quaternion(0, 0, 0, 0) : new Quaternion(X / len, Y / len, Z / len, W / len);
        }

        public Quaternion Conjugate()
        {
            return new Quaternion(-X, -Y, -Z, W);
        }

        public Quaternion Inverse()
        {
            decimal lengthSq = DMath.Pow(X, 2) + DMath.Pow(Y, 2) + DMath.Pow(Z, 2) + DMath.Pow(W, 2);
            if (lengthSq != 0)
            {
                decimal i = 1 / lengthSq;
                return new Quaternion(Vector * -i, W * i);
            }
            return this;
        }

        public Quaternion Clone()
        {
            return new Quaternion(X, Y, Z, W);
        }

        public Tuple<Coordinate, decimal> GetAxisAngle()
        {
            Quaternion q = W > 1 ? Normalise() : this;
            decimal angle = 2 * DMath.Acos(q.W);
            decimal denom = DMath.Sqrt(1 - q.W * q.W);
            Coordinate coord = denom <= 0.0001m ? Coordinate.UnitX : q.Vector / denom;
            return Tuple.Create(coord, angle);
        }

        public Coordinate GetEulerAngles(bool homogenous = true)
        {
            // http://willperone.net/Code/quaternion.php
            decimal sqw = W * W;
            decimal sqx = X * X;
            decimal sqy = Y * Y;
            decimal sqz = Z * Z;

            return homogenous
                       ? new Coordinate(
                             DMath.Atan2(2 * (X * Y + Z * W), sqx - sqy - sqz + sqw),
                             DMath.Asin(-2 * (X * Z - Y * W)),
                             DMath.Atan2(2 * (Y * Z + X * W), -sqx - sqy + sqz + sqw))
                       : new Coordinate(
                             DMath.Atan2(2 * (Z * Y + X * W), 1 - 2 * (sqx + sqy)),
                             DMath.Asin(-2 * (X * Z - Y * W)),
                             DMath.Atan2(2 * (X * Y + Z * W), 1 - 2 * (sqy + sqz)));
        }

        public Matrix GetMatrix()
        {
            // http://local.wasp.uwa.edu.au/~pbourke/miscellaneous/quaternions/
            decimal xx = X * X;
            decimal yy = Y * Y;
            decimal zz = Z * Z;
            return new Matrix(
                           1 - 2 * yy - 2 * zz,
                           2 * X * Y + 2 * W * Z,
                           2 * X * Z - 2 * W * Y,
                           0,

                           2 * X * Y - 2 * W * Z,
                           1 - 2 * xx - 2 * zz,
                           2 * Y * Z + 2 * W * X,
                           0,

                           2 * X * Z + 2 * W * Y,
                           2 * Y * Z - 2 * W * X,
                           1 - 2 * xx - 2 * yy,
                           0,

                           0,
                           0,
                           0,
                           1
                       );
        }

        public Coordinate Rotate(Coordinate coord)
        {
            // http://content.gpwiki.org/index.php/OpenGL:Tutorials:Using_Quaternions_to_represent_rotation
            Quaternion q = new Quaternion(coord.Normalise(), 0);
            Quaternion temp = q * Conjugate();
            return (this * temp).Vector;
        }

        public bool Equals(Quaternion other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Vector, Vector) && other.Scalar == Scalar;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof(Quaternion)) return false;
            return Equals((Quaternion)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Vector != null ? Vector.GetHashCode() : 0) * 397) ^ Scalar.GetHashCode();
            }
        }

        public static bool operator ==(Quaternion left, Quaternion right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Quaternion left, Quaternion right)
        {
            return !Equals(left, right);
        }

        public static Quaternion operator +(Quaternion c1, Quaternion c2)
        {
            return new Quaternion(c1.X + c2.X, c1.Y + c2.Y, c1.Z + c2.Z, c1.W + c2.W);
        }

        public static Quaternion operator -(Quaternion c1, Quaternion c2)
        {
            return new Quaternion(c1.X - c2.X, c1.Y - c2.Y, c1.Z - c2.Z, c1.W - c2.W);
        }

        public static Quaternion operator -(Quaternion c1)
        {
            return new Quaternion(-c1.X, -c1.Y, -c1.Z, -c1.W);
        }

        public static Quaternion operator /(Quaternion c, decimal f)
        {
            return f == 0 ? new Quaternion(0, 0, 0, 0) : new Quaternion(c.X / f, c.Y / f, c.Z / f, c.W / f);
        }

        public static Quaternion operator *(Quaternion c, decimal f)
        {
            return new Quaternion(c.X * f, c.Y * f, c.Z * f, c.W * f);
        }

        public static Quaternion operator *(decimal f, Quaternion c)
        {
            return c * f;
        }

        public static Quaternion operator /(Quaternion left, Quaternion right)
        {
            return left * right.Inverse();
        }

        public static Quaternion operator *(Quaternion left, Quaternion right)
        {
            return new Quaternion(
                right.W * left.Vector + left.W * right.Vector + left.Vector.Cross(right.Vector),
                left.W * right.W - left.Vector.Dot(right.Vector));
        }

        public static Quaternion EulerAngles(decimal x, decimal y, decimal z)
        {
            return EulerAngles(new Coordinate(x, y, z));
        }

        public static Quaternion EulerAngles(Coordinate angles)
        {
            // http://www.euclideanspace.com/maths/geometry/rotations/conversions/eulerToQuaternion/index.htm
            angles = angles / 2;
            decimal sy = DMath.Sin(angles.Z);
            decimal sp = DMath.Sin(angles.Y);
            decimal sr = DMath.Sin(angles.X);
            decimal cy = DMath.Cos(angles.Z);
            decimal cp = DMath.Cos(angles.Y);
            decimal cr = DMath.Cos(angles.X);
            return new Quaternion(sr * cp * cy - cr * sp * sy,
                                  cr * sp * cy + sr * cp * sy,
                                  cr * cp * sy - sr * sp * cy,
                                  cr * cp * cy + sr * sp * sy);

        }

        public static Quaternion AxisAngle(Coordinate axis, decimal angle)
        {
            return axis.VectorMagnitude() == 0
                       ? Identity
                       : new Quaternion(axis.Normalise() * DMath.Sin(angle / 2), DMath.Cos(angle / 2)).Normalise();
        }

        public static Quaternion Lerp(Quaternion start, Quaternion end, decimal blend)
        {
            // Clone to avoid modifying the parameters
            Quaternion q1 = start.Clone();
            Quaternion q2 = end.Clone();

            // if either input is zero, return the other.
            if (q1.Magnitude() == 0) return q2.Magnitude() == 0 ? Identity : q2;
            if (q2.Magnitude() == 0) return q1;

            decimal blendA = 1 - blend;
            decimal blendB = blend;

            Quaternion result = new Quaternion(blendA * q1.Vector + blendB * q2.Vector, blendA * q1.W + blendB * q2.W);
            return result.Magnitude() > 0 ? result.Normalise() : Identity;
        }

        public static Quaternion Slerp(Quaternion start, Quaternion end, decimal blend)
        {
            // Clone to avoid modifying the parameters
            Quaternion q1 = start.Clone();
            Quaternion q2 = end.Clone();

            // if either input is zero, return the other.
            if (q1.Magnitude() == 0) return q2.Magnitude() == 0 ? Identity : q2;
            if (q2.Magnitude() == 0) return q1;

            decimal cosHalfAngle = q1.Dot(q2);

            if (cosHalfAngle >= 1 || cosHalfAngle <= -1) return q1;

            if (cosHalfAngle < 0)
            {
                q2.Vector = -q2.Vector;
                q2.Scalar = -q2.Scalar;
                cosHalfAngle = -cosHalfAngle;
            }

            decimal blendA;
            decimal blendB;
            if (cosHalfAngle < 0.99m)
            {
                // do proper slerp for big angles
                decimal halfAngle = DMath.Acos(cosHalfAngle);
                decimal sinHalfAngle = DMath.Sin(halfAngle);
                decimal oneOverSinHalfAngle = 1 / sinHalfAngle;
                blendA = DMath.Sin(halfAngle * (1 - blend)) * oneOverSinHalfAngle;
                blendB = DMath.Sin(halfAngle * blend) * oneOverSinHalfAngle;
            }
            else
            {
                // do lerp if angle is really small.
                blendA = 1 - blend;
                blendB = blend;
            }

            Quaternion result = new Quaternion(blendA * q1.Vector + blendB * q2.Vector, blendA * q1.W + blendB * q2.W);
            return result.Magnitude() > 0 ? result.Normalise() : Identity;
        }
    }
}
