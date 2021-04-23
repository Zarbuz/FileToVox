using System;
using System.Collections.Generic;
using System.Text;
using FileToVox.Schematics;
using FileToVox.Utils;

namespace FileToVox.Generator.Shaders
{
	public static partial class ShaderUtils
	{
		private static Schematic ApplyShaderLonely(Schematic schematic)
		{
			Schematic resultSchematic = new Schematic(schematic.BlockDict);

			int index = 0;
			using (ProgressBar progressBar = new ProgressBar())
			{
				foreach (KeyValuePair<ulong, Voxel> voxel in schematic.BlockDict)
				{
					int x = voxel.Value.X;
					int y = voxel.Value.Y;
					int z = voxel.Value.Z;

					if (x == 0 || y == 0 || z == 0)
						continue;

					if (schematic.GetColorAtVoxelIndex(x - 1, y, z) == 0
					    && schematic.GetColorAtVoxelIndex(x + 1, y, z) == 0
					    && schematic.GetColorAtVoxelIndex(x, y - 1, z) == 0
					    && schematic.GetColorAtVoxelIndex(x, y + 1, z) == 0
					    && schematic.GetColorAtVoxelIndex(x, y, z - 1) == 0
					    && schematic.GetColorAtVoxelIndex(x, y, z + 1) == 0)
					{
						resultSchematic.RemoveVoxel(x, y, z);
					}

					progressBar.Report(index++ / (float)schematic.BlockDict.Count);

				}
			}
			Console.WriteLine("[INFO] Done.");
			return resultSchematic;
		}
	}
}
