using System;
using System.Collections.Generic;
using FileToVox.Generator.Terrain.Data;
using FileToVox.Schematics;
using FileToVox.Schematics.Tools;

namespace FileToVox.Generator.Terrain
{
	public class TerrainGenerator : IGenerator
	{
		public TerrainGenerator(WorldTerrainData worldTerrainData)
		{
			TerrainEnvironment.Instance.MainInitialize(worldTerrainData);
		}

		public Schematic WriteSchematic()
		{
			TerrainEnvironment.Instance.StartGeneration();
			Schematic schematic = GetSchematic();
			TerrainEnvironment.Instance.DisposeAll();
			return schematic;
		}

		private Schematic GetSchematic()
		{
			int width = Math.Min(Math.Max(TerrainEnvironment.Instance.WorldTerrainData.Width, TerrainEnvironment.Instance.WorldTerrainData.Length), 2000);
			int chunkXZDistance = width / 2;
			int chunkYDistance = 1000 / 2;

			int visibleXMin = -chunkXZDistance;
			int visibleXMax = chunkXZDistance - 1;

			int visibleZMin = -chunkXZDistance;
			int visibleZMax = chunkXZDistance - 1;

			int visibleYMin = -chunkYDistance;
			int visibleYMax = chunkYDistance - 1;

			uint[,,] voxels = TerrainEnvironment.Instance.GetVoxels(new Vector3(visibleXMin, visibleYMin, visibleZMin), new Vector3(visibleXMax, visibleYMax, visibleZMax));
			Schematic schematic = new Schematic();
			for (int y = 0; y < voxels.GetLength(0); y++)
			{
				for (int z = 0; z < voxels.GetLength(1); z++)
				{
					for (int x = 0; x < voxels.GetLength(2); x++)
					{
						if (voxels[y, z, x] != 0)
						{
							schematic.Blocks.Add(new Voxel((ushort)x, (ushort)y, (ushort)z, voxels[y, z, x]));
						}
					}
				}
			}

			return schematic;
		}
	}
}
