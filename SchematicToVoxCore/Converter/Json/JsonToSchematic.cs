using FileToVox.Generator;
using FileToVox.Generator.Terrain;
using FileToVox.Generator.Terrain.Data;
using FileToVox.Schematics;
using Newtonsoft.Json;
using System;
using System.IO;
using FileToVox.Generator.Heightmap;
using FileToVox.Generator.Heightmap.Data;
using FileToVox.Generator.Shaders;

namespace FileToVox.Converter.Json
{
	public class JsonToSchematic : AbstractToSchematic
	{
		private IGenerator mGenerator;
		private Schematic mSchematic;
		public JsonToSchematic(string path) : base(path)
		{
			ParseFile();
		}

		public JsonToSchematic(string path, Schematic schematic) : base(path)
		{
			mSchematic = schematic;
			ParseFile();
		}

		private void ParseFile()
		{
			try
			{
				JsonBaseImportData data = JsonConvert.DeserializeObject<JsonBaseImportData>(File.ReadAllText(Path));
				if (data != null)
				{
					Console.WriteLine("[INFO] GeneratorType: " + data.GeneratorType);
					switch (data.GeneratorType)
					{
						case GeneratorType.Terrain:
							string directoryName = System.IO.Path.GetDirectoryName(Path);
							WorldTerrainData worldTerrainData = data as WorldTerrainData;
							worldTerrainData.DirectoryPath = directoryName;
							mGenerator = new TerrainGenerator(worldTerrainData);
							break;
						case GeneratorType.Heightmap:
							HeightmapData heightmapData = data as HeightmapData;
							mGenerator = new HeightmapGenerator(heightmapData, mSchematic);
							break;
						case GeneratorType.Shader:
							ShaderData shaderData = data as ShaderData;
							mGenerator = new ShaderGenerator(shaderData, mSchematic);
							break;
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine("[ERROR] Failed to parse the JSON file: " + e.Message);
			}
		}

		public override Schematic WriteSchematic()
		{
			return mGenerator.WriteSchematic();
		}

	}

}
