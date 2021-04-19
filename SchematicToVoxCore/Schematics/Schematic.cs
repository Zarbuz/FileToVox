using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using SchematicToVoxCore.Extensions;

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
				    mWidth = (ushort) (BlockDict.Values.Max(v => v.X) - BlockDict.Values.Min(v => v.X) + 1);
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
				    mHeight = (ushort) (BlockDict.Values.Max(v => v.Y) - BlockDict.Values.Min(v => v.Y) + 1);
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
				    mLength = (ushort) (BlockDict.Values.Max(v => v.Z) - BlockDict.Values.Min(v => v.Z) + 1);
			    }

			    return mLength;
		    }
	    }

	    /// <summary>Contains all usual blocks</summary>
        //public HashSet<Voxel> Blocks { get; set; }

		public Dictionary<ulong, Voxel> BlockDict { get; private set; }

        public Schematic()
        {
            //Blocks = new HashSet<Voxel>();
            BlockDict = new Dictionary<ulong, Voxel>(); //TODO
        }

        public Schematic(Dictionary<ulong, Voxel> blocks)
        {
	        BlockDict = blocks;
        }

        public List<uint> Colors
        {
	        get
	        {
                List<uint> colors = new List<uint>();
                foreach (Voxel block in BlockDict.Values.Where(block => !colors.Contains(block.Color)))
                {
	                colors.Add(block.Color);
                }

                return colors;
	        }
        }

		public static ulong GetVoxelIndex(int x, int y, int z)
		{
			return (ulong) ((y * MAX_WORLD_LENGTH + z) * MAX_WORLD_WIDTH + x);
		}

		public void AddVoxel(Voxel voxel)
		{
			BlockDict[GetVoxelIndex(voxel.X, voxel.Y, voxel.Z)] = voxel;
		}

		public void AddVoxel(int x, int y, int z, Color color)
		{
			BlockDict[GetVoxelIndex(x, y, z)] = new Voxel((ushort) x, (ushort) y, (ushort) z, color.ColorToUInt());
		}

		public void AddVoxel(int x, int y, int z, uint color)
		{
			if (color != 0)
			{
				BlockDict[GetVoxelIndex(x, y, z)] = new Voxel((ushort)x, (ushort)y, (ushort)z, color);
			}
		}

		public void AddVoxel(int x, int y, int z, uint color, int palettePosition)
		{
			BlockDict[GetVoxelIndex(x, y, z)] = new Voxel((ushort)x, (ushort)y, (ushort)z, color, palettePosition);
		}

		public void ReplaceVoxel(int x, int y, int z, uint color)
		{
			Voxel voxel = BlockDict[GetVoxelIndex(x, y, z)];
			voxel.Color = color;
		}

		public void RemoveVoxel(int x, int y, int z)
		{
			ulong index = GetVoxelIndex(x, y, z);
			if (BlockDict.ContainsKey(index))
			{
				BlockDict.Remove(index);
			}
		}

		public uint GetColorAtVoxelIndex(int x, int y, int z)
		{
			ulong voxelIndex = GetVoxelIndex(x, y, z);
			if (BlockDict.ContainsKey(voxelIndex))
			{
				return BlockDict[voxelIndex].Color;
			}
			return 0;
		}

		public bool GetVoxel(int x, int y, int z, out Voxel voxel)
		{
			ulong voxelIndex = GetVoxelIndex(x, y, z);
			bool found =BlockDict.TryGetValue(voxelIndex, out Voxel foundVoxel);
			voxel = foundVoxel;
			return found;
		}

		public bool ContainsVoxel(int x, int y, int z)
		{
			ulong voxelIndex = GetVoxelIndex(x, y, z);
			return BlockDict.TryGetValue(voxelIndex, out _);
		}
	}
}
