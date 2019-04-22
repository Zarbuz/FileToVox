using System;
using System.Runtime.InteropServices;

namespace FileToVox.Schematics
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Block : IEquatable<Block>
    {
        public readonly short X;
        public readonly short Y;
        public readonly short Z;
        public readonly uint Color;

        public Block(short x, short y, short z, uint color)
        {
            X = x;
            Y = y;
            Z = z;
            Color = color;
        }

        public override int GetHashCode()
        {
            //the index of the block at X,Y,Z is (Y×length + Z)×width + X
            return (Y * SchematicReader.LengthSchematic + Z) * SchematicReader.WidthSchematic + X;
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

    public struct BlockV1 : IEquatable<BlockV1>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;
        public readonly uint Color;

        public BlockV1(int x, int y, int z, uint color)
        {
            X = x;
            Y = y;
            Z = z;
            Color = color;
        }

        public override int GetHashCode()
        {
            //the index of the block at X,Y,Z is (Y×length + Z)×width + X
            return (Y * SchematicReader.LengthSchematic + Z) * SchematicReader.WidthSchematic + X;
        }

        public bool Equals(BlockV1 other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Z: {Z}, Color: {Color.ToString()}";
        }

    }
}
