using System;
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

		public void DisplayInfo()
		{
			Console.WriteLine("[INFO] ShaderType: " + ShaderType);
			Console.WriteLine("[INFO] ###############################");
			switch (ShaderType)
			{
				case ShaderType.CASE:
					Console.WriteLine("[INFO] Iterations: " + Iterations);
					break;
			}
			Console.WriteLine("[INFO] ###############################");
		}
	}
}
