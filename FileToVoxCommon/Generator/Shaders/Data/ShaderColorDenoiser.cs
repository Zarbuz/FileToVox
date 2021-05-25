using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FileToVoxCommon.Generator.Shaders.Data
{
	public class ShaderColorDenoiser : ShaderStep
	{
		public override ShaderType ShaderType { get; set; } = ShaderType.COLOR_DENOISER;

		[Description("Iterations: Set the number of times the shader will be applied for this step")]
		[Range(1, 10)]
		public int Iterations { get; set; } = 1;

		[Description("StrictMode: Indicates whether the algorithm is in strict mode or not. If so, the 4 adjacent voxels must all be the same color to replace the color of the voxel. Otherwise the algorithm calculates the distance between the index of the color of the voxel and that of the adjacent voxels. If the average distance is less than or equal to the 'colorRange' parameter then the color is replaced by the dominant color of adjacent voxels.")]
		public bool StrictMode { get; set; } = true;

		[Description("ColorRange: Specifies the maximum distance between the color indexes of the palette")]
		[Range(0, 256)]
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
