using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FileToVoxCommon.Json
{
	public enum GeneratorType
	{
		Terrain,
		Heightmap,
		Shader
	}

	[JsonConverter(typeof(JsonBaseImportDataConverter))]
	public abstract class JsonBaseImportData
	{
		[JsonConverter(typeof(StringEnumConverter))]
		public GeneratorType GeneratorType { get; set; }
	}
}
