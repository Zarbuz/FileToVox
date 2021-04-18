using FileToVox.Generator;
using FileToVox.Generator.Terrain;
using FileToVox.Generator.Terrain.Data;
using FileToVox.Schematics;
using Newtonsoft.Json;
using System;
using System.IO;
using FileToVox.Generator.Heightmap;
using FileToVox.Generator.Heightmap.Data;

namespace FileToVox.Converter.Json
{
	public class JsonToSchematic : AbstractToSchematic
	{
		private readonly IGenerator mGenerator;
		public JsonToSchematic(string path) : base(path)
		{
			JsonBaseImportData data = JsonConvert.DeserializeObject<JsonBaseImportData>(File.ReadAllText(path));
			switch (data.GeneratorType)
			{
				case GeneratorType.Terrain:
					string directoryName = System.IO.Path.GetDirectoryName(path);
					WorldTerrainData worldTerrainData = data as WorldTerrainData;
					worldTerrainData.DirectoryPath = directoryName;
					mGenerator = new TerrainGenerator(worldTerrainData);
					break;
				case GeneratorType.City:
					break;
				case GeneratorType.Heightmap:
					HeightmapData heightmapData = data as HeightmapData;
					mGenerator = new HeightmapGenerator(heightmapData);
					break;
			}
			Console.WriteLine(data);
		}

		public override Schematic WriteSchematic()
		{
			return mGenerator.WriteSchematic();
		}
	}

}
