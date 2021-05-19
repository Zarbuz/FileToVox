using System;

namespace FileToVoxCommon.Generator.Shaders.Data
{
	public class ShaderColorDenoiser : ShaderStep
	{
		public override ShaderType ShaderType { get; set; } = ShaderType.COLOR_DENOISER;
		public int Iterations { get; set; } = 1;
		public bool StrictMode { get; set; } = true;
		public int ColorRange { get; set; }

		public override void DisplayInfo()
		{
			base.DisplayInfo();
			Console.WriteLine("[INFO] Iterations: " + Iterations);
			Console.WriteLine("[INFO] StrictMode: " + StrictMode);
			if (!StrictMode)
			{
				Console.WriteLine("[INFO] ColorRange: " + ColorRange);
			}
		}

		public override void ValidateSettings()
		{
			if (Iterations < 0)
			{
				Console.WriteLine("[WARNING] Negative value found for Iterations, replace to 1...");
				Iterations = 1;
			}

			if (!StrictMode && ColorRange < 0)
			{
				Console.WriteLine("[WARNING] Negative value found for ColorRange, replace to 1...");
				ColorRange = 1;
			}
		}
	}
}
