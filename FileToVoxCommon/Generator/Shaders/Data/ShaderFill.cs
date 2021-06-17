using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FileToVoxCommon.Generator.Shaders.Data
{
	public enum FillDirection
	{
		[Display(Name = "Above", Description = "Will fill all voxels above the specific limit")]
		PLUS = 0,

		[Display(Name = "Below", Description = "Will fill all voxels below the specific limit")]
		MINUS = 1
	}

	public enum RotationMode
	{
		X,
		Y,
		Z
	}

	public class ShaderFill : ShaderStep
	{
		[Description("TargetColorIndex: The index of the color target to fill all voxels")]
		[Range(0, 256)]
		public int TargetColorIndex { get; set; } = 0;

		[Range(1, 2000)]
		public int Limit { get; set; } = 1;

		[Description("FillWay: The way voxels should be filled")]
		public FillDirection FillDirection { get; set; } = FillDirection.MINUS;

		[Description("RotationMode: The way voxels should be filled")]
		public RotationMode RotationMode { get; set; } = RotationMode.Y;

		public override ShaderType ShaderType { get; set; } = ShaderType.FILL;
		public override void ValidateSettings()
		{
			
		}

		public override void DisplayInfo()
		{
			base.DisplayInfo();
			Console.WriteLine("[INFO] FillDirection: " + FillDirection);
			Console.WriteLine("[INFO] RotationMode: " + RotationMode);
			Console.WriteLine("[INFO] TargetColorIndex: " + TargetColorIndex);
		}
	}
}
