using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FileToVox.Schematics
{
	public class Schematic
    {
	    public const int MAX_WORLD_WIDTH = 2000;
	    public const int MAX_WORLD_HEIGHT = 1000;
	    public const int MAX_WORLD_LENGTH = 2000;

		public ushort Width { get; set; }
        public ushort Height { get; set; }
        public ushort Length { get; set; }
        /// <summary>Contains all usual blocks</summary>
        public HashSet<Voxel> Blocks { get; set; }

        public Schematic()
        {
            Blocks = new HashSet<Voxel>();
        }

        public Schematic(ushort width, ushort height, ushort length)
        {
            Width = width;
            Height = height;
            Length = length;
        }

        public Schematic(ushort width, ushort height, ushort length, HashSet<Voxel> blocks) : this(width, height, length)
        {
            Blocks = blocks;
        }

        public List<uint> Colors
        {
	        get
	        {
                List<uint> colors = new List<uint>();
                foreach (Voxel block in Blocks.Where(block => !colors.Contains(block.Color)))
                {
	                colors.Add(block.Color);
                }

                return colors;
	        }
        }
    }
}
