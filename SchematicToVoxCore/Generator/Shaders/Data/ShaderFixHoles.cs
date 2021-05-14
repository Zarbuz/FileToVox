using System;

namespace FileToVox.Generator.Shaders.Data
{
	public class ShaderFixHoles :ShaderStep
	{
		public override ShaderType ShaderType { get; set; } = ShaderType.FIX_HOLES;
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
