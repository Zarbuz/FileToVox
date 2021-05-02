using FileToVox.Schematics;
using FileToVox.Utils;
using System;
using System.Collections.Generic;
using FileToVox.Generator.Shaders.Data;

namespace FileToVox.Generator.Shaders
{
	public class ApplyShaderCase : IShaderGenerator
	{
		private ShaderCase mShaderCase;

		public Schematic ApplyShader(Schematic schematic, ShaderStep shaderStep)
		{
			mShaderCase = shaderStep as ShaderCase;
			for (int i = 0; i < mShaderCase.Iterations; i++)
			{
				Console.WriteLine("[INFO] Process iteration: " + i);
				schematic = ProcessShaderCase(schematic, mShaderCase);
			}
			Console.WriteLine("[INFO] Done.");
			return schematic;
		}

		private Schematic ProcessShaderCase(Schematic schematic, ShaderCase shaderCase)
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
									if (shaderCase.TargetColorIndex != -1 && voxel.PalettePosition == shaderCase.TargetColorIndex || shaderCase.TargetColorIndex == -1)
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
