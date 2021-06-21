using FileToVoxCommon.Json;
using Newtonsoft.Json;

namespace FileToVox.Generator.Terrain.Data
{
	public class WorldTerrainData : JsonBaseImportData
	{
		public override GeneratorType GeneratorType { get; set; } = GeneratorType.Terrain;
		public int Width { get; set; }
		public int Length { get; set; }

		public int Seed { get; set; }
		public BiomeSettings[] Biomes { get; set; }

		public BiomeSettings DefaultBiome { get; set; }

		public TerrainGeneratorDataSettings TerrainGeneratorDataSettings { get; set; }

		[JsonIgnore] public string DirectoryPath { get; set; }
	}
}
