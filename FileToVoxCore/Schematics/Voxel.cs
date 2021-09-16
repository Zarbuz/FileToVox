using System;
using System.Runtime.InteropServices;

namespace FileToVoxCore.Schematics
{
	[StructLayout(LayoutKind.Sequential)]
    public class Voxel : IEquatable<Voxel>
    {
        public readonly ushort X;
        public readonly ushort Y;
        public readonly ushort Z;

        public uint Color;

        public Voxel()
		{

		}

        public Voxel(ushort x, ushort y, ushort z, uint color)
        {
            X = x;
            Y = y;
            Z = z;
            Color = color;
        }

        public override int GetHashCode()
        {
            //the index of the block at X,Y,Z is (Y×length + Z)×width + X
            return (Y * Schematic.MAX_WORLD_LENGTH + Z) * Schematic.MAX_WORLD_WIDTH + X;
        }

        public bool Equals(Voxel other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }

        public override string ToString()
        {
            return $"X: {X}, Y: {Y}, Z: {Z}, Color: {Color.ToString()}";
        }

    }
}
