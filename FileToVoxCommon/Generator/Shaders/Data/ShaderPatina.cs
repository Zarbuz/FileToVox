using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FileToVoxCommon.Generator.Shaders.Data
{
	public class ShaderPatina : ShaderStep
	{
		[Description("Iterations: Set the number of times the shader will be applied for this step")]
		[Range(1, 10)]
		public int Iterations { get; set; } = 1;

		[Description("TargetColorIndex: The index of the color target")]
		[Range(0, 256)]
		public int TargetColorIndex { get; set; } = -1;

		[Description("AdditionalColorRange: The additional index range")]
		[Range(0, 256)]
		public int AdditionalColorRange { get; set; }

		[Description("Seed: Using the shader on the same scene will always yield the exact same result as long as you don't change this value. Play with this to yield different patterns on the same scene.")]
		[Range(0, 100000)]
		public int Seed { get; set; }

		[Description("Density: This defines the probability that a voxel is painted in one step. The higher the value the more aggressive the spread of the patina")]
		[Range(0.0f, 1.0f)]
		public float Density { get; set; }

		[Description("Thickness: This influences the color placement pattern of the patina. Just play around with it. Higher values might cause MagicaVoxel to crash due to high computational effort")]
		[Range(1, 100)]
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
