using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FileToVox.Converter.Json
{
	public enum GeneratorType
	{
		Terrain,
		City
	}

	[JsonConverter(typeof(JsonBaseImportDataConverter))]
	public abstract class JsonBaseImportData
	{
		public int Width { get; set; }
		public int Length { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public GeneratorType GeneratorType { get; set; }
	}
}
