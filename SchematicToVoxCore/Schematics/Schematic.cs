using System.Collections.Generic;

namespace FileToVox.Schematics
{
	public class Schematic
    {
        public ushort Width { get; set; }
        public ushort Height { get; set; }
        public ushort Length { get; set; }
        /// <summary>Contains all usual blocks</summary>
        public HashSet<Block> Blocks { get; set; }

        public Schematic()
        {
            Blocks = new HashSet<Block>();
        }

        public Schematic(ushort width, ushort height, ushort length)
        {
            Width = width;
            Height = height;
            Length = length;
        }

        public Schematic(ushort width, ushort height, ushort length, HashSet<Block> blocks) : this(width, height, length)
        {
            Blocks = blocks;
        }
       
        
    }
}
