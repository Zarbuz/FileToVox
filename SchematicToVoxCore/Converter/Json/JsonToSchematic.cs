using System;
using System.IO;
using FileToVox.Schematics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FileToVox.Converter.Json
{
	public class JsonToSchematic : AbstractToSchematic
	{
		public JsonToSchematic(string path) : base(path)
		{
			JsonBaseImportData data = JsonConvert.DeserializeObject<JsonBaseImportData>(File.ReadAllText(path));

			switch (data.GeneratorType)
			{
				case GeneratorType.Terrain:
					Test test1 = data as Test;
					break;
				case GeneratorType.City:
					break;
			}
			Console.WriteLine(data);
		}

		public override Schematic WriteSchematic()
		{
			throw new NotImplementedException();
		}
	}

	public class Test : JsonBaseImportData
	{
		public bool Test1 { get; set; }
		public int Test2 { get; set; }
	}
}
