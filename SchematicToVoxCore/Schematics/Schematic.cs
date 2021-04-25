using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using FileToVox.Generator.Heightmap.Data;
using FileToVox.Schematics.Tools;
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
		public const int MAX_COLORS_IN_PALETTE = 256;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static ulong GetVoxelIndex(int x, int y, int z)
		{
			return (ulong)((y * MAX_WORLD_LENGTH + z) * MAX_WORLD_WIDTH + x);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static int GetVoxelIndex2D(int x, int z)
		{
			return x + MAX_WORLD_WIDTH * z;
		}

		public static int GetVoxelIndex2DFromRotation(int x, int y, int z, RotationMode rotationMode)
		{
			switch (rotationMode)
			{
				case RotationMode.X:
					return GetVoxelIndex2D(z, y);
				case RotationMode.Y:
					return GetVoxelIndex2D(x, z);
				case RotationMode.Z:
					return GetVoxelIndex2D(x, y);
			}
			return GetVoxelIndex2D(x, z);
		}

		#endregion

		#region Fields

		private ushort mWidth;

		public ushort Width
		{
			get
			{
				if (UsedColors.Count == 0)
				{
					mWidth = 0;
					return mWidth;
				}

				if (mWidth == 0)
				{
					mWidth = (ushort)(mMaxX - mMinX + 1);
				}

				return mWidth;
			}
		}

		private ushort mHeight;

		public ushort Height
		{
			get
			{
				if (UsedColors.Count == 0)
				{
					mHeight = 0;
					return mHeight;
				}

				if (mHeight == 0)
				{
					mHeight = (ushort)(mMaxY - mMinY + 1);
				}

				return mHeight;
			}
		}

		private ushort mLength;

		public ushort Length
		{
			get
			{
				if (UsedColors.Count == 0)
				{
					mLength = 0;
					return mLength;
				}

				if (mLength == 0)
				{
					mLength = (ushort)(mMaxZ - mMinZ + 1);
				}

				return mLength;
			}
		}

		public Dictionary<ulong, Region> RegionDict { get; private set; }
		public List<uint> UsedColors { get; private set; }

		private int mMinX;
		private int mMinY;
		private int mMinZ;

		private int mMaxX;
		private int mMaxY;
		private int mMaxZ;

		public Schematic()
		{
			UsedColors = new List<uint>();
			CreateAllRegions();
		}

		public Schematic(List<Voxel> voxels)
		{
			UsedColors = new List<uint>();
			CreateAllRegions();
			AddVoxels(voxels);
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
			if (color != 0 && x < MAX_WORLD_WIDTH && y < MAX_WORLD_HEIGHT && z < MAX_WORLD_LENGTH)
			{
				AddColorInUsedColors(color);
				if (palettePosition == -1)
				{
					palettePosition = GetPaletteIndex(color);
				}
				AddUsageForRegion(x, y, z, color, palettePosition);
				ComputeMinMax(x, y, z);
			}
		}

		public void ReplaceVoxel(Voxel voxel, uint color)
		{
			ReplaceVoxel(voxel.X, voxel.Y, voxel.Z, color);
		}

		public void ReplaceVoxel(int x, int y, int z, uint color)
		{
			if (color != 0)
			{
				ReplaceUsageForRegion(x, y, z, color);
				AddColorInUsedColors(color);
			}
		}

		public void RemoveVoxel(int x, int y, int z)
		{
			RemoveUsageForRegion(x, y, z);
		}

		public uint GetColorAtVoxelIndex(Vector3Int pos)
		{
			return GetVoxel(pos.X, pos.Y, pos.Z, out Voxel voxel) ? voxel.Color : 0;
		}

		public uint GetColorAtVoxelIndex(int x, int y, int z)
		{
			return GetVoxel(x, y, z, out Voxel voxel) ? voxel.Color : 0;
		}

		public bool ContainsVoxel(int x, int y, int z)
		{
			return GetVoxel(x, y, z, out _);
		}

		public bool GetVoxel(int x, int y, int z, out Voxel voxel)
		{
			int chunkX = x / Program.CHUNK_SIZE;
			int chunkY = y / Program.CHUNK_SIZE;
			int chunkZ = z / Program.CHUNK_SIZE;
			ulong chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			ulong voxelIndex = GetVoxelIndex(x, y, z);
			bool found = RegionDict[chunkIndex].BlockDict.TryGetValue(voxelIndex, out Voxel foundVoxel);
			voxel = foundVoxel;
			return found;
		}

		public List<Region> GetAllRegions()
		{
			return RegionDict.Values.Where(region => region.BlockDict.Count > 0).ToList();
		}

		public uint GetColorAtPaletteIndex(int index)
		{
			return index > UsedColors.Count ? UsedColors[index] : 0;
		}

		public List<Voxel> GetAllVoxels()
		{
			List<Voxel> voxels = new List<Voxel>();
			foreach (KeyValuePair<ulong, Region> region in RegionDict)
			{
				voxels.AddRange(region.Value.BlockDict.Values);
			}

			return voxels;
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

		private void AddUsageForRegion(int x, int y, int z, uint color, int palettePosition)
		{
			int chunkX = x / Program.CHUNK_SIZE;
			int chunkY = y / Program.CHUNK_SIZE;
			int chunkZ = z / Program.CHUNK_SIZE;
			ulong chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			ulong voxelIndex = GetVoxelIndex(x, y, z);

			RegionDict[chunkIndex].UsageCount++;
			RegionDict[chunkIndex].BlockDict[voxelIndex] = new Voxel((ushort)x, (ushort)y, (ushort)z, color, palettePosition);
		}

		private void ReplaceUsageForRegion(int x, int y, int z, uint color)
		{
			int chunkX = x / Program.CHUNK_SIZE;
			int chunkY = y / Program.CHUNK_SIZE;
			int chunkZ = z / Program.CHUNK_SIZE;
			ulong chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			ulong voxelIndex = GetVoxelIndex(x, y, z);

			if (RegionDict[chunkIndex].BlockDict.ContainsKey(voxelIndex))
			{
				RegionDict[chunkIndex].BlockDict[voxelIndex].Color = color;
			}
		}

		private void RemoveUsageForRegion(int x, int y, int z)
		{
			int chunkX = x / Program.CHUNK_SIZE;
			int chunkY = y / Program.CHUNK_SIZE;
			int chunkZ = z / Program.CHUNK_SIZE;
			ulong chunkIndex = GetVoxelIndex(chunkX, chunkY, chunkZ);
			ulong voxelIndex = GetVoxelIndex(x, y, z);

			if (RegionDict[chunkIndex].BlockDict.ContainsKey(voxelIndex))
			{
				RegionDict[chunkIndex].BlockDict.Remove(voxelIndex);
			}
		}

		private void AddColorInUsedColors(uint color)
		{
			if (UsedColors.Count < MAX_COLORS_IN_PALETTE && !UsedColors.Contains(color))
			{
				UsedColors.Add(color);
			}
		}

		private int GetPaletteIndex(uint color)
		{
			return UsedColors.IndexOf(color);
		}

		private void ComputeMinMax(int x, int y, int z)
		{
			mMinX = Math.Min(x, mMinX);
			mMinY = Math.Min(y, mMinY);
			mMinZ = Math.Min(z, mMinZ);


			mMaxX = Math.Max(x, mMaxX);
			mMaxY = Math.Max(y, mMaxY);
			mMaxZ = Math.Max(z, mMaxZ);
		}
		#endregion
	}
}
