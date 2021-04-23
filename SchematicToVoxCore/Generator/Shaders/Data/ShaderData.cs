using System;
using System.Collections.Generic;
using FileToVox.Converter.Json;

namespace FileToVox.Generator.Shaders
{
	public class ShaderData : JsonBaseImportData
	{
		public ShaderStep[] Steps { get; set; }
	}

	public enum ShaderType
	{
		FIX_HOLES,
		FIX_LONELY,
		CASE,
		PATINA
	}

	public class ShaderStep
	{
		public ShaderType ShaderType { get; set; }
		public int Iterations { get; set; }
		public int TargetColorIndex { get; set; }
		public int AdditionalColorRange { get; set; }
		public int Seed { get; set; }
		public float Density { get; set; }
		public int Thickness { get; set; }


		public void DisplayInfo()
		{
			Console.WriteLine("[INFO] ShaderType: " + ShaderType);
			Console.WriteLine("[INFO] ###############################");
			switch (ShaderType)
			{
				case ShaderType.CASE:
					Console.WriteLine("[INFO] Iterations: " + Iterations);
					break;
				case ShaderType.PATINA:
					Console.WriteLine("[INFO] TargetColorIndex: " + TargetColorIndex);
					Console.WriteLine("[INFO] AdditionalColorRange: " + AdditionalColorRange);
					Console.WriteLine("[INFO] Seed: " + Seed);
					Console.WriteLine("[INFO] Density: " + Density);
					Console.WriteLine("[INFO] Thickness: " + Thickness);
					break;
			}
			Console.WriteLine("[INFO] ###############################");
		}

		public void ValidateSettings()
		{
			switch (ShaderType)
			{
				case ShaderType.CASE:
					ValidateCaseSettings();
					break;
				case ShaderType.PATINA:
					ValidatePatinaSettings();
					break;
			}
		}

		private void ValidateCaseSettings()
		{
			if (Iterations < 0)
			{
				Console.WriteLine("[WARNING] Negative value found for Iterations, replace to 1...");
				Iterations = 1;
			}

			if (Iterations > 10)
			{
				Console.WriteLine("[WARNING] Replace maximal value of Iterations to 10");
				Iterations = 10;
			}
		}

		private void ValidatePatinaSettings()
		{
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

			if (Thickness < 0)
			{
				Console.WriteLine("[WARNING] Negative value found for Thickness, replace to 4...");
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
