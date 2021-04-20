using System;
using System.Collections.Generic;

namespace FileToVox.Schematics.Tools
{
    public struct Vector3Int : IEquatable<Vector3Int>
    {
        public int X;
        public int Y;
        public int Z;

        // Creates a new vector with given x, y, z components.
        public Vector3Int(int x, int y, int z) { this.X = x; this.Y = y; this.Z = z; }
        // Creates a new vector with given x, y components and sets /z/ to zero.
        public Vector3Int(int x, int y) { this.X = x; this.Y = y; Z = 0; }

        public static Vector3Int zero { get; } = new Vector3Int(0, 0, 0);
        public int magnitude => (int)Math.Sqrt(X * X + Y * Y + Z * Z);
        public static int SqrMagnitude(Vector3Int vector) { return vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z; }

        // Subtracts one vector from another.
        public static Vector3Int operator -(Vector3Int a, Vector3Int b) { return new Vector3Int(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }
        // Adds two vectors.
        public static Vector3Int operator +(Vector3Int a, Vector3Int b) { return new Vector3Int(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
        // Subtracts one vector from another.
        // Negates a vector.
        public static Vector3Int operator -(Vector3Int a) { return new Vector3Int(-a.X, -a.X, -a.Z); }
        // Multiplies a vector by a number.
        public static Vector3Int operator *(Vector3Int a, int d) { return new Vector3Int(a.X * d, a.Y * d, a.Z * d); }
        // Multiplies a vector by a number.
        public static Vector3Int operator *(int d, Vector3Int a) { return new Vector3Int(a.X * d, a.Y * d, a.Z * d); }
        // Divides a vector by a number.
        public static Vector3Int operator /(Vector3Int a, int d) { return new Vector3Int(a.X / d, a.Y / d, a.Z / d); }

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1})", X, Y, Z);
        }

        public bool Equals(Vector3Int other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z);
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector3Int)) return false;

            return Equals((Vector3Int)other);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2);
        }
    
    }

}
