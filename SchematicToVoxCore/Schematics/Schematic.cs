using System.Collections.Generic;

namespace FileToVox.Schematics
{
	public class Schematic
    {
        public ushort Width { get; set; }
        public ushort Heigth { get; set; }
        public ushort Length { get; set; }
        /// <summary>Contains all usual blocks</summary>
        public HashSet<Block> Blocks { get; set; }
        /// <summary>Returns how much blocks and tile entities there are in total.</summary>
        public int TotalCount { get { return Blocks.Count; } }

        public Schematic()
        {
            Blocks = new HashSet<Block>();
        }

        public Schematic(string name, ushort width, ushort heigth, ushort length)
        {
            this.Width = width;
            this.Heigth = heigth;
            this.Length = length;
        }

        public Schematic(string name, ushort width, ushort heigth, ushort length, HashSet<Block> blocks) : this(name, width, heigth, length)
        {
            this.Blocks = blocks;
        }
       
        
    }
}
