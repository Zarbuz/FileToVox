using System;

namespace FileToVox.Generator.Shaders.Data
{
	public class ShaderCase : ShaderStep
	{
		public int Iterations { get; set; }
		public int TargetColorIndex { get; set; } = -1;
		public int AdditionalColorRange { get; set; }

		public override ShaderType ShaderType { get; set; }
		public override void DisplayInfo()
		{
			Console.WriteLine("[INFO] Iterations: " + Iterations);
			Console.WriteLine("[INFO] TargetColorIndex: " + TargetColorIndex);
			Console.WriteLine("[INFO] AdditionalColorRange: " + AdditionalColorRange);
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
