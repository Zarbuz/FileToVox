using System;
using System.ComponentModel;
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
		[Description("Case")]
		CASE = 0,
		[Description("Color Denoiser")]
		COLOR_DENOISER = 1,
		[Description("Fix Holes")]
		FIX_HOLES = 2,
		[Description("Fix Lonely")]
		FIX_LONELY = 3,
		[Description("Patina")]
		PATINA = 4,
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
