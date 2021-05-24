using System;
using FileToVoxCommon.Generator.Shaders.Json;
using FileToVoxCommon.Json;
using Newtonsoft.Json;

namespace FileToVoxCommon.Generator.Shaders.Data
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

	public abstract class ShaderStep : StepData.StepData
	{
		public abstract ShaderType ShaderType { get; set; }

		public virtual void DisplayInfo()
		{
			Console.WriteLine("[INFO] ShaderType: " + ShaderType);
		}

		public abstract void ValidateSettings();
	
	}
}
