using FileToVox.Schematics;
using FileToVox.Utils;
using System;
using System.Collections.Generic;

namespace FileToVox.Generator.Shaders
{
	public static partial class ShaderUtils
	{
		private static Schematic ApplyShaderCase(Schematic schematic, int iterations)
		{
			Schematic stepSchematic = schematic;
			for (int i = 0; i < iterations; i++)
			{
				Console.WriteLine("[INFO] Process iteration: " + i);
				stepSchematic = ProcessShaderCase(stepSchematic);
			}
			Console.WriteLine("[INFO] Done.");
			return stepSchematic;
		}

		private static Schematic ProcessShaderCase(Schematic schematic)
		{
			Schematic resultSchematic = new Schematic(schematic.BlockDict);

			using (ProgressBar progressBar = new ProgressBar())
			{
				int index = 0;
				foreach (KeyValuePair<ulong, Voxel> voxel in schematic.BlockDict)
				{
					int x = voxel.Value.X;
					int y = voxel.Value.Y;
					int z = voxel.Value.Z;

					if (x == 0 || y == 0 || z == 0)
						continue;

					for (int minX = x - 1; minX < x + 1; minX++)
					{
						for (int minY = y - 1; minY < y + 1; minY++)
						{
							for (int minZ = z - 1; minZ < z + 1; minZ++)
							{
								if (!schematic.GetVoxel(minX, minY, minZ, out _))
								{
									resultSchematic.AddVoxel(minX, minY, minZ, voxel.Value.Color);
								}
							}
						}
					}

					progressBar.Report(index++ / (float)schematic.BlockDict.Count);
				}
			}

			return resultSchematic;
		}
	}
}
