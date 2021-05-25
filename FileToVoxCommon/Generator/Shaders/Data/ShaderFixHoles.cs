using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FileToVoxCommon.Generator.Shaders.Data
{
	public class ShaderFixHoles :ShaderStep
	{
		public override ShaderType ShaderType { get; set; } = ShaderType.FIX_HOLES;

		[Description("Iterations: Set the number of times the shader will be applied for this step")]
		[Range(1, 10)]
		public int Iterations { get; set; } = 1;

		public override void DisplayInfo()
		{
			base.DisplayInfo();
			Console.WriteLine("[INFO] Iterations: " + Iterations);
		}

		public override void ValidateSettings()
		{
			if (Iterations < 0)
			{
				Console.WriteLine("[WARNING] Negative value found for Iterations, replace to 1...");
				Iterations = 1;
			}
		}
	}
}
