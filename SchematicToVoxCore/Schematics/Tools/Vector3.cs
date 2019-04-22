using System;

namespace FileToVox.Schematics.Tools
{
    public struct Vector3 : IEquatable<Vector3>
    {
        public const float KEpsilon = 0.00001F;
        public const float KEpsilonNormalSqrt = 1e-15F;

        public float X;
        public float Y;
        public float Z;

        // Creates a new vector with given x, y, z components.
        public Vector3(float x, float y, float z) { this.X = x; this.Y = y; this.Z = z; }
        // Creates a new vector with given x, y components and sets /z/ to zero.
        public Vector3(float x, float y) { this.X = x; this.Y = y; Z = 0F; }

        static readonly Vector3 zeroVector = new Vector3(0F, 0F, 0F);
        public static Vector3 zero { get { return zeroVector; } }

        public static float SqrMagnitude(Vector3 vector) { return vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z; }

        // Subtracts one vector from another.
        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1})", X, Y, Z);
        }

        public bool Equals(Vector3 other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3)) return false;

            return Equals((Vector3)other);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2);
        }

        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            // Returns false in the presence of NaN values.
            return SqrMagnitude(lhs - rhs) < KEpsilon * KEpsilon;
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !(left == right);
        }
    }
}
