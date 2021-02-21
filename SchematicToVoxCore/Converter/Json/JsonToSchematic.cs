using FileToVox.Generator;
using FileToVox.Generator.Terrain;
using FileToVox.Generator.Terrain.Data;
using FileToVox.Schematics;
using Newtonsoft.Json;
using System;
using System.IO;

namespace FileToVox.Converter.Json
{
	public class JsonToSchematic : AbstractToSchematic
	{
		private IGenerator _generator;
		public JsonToSchematic(string path) : base(path)
		{
			JsonBaseImportData data = JsonConvert.DeserializeObject<JsonBaseImportData>(File.ReadAllText(path));

			switch (data.GeneratorType)
			{
				case GeneratorType.Terrain:
					_generator = new TerrainGenerator(data as WorldTerrainData);
					break;
				case GeneratorType.City:
					break;
			}
			Console.WriteLine(data);
		}

		public override Schematic WriteSchematic()
		{
			return _generator.WriteSchematic();
		}
	}

}
