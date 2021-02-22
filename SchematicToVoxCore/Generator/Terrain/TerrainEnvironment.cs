using FileToVox.Generator.Terrain.Data;

namespace FileToVox.Generator.Terrain
{
	public partial class TerrainEnvironment
	{
		public const int CHUNK_SIZE = 16;

		private WorldTerrainData mWorldTerrainData;

		public TerrainEnvironment(WorldTerrainData terrainData)
		{
			mWorldTerrainData = terrainData;
		}
	}
}
