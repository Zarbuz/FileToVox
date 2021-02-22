using FileToVox.Schematics;
using System;
using FileToVox.Generator.Terrain.Data;
using FileToVox.Generator.Terrain.Environment;

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
