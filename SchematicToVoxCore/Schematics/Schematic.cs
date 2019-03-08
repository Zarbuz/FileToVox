using System.Collections.Generic;

namespace SchematicToVoxCore.Schematics
{
    public class Schematic
    {
        public short Width { get; set; }
        public short Heigth { get; set; }
        public short Length { get; set; }
        /// <summary>Contains all usual blocks</summary>
        public HashSet<Block> Blocks { get; set; }
        /// <summary>Returns how much blocks and tile entities there are in total.</summary>
        public int TotalCount { get { return Blocks.Count; } }

        public Schematic()
        {
            Blocks = new HashSet<Block>();
        }

        public Schematic(string name, short width, short heigth, short length)
        {
            this.Width = width;
            this.Heigth = heigth;
            this.Length = length;
        }

        public Schematic(string name, short width, short heigth, short length, HashSet<Block> blocks) : this(name, width, heigth, length)
        {
            this.Blocks = blocks;
        }
       
        
    }
}
