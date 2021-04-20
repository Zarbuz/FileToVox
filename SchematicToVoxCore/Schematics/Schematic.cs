using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FileToVox.Vox;
using SchematicToVoxCore.Extensions;
using Region = FileToVox.Vox.Region;

namespace FileToVox.Schematics
{
	public class Schematic
	{
		#region ConstStatic

		public const int MAX_WORLD_WIDTH = 2000;
		public const int MAX_WORLD_HEIGHT = 1000;
		public const int MAX_WORLD_LENGTH = 2000;

		public static ulong GetVoxelIndex(int x, int y, int z)
		{
			return (ulong)((y * MAX_WORLD_LENGTH + z) * MAX_WORLD_WIDTH + x);
		}

		public static int GetVoxelIndex2D(int x, int z)
		{
			return x + MAX_WORLD_WIDTH * z;
		}

		#endregion

		#region Fields

		private ushort mWidth;

		public ushort Width
		{
			get
			{
				if (mWidth == 0)
				{
					mWidth = (ushort)(BlockDict.Values.Max(v => v.X) - BlockDict.Values.Min(v => v.X) + 1);
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
					mHeight = (ushort)(BlockDict.Values.Max(v => v.Y) - BlockDict.Values.Min(v => v.Y) + 1);
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
					mLength = (ushort)(BlockDict.Values.Max(v => v.Z) - BlockDict.Values.Min(v => v.Z) + 1);
				}

				return mLength;
			}
		}

		/// <summary>Contains all usual blocks</summary>
		//public HashSet<Voxel> Blocks { get; set; }

		public Dictionary<ulong, Voxel> BlockDict { get; private set; }
		public Dictionary<ulong, Region> RegionDict { get; private set; }
		
		public Schematic()
		{
			//Blocks = new HashSet<Voxel>();
			BlockDict = new Dictionary<ulong, Voxel>();
			CreateAllRegions();
		}

		public Schematic(Dictionary<ulong, Voxel> voxels)
		{
			BlockDict = new Dictionary<ulong, Voxel>();
			CreateAllRegions();
			AddVoxels(voxels.Values);
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


		#endregion

		#region PublicMethods

		public void AddVoxels(IEnumerable<Voxel> voxels)
		{
			foreach (Voxel voxel in voxels)
			{
				AddVoxel(voxel);
			}
		}

		public void AddVoxel(Voxel voxel)
		{
			AddVoxel(voxel.X, voxel.Y, voxel.Z, voxel.Color);
		}

		public void AddVoxel(int x, int y, int z, Color color)
		{
			AddVoxel(x, y, z, color.ColorToUInt());
		}

		public void AddVoxel(int x, int y, int z, uint color)
		{
			AddVoxel(x, y, z, color, -1);
		}

		public void AddVoxel(int x, int y, int z, uint color, int palettePosition)
		{
			if (color != 0)
			{
				BlockDict[GetVoxelIndex(x, y, z)] = new Voxel((ushort)x, (ushort)y, (ushort)z, color, palettePosition);
				AddUsageForRegion(x, y, z);
			}
		}

		public void ReplaceVoxel(int x, int y, int z, uint color)
		{
			BlockDict[GetVoxelIndex(x, y, z)].Color = color;
		}

		public void RemoveVoxel(int x, int y, int z)
		{
			ulong index = GetVoxelIndex(x, y, z);
			if (BlockDict.ContainsKey(index))
			{
				BlockDict.Remove(index);
				RemoveUsageForRegion(x, y, z);
			}
		}

		public uint GetColorAtVoxelIndex(int x, int y, int z)
		{
			ulong voxelIndex = GetVoxelIndex(x, y, z);
			//if (BlockDict.ContainsKey(voxelIndex))
			//{
			//	return BlockDict[voxelIndex].Color;
			//}
			//return 0;
			BlockDict.TryGetValue(voxelIndex, out Voxel foundVoxel);
			return foundVoxel?.Color ?? 0;
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

		public List<Region> GetAllRegions()
		{
			return RegionDict.Values.Where(region => region.UsageCount > 0).ToList();
		}

		public List<Voxel> GetVoxelInRegion(HashSet<ulong> voxelIndex)
		{
			return voxelIndex.Select(vi => BlockDict[vi]).ToList();
		}
		#endregion

		#region PrivateMethods

		private void CreateAllRegions()
		{
			RegionDict = new Dictionary<ulong, Region>();

			int worldRegionX = (int)Math.Ceiling(((decimal)MAX_WORLD_WIDTH / Program.CHUNK_SIZE));
			int worldRegionY = (int)Math.Ceiling(((decimal)MAX_WORLD_HEIGHT / Program.CHUNK_SIZE));
			int worldRegionZ =(int)Math.Ceiling(((decimal)MAX_WORLD_LENGTH / Program.CHUNK_SIZE));

			int countSize = worldRegionX * worldRegionY * worldRegionZ;

			for (int i = 0; i < countSize; i++)
			{
				int x = i % worldRegionX;
				int y = (i / worldRegionX) % worldRegionY;
				int z = i / (worldRegionX * worldRegionY);

				RegionDict[GetVoxelIndex(x, y, z)] = new Region(x * Program.CHUNK_SIZE, y * Program.CHUNK_SIZE, z * Program.CHUNK_SIZE);
			}

		}

		private void AddUsageForRegion(int x, int y, int z)
		{
			int chunkX = x / Program.CHUNK_SIZE;
			int chunkY = y / Program.CHUNK_SIZE;
			int chunkZ = z / Program.CHUNK_SIZE;
			ulong chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			ulong voxelIndex = GetVoxelIndex(x, y, z);

			RegionDict[chunkIndex].UsageCount++;
			RegionDict[chunkIndex].VoxelIndexUsed.Add(voxelIndex);
		}

		private void RemoveUsageForRegion(int x, int y, int z)
		{
			int chunkX = x / Program.CHUNK_SIZE;
			int chunkY = y / Program.CHUNK_SIZE;
			int chunkZ = z / Program.CHUNK_SIZE;
			ulong chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			ulong voxelIndex = GetVoxelIndex(x, y, z);

			RegionDict[chunkIndex].UsageCount--;
			RegionDict[chunkIndex].VoxelIndexUsed.Remove(voxelIndex);

		}

		

		#endregion
	}
}
