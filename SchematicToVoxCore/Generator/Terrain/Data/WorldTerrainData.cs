using FileToVox.Converter.Json;

namespace FileToVox.Generator.Terrain.Data
{
	public class WorldTerrainData : JsonBaseImportData
	{
		public int Seed { get; set; }
		public BiomeDefinition[] Biomes { get; set; }

		public BiomeDefinition DefaultBiome { get; set; }
	}
}
