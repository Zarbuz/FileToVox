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

	    private ushort mWidth;

	    public ushort Width
	    {
		    get
		    {
			    if (mWidth == 0)
			    {
				    mWidth = (ushort) (Blocks.Max(v => v.X) + 1);
			    }

			    return mWidth;
		    }
	    }

	    private ushort mHeight;

	    public ushort Height
	    {
		    get
		    {
			    if (mHeight == 0)
			    {
				    mHeight = (ushort) (Blocks.Max(v => v.Y) + 1);
			    }

			    return mHeight;
		    }
	    }

	    private ushort mLength;

	    public ushort Length
	    {
		    get
		    {
			    if (mLength == 0)
			    {
				    mLength = (ushort) (Blocks.Max(v => v.Z) + 1);
			    }

			    return mLength;
		    }
	    }

	    /// <summary>Contains all usual blocks</summary>
        public HashSet<Voxel> Blocks { get; set; }

        public Schematic()
        {
            Blocks = new HashSet<Voxel>();
        }

        public Schematic(HashSet<Voxel> blocks)
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
