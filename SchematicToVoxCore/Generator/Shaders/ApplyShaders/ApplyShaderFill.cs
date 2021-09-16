using System;
using System.Collections.Generic;
using FileToVoxCommon.Generator.Shaders.Data;
using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;

namespace FileToVox.Generator.Shaders.ApplyShaders
{
	public class ApplyShaderFill : IShaderGenerator
	{
		public Schematic ApplyShader(Schematic schematic, ShaderStep shaderStep)
		{
			ShaderFill shaderFill = shaderStep as ShaderFill;
			if (shaderFill.TargetColorIndex == -1)
			{
				schematic = ProcessSchematicInDeleteMode(schematic, shaderFill);
			}
			else
			{
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
			}

			return schematic;
		}

		private Schematic ProcessSchematicInDeleteMode(Schematic schematic, ShaderFill shaderFill)
		{
			List<Voxel> allVoxels = schematic.GetAllVoxels();

			using (ProgressBar progressBar = new ProgressBar())
			{
				int index = 0;
				foreach (Voxel voxel in allVoxels)
				{
					int x = voxel.X;
					int y = voxel.Y;
					int z = voxel.Z;

					bool shouldDelete = false;
					switch (shaderFill.RotationMode)
					{
						case RotationMode.X:
							shouldDelete = shaderFill.FillDirection == FillDirection.PLUS ? x >= shaderFill.Limit : x <= shaderFill.Limit;
							break;
						case RotationMode.Y:
							shouldDelete = shaderFill.FillDirection == FillDirection.PLUS ? y >= shaderFill.Limit : y <= shaderFill.Limit;
							break;
						case RotationMode.Z:
							shouldDelete = shaderFill.FillDirection == FillDirection.PLUS ? z >= shaderFill.Limit : z <= shaderFill.Limit;
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}

					if (shouldDelete)
					{
						schematic.RemoveVoxel(x, y, z);
					}

					progressBar.Report(index++ / (float)allVoxels.Count);
				}
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
							schematic.AddVoxel(x, y, z, color, shaderFill.Replace);
						}
					}
					else
					{
						for (int x = min; x >= 0; x--)
						{
							schematic.AddVoxel(x, y, z, color, shaderFill.Replace);
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
							schematic.AddVoxel(x, y, z, color, shaderFill.Replace);
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
							schematic.AddVoxel(x, y, z, color, shaderFill.Replace);
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
							schematic.AddVoxel(x, y, z, color, shaderFill.Replace);
						}
					}
				}
				else
				{
					for (int z = min; z >= 0; z--)
					{
						for (int x = 0; x < schematic.Width; x++)
						{
							schematic.AddVoxel(x, y, z, color, shaderFill.Replace);
						}
					}
				}
			}


			return schematic;
		}
	}
}
