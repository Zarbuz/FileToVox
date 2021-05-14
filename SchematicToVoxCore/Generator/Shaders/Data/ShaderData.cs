using System;
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
		PATINA,
		COLOR_DENOISER
	}

	[JsonConverter(typeof(ShaderStepDataConverter))]

	public abstract class ShaderStep
	{
		public abstract ShaderType ShaderType { get; set; }

		public virtual void DisplayInfo()
		{
			Console.WriteLine("[INFO] ShaderType: " + ShaderType);
		}

		public abstract void ValidateSettings();
	
	}
}
