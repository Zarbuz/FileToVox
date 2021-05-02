using FileToVox.Converter.Json;
using FileToVox.Generator.Shaders.Json;
using Newtonsoft.Json;

namespace FileToVox.Generator.Shaders
{
	public class ShaderData : JsonBaseImportData
	{
		public ShaderStep[] Steps { get; set; }
	}

	public enum ShaderType
	{
		FIX_HOLES,
		FIX_LONELY,
		CASE,
		PATINA
	}

	[JsonConverter(typeof(ShaderStepDataConverter))]

	public abstract class ShaderStep
	{
		public abstract ShaderType ShaderType { get; set; }

		public abstract void DisplayInfo();

		public abstract void ValidateSettings();
	
	}
}
