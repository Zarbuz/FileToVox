using FileToVox.Schematics;
using System;
using FileToVox.Generator.Terrain.Data;

namespace FileToVox.Generator.Terrain
{
	public class TerrainGenerator : IGenerator
	{
		private TerrainEnvironment _terrainEnvironment;

		public TerrainGenerator(WorldTerrainData worldTerrainData)
		{
			_terrainEnvironment = new TerrainEnvironment(worldTerrainData);
		}

		public Schematic WriteSchematic()
		{
			throw new NotImplementedException();
		}
	}
}
