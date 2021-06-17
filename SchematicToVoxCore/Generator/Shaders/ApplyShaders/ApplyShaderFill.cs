using System;
using FileToVox.Schematics;
using FileToVoxCommon.Generator.Shaders.Data;

namespace FileToVox.Generator.Shaders.ApplyShaders
{
	public class ApplyShaderFill : IShaderGenerator
	{
		public Schematic ApplyShader(Schematic schematic, ShaderStep shaderStep)
		{
			ShaderFill shaderFill = shaderStep as ShaderFill;
			switch (shaderFill.RotationMode)
			{
				case RotationMode.X:
					schematic = ProcessSchematicInXAxis(schematic, shaderFill);
					break;
				case RotationMode.Y:
					schematic = ProcessSchematicInYAxis(schematic, shaderFill);
					break;
				case RotationMode.Z:
					schematic = ProcessSchematicInZAxis(schematic, shaderFill);
					break;
			}

			return schematic;
		}

		private Schematic ProcessSchematicInXAxis(Schematic schematic, ShaderFill shaderFill)
		{
			int min = shaderFill.Limit;
			uint color = schematic.GetColorAtPaletteIndex(shaderFill.TargetColorIndex);
			for (int y = 0; y < schematic.Height; y++)
			{
				for (int z = 0; z < schematic.Length; z++)
				{
					if (shaderFill.FillDirection == FillDirection.PLUS)
					{
						for (int x = min; x < schematic.Width; x++)
						{
							schematic.AddVoxelWithoutReplace(x, y, z, color);
						}
					}
					else
					{
						for (int x = min; x >= 0; x--)
						{
							schematic.AddVoxelWithoutReplace(x, y, z, color);
						}
					}
				}
			}

			return schematic;
		}

		private Schematic ProcessSchematicInYAxis(Schematic schematic, ShaderFill shaderFill)
		{
			int min = shaderFill.Limit;
			uint color = schematic.GetColorAtPaletteIndex(shaderFill.TargetColorIndex);
			if (shaderFill.FillDirection == FillDirection.PLUS)
			{
				for (int y = min; y < schematic.Height; y++)
				{
					for (int z = 0; z < schematic.Length; z++)
					{
						for (int x = 0; x < schematic.Width; x++)
						{
							schematic.AddVoxelWithoutReplace(x, y, z, color);
						}
					}

				}
			}
			else
			{
				for (int y = min; y >= 0; y--)
				{
					for (int z = 0; z < schematic.Length; z++)
					{
						for (int x = 0; x < schematic.Width; x++)
						{
							schematic.AddVoxelWithoutReplace(x, y, z, color);
						}
					}
				}
			}

			return schematic;
		}

		private Schematic ProcessSchematicInZAxis(Schematic schematic, ShaderFill shaderFill)
		{
			int min = shaderFill.Limit;
			uint color = schematic.GetColorAtPaletteIndex(shaderFill.TargetColorIndex);
			for (int y = 0; y < schematic.Height; y++)
			{
				if (shaderFill.FillDirection == FillDirection.PLUS)
				{
					for (int z = min; z < schematic.Length; z++)
					{
						for (int x = 0; x < schematic.Width; x++)
						{
							schematic.AddVoxelWithoutReplace(x, y, z, color);
						}
					}
				}
				else
				{
					for (int z = min; z >= 0; z--)
					{
						for (int x = 0; x < schematic.Width; x++)
						{
							schematic.AddVoxelWithoutReplace(x, y, z, color);
						}
					}
				}
			}


			return schematic;
		}
	}
}
