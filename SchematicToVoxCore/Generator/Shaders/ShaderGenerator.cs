using System;
using FileToVoxCommon.Generator.Shaders.Data;
using FileToVoxCore.Schematics;

namespace FileToVox.Generator.Shaders
{
	public class ShaderGenerator : IGenerator
	{
		private ShaderData mShaderData;
		private Schematic mSchematic;

		public ShaderGenerator(ShaderData shaderData, Schematic schematic)
		{
			mShaderData = shaderData;
			mSchematic = schematic;
		}

		public Schematic WriteSchematic()
		{
			if (mSchematic == null)
			{
				Console.WriteLine("[WARNING] Current schematic is null");
				mSchematic = new Schematic();
			}

			Console.WriteLine("[INFO] Count steps: " + mShaderData.Steps.Length);
			for (int index = 0; index < mShaderData.Steps.Length; index++)
			{
				Console.WriteLine("[INFO] Start parse shader for step : " + index);
				ShaderStep step = mShaderData.Steps[index];
				step.ValidateSettings();
				step.DisplayInfo();

				mSchematic = ShaderUtils.ApplyShader(mSchematic, step);
			}

			return mSchematic;
		}
	}
}
