using System;

namespace FileToVox.Generator.Shaders.Data
{
	public class ShaderCase : ShaderStep
	{
		public int Iterations { get; set; } = 1;
		public int TargetColorIndex { get; set; } = -1;

		public override ShaderType ShaderType { get; set; } = ShaderType.CASE;
		public override void DisplayInfo()
		{
			base.DisplayInfo();
			Console.WriteLine("[INFO] Iterations: " + Iterations);
			Console.WriteLine("[INFO] TargetColorIndex: " + TargetColorIndex);
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
