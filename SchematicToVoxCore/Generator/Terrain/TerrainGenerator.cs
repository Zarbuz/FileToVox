using FileToVox.Schematics;
using FileToVox.Schematics.Tools;
using System;
using WorldTerrainData = FileToVox.Generator.Terrain.Data.WorldTerrainData;

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

			return TerrainEnvironment.Instance.CreateSchematic(new Vector3(visibleXMin, visibleYMin, visibleZMin), new Vector3(visibleXMax, visibleYMax, visibleZMax));
		}
	}
}
