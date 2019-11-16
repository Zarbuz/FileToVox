using System;
using System.Runtime.InteropServices;

namespace FileToVox.Schematics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Block : IEquatable<Block>
    {
        public readonly ushort X;
        public readonly ushort Y;
        public readonly ushort Z;
        public readonly uint Color;

        public Block(ushort x, ushort y, ushort z, uint color)
        {
            X = x;
            Y = y;
            Z = z;
            Color = color;
        }

        public override int GetHashCode()
        {
            //the index of the block at X,Y,Z is (Y×length + Z)×width + X
            return (Y * LoadedSchematic.LengthSchematic + Z) * LoadedSchematic.WidthSchematic + X;
        }

        public bool Equals(Block other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Z: {Z}, Color: {Color.ToString()}";
        }

    }
}
