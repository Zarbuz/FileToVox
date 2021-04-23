using System;
using System.Collections.Generic;
using System.Text;
using FileToVox.Schematics;
using FileToVox.Utils;

namespace FileToVox.Generator.Shaders
{
	public static partial class ShaderUtils
	{
		private static Schematic ApplyShaderFillHoles(Schematic schematic)
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

					uint left = schematic.GetColorAtVoxelIndex(x-1, y, z);
					uint right = schematic.GetColorAtVoxelIndex(x+1, y, z);

					uint front = schematic.GetColorAtVoxelIndex(x, z - 1, x);
					uint back = schematic.GetColorAtVoxelIndex(x, y, z + 1);

					uint top = schematic.GetColorAtVoxelIndex(x, y +1, z);
					uint bottom = schematic.GetColorAtVoxelIndex(x, y - 1, z);


					if (left != 0 && right != 0 && front != 0 && back != 0)
					{
						resultSchematic.AddVoxel(x, y, z, left);
					}

					if (top != 0 && bottom != 0 && left != 0 && right != 0)
					{
						resultSchematic.AddVoxel(x, y, z, top);
					}

					if (front != 0 && back != 0 && top != 0 && bottom != 0)
					{
						resultSchematic.AddVoxel(x, y, z, front);
					}

					progressBar.Report(index++ / (float)schematic.BlockDict.Count);
				}
			}

			Console.WriteLine("[INFO] Done.");
			return schematic;
		}

	}
}
