using FileToVoxCommon.Generator.Shaders.Json;
using FileToVoxCommon.Json;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Converters;

namespace FileToVoxCommon.Generator.Shaders.Data
{
	public class ShaderData : JsonBaseImportData
	{
		public ShaderStep[] Steps { get; set; }
		public override GeneratorType GeneratorType { get; set; } = GeneratorType.Shader;
	}

	public enum ShaderType
	{
		[Display(Name = "Case", Description = "This shader case surrounds / encases the voxels which match your selected color with a chosen color")]
		CASE = 0,

		[Display(Name = "Color Denoiser", Description = "This shader allows you to replace the color of a voxel according to the adjacent voxels")]
		COLOR_DENOISER = 1,

		[Display(Name = "Fix Holes", Description = "This shader is used to fill the holes. A hole is an \"empty\" voxel of which at least 4 adjacent voxels are not empty.")]
		FIX_HOLES = 2,

		[Display(Name = "Fix Lonely", Description = "This shader removes all voxels that have no adjacent voxels.")]
		FIX_LONELY = 3,

		[Display(Name = "Patina", Description = "This shader will grow a patina on your voxels. It won't create new voxels, just change the color. This voxel is based on the patStar shader")]
		PATINA = 4,
	}

	[JsonConverter(typeof(ShaderStepDataConverter))]

	public abstract class ShaderStep : StepData.StepData
	{
		[Browsable(false)]
		[JsonConverter(typeof(StringEnumConverter))]
		public abstract ShaderType ShaderType { get; set; }

		public virtual void DisplayInfo()
		{
			Console.WriteLine("[INFO] ShaderType: " + ShaderType);
		}

		public abstract void ValidateSettings();

	}
}
