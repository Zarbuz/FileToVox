using System;

namespace FileToVox.Generator.Shaders.Data
{
	public class ShaderPatina : ShaderStep
	{
		public int Iterations { get; set; } = 1;
		public int TargetColorIndex { get; set; } = -1;
		public int AdditionalColorRange { get; set; }
		public int Seed { get; set; }
		public float Density { get; set; }
		public int Thickness { get; set; }

		public override ShaderType ShaderType { get; set; }= ShaderType.PATINA;
		public override void DisplayInfo()
		{
			base.DisplayInfo();
			Console.WriteLine("[INFO] Iterations: " + Iterations);
			Console.WriteLine("[INFO] TargetColorIndex: " + TargetColorIndex);
			Console.WriteLine("[INFO] AdditionalColorRange: " + AdditionalColorRange);
			Console.WriteLine("[INFO] Seed: " + Seed);
			Console.WriteLine("[INFO] Density: " + Density);
			Console.WriteLine("[INFO] Thickness: " + Thickness);
		}

		public override void ValidateSettings()
		{
			if (Iterations < 0)
			{
				Console.WriteLine("[WARNING] Negative value found for Iterations, replace to 1...");
				Iterations = 1;
			}

			if (TargetColorIndex < 0)
			{
				Console.WriteLine("[WARNING] Negative value found for TargetColorIndex, replace to 0...");
				TargetColorIndex = 0;
			}

			int additionalColorRange = Math.Abs(AdditionalColorRange);
			if (TargetColorIndex - additionalColorRange < 0)
			{
				Console.WriteLine("[WARNING] TargetColorIndex - " + additionalColorRange + " < 0");
				AdditionalColorRange = 0;
			}
			else if (TargetColorIndex + additionalColorRange > 255)
			{
				Console.WriteLine($"[WARNING] TargetColorIndex: ${TargetColorIndex} + " + additionalColorRange + " > 255");
				AdditionalColorRange = 0;
			}

			if (Density < 0 || Density > 1)
			{
				Console.WriteLine($"[WARNING] Density must be between 0 and 1");
				Density = 0.30f;
			}

			if (Seed < 0)
			{
				Console.WriteLine("[WARNING] Negative value found for Seed, replace to 123...");
				Seed = 123;
			}

			if (Thickness < 0 || Thickness > 100)
			{
				Console.WriteLine("[WARNING] Thickness value must be between 0 and 100");
				Thickness = 4;
			}

			if (Thickness > 100)
			{
				Console.WriteLine("[WARNING] Value > 100 found for Thickness, replace to 100 ...");
				Thickness = 100;
			}
		}
	}
}
