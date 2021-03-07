using System.Text.Json.Serialization;
using FileToVox.Converter.Json;

namespace FileToVox.Generator.Terrain.Data
{
	public class WorldTerrainData : JsonBaseImportData
	{
		public int Seed { get; set; }
		public BiomeSettings[] Biomes { get; set; }

		public BiomeSettings DefaultBiome { get; set; }

		public TerrainGeneratorSettings TerrainGeneratorSettings { get; set; }

		[JsonIgnore] public string DirectoryPath { get; set; }
	}
}
