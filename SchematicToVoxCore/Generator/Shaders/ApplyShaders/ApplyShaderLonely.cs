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
			List<Voxel> allVoxels = schematic.GetAllVoxels();

			int index = 0;
			using (ProgressBar progressBar = new ProgressBar())
			{
				foreach (Voxel voxel in allVoxels)
				{
					int x = voxel.X;
					int y = voxel.Y;
					int z = voxel.Z;

					if (x == 0 || y == 0 || z == 0)
						continue;

					if (schematic.GetColorAtVoxelIndex(x - 1, y, z) == 0
					    && schematic.GetColorAtVoxelIndex(x + 1, y, z) == 0
					    && schematic.GetColorAtVoxelIndex(x, y - 1, z) == 0
					    && schematic.GetColorAtVoxelIndex(x, y + 1, z) == 0
					    && schematic.GetColorAtVoxelIndex(x, y, z - 1) == 0
					    && schematic.GetColorAtVoxelIndex(x, y, z + 1) == 0)
					{
						schematic.RemoveVoxel(x, y, z);
					}

					progressBar.Report(index++ / (float)allVoxels.Count);

				}
			}
			Console.WriteLine("[INFO] Done.");
			return schematic;
		}
	}
}
