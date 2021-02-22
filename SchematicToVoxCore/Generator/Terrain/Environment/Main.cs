using System;
using System.Collections.Generic;
using System.Text;
using FileToVox.Generator.Terrain.Data;

namespace FileToVox.Generator.Terrain.Environment
{
	public partial class TerrainEnvironment
	{
		private WorldTerrainData mWorldTerrainData;

		public TerrainEnvironment(WorldTerrainData terrainData)
		{
			mWorldTerrainData = terrainData;
		}
	}
}
