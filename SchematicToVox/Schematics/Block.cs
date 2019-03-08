using SchematicToVox.Schematics.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Schematics
{
    public struct Block : IEquatable<Block>
    {
        public readonly int X;
        public readonly int Y;
        public readonly int Z;
        public readonly Color32 Color;

        public Block(int x, int y, int z, Color32 color)
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
}
