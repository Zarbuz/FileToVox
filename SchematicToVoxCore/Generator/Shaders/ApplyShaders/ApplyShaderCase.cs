using FileToVox.Schematics;
using FileToVox.Utils;
using System;
using System.Collections.Generic;

namespace FileToVox.Generator.Shaders
{
	public static partial class ShaderUtils
	{
		private static Schematic ApplyShaderCase(Schematic schematic, ShaderStep shaderStep)
		{
			for (int i = 0; i < shaderStep.Iterations; i++)
			{
				Console.WriteLine("[INFO] Process iteration: " + i);
				schematic = ProcessShaderCase(schematic);
			}
			Console.WriteLine("[INFO] Done.");
			return schematic;
		}

		private static Schematic ProcessShaderCase(Schematic schematic)
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

					if (x == 0 || y == 0 || z == 0)
						continue;

					for (int minX = x - 1; minX < x + 1; minX++)
					{
						for (int minY = y - 1; minY < y + 1; minY++)
						{
							for (int minZ = z - 1; minZ < z + 1; minZ++)
							{
								if (!schematic.ContainsVoxel(minX, minY, minZ))
								{
									if (mShaderStep.TargetColorIndex != -1 && voxel.PalettePosition == mShaderStep.TargetColorIndex || mShaderStep.TargetColorIndex == -1)
									{
										schematic.AddVoxel(minX, minY, minZ, voxel.Color);
									}
								}
							}
						}
					}

					progressBar.Report(index++ / (float) allVoxels.Count);
				}
			}

			return schematic;
		}
	}
}
