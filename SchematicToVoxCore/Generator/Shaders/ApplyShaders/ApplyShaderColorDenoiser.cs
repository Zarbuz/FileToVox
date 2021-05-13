using System;
using FileToVox.Schematics;
using FileToVox.Utils;
using System.Collections.Generic;

namespace FileToVox.Generator.Shaders.ApplyShaders
{
	public class ApplyShaderColorDenoiser : IShaderGenerator
	{
		public Schematic ApplyShader(Schematic schematic, ShaderStep shaderStep)
		{
			int colorChanged = 0;
			int index = 0;
			using (ProgressBar progressBar = new ProgressBar())
			{
				List<Voxel> voxels = schematic.GetAllVoxels();
				foreach (Voxel voxel in voxels)
				{
					int x = voxel.X;
					int y = voxel.Y;
					int z = voxel.Z;

					uint left = schematic.GetColorAtVoxelIndex(x - 1, y, z);
					uint right = schematic.GetColorAtVoxelIndex(x + 1, y, z);

					uint top = schematic.GetColorAtVoxelIndex(x, y + 1, z);
					uint bottom = schematic.GetColorAtVoxelIndex(x, y - 1, z);

					uint front = schematic.GetColorAtVoxelIndex(x, y, z + 1);
					uint back = schematic.GetColorAtVoxelIndex(x, y, z - 1);
					progressBar.Report(index++ / (float)voxels.Count);


					if (left == right && left == front && left == back && left != 0 && voxel.Color != left)
					{
						schematic.ReplaceVoxel(voxel, left);
						colorChanged++;
						continue;
					}

					if (left == right && left == top && left == bottom && left != 0 && voxel.Color != left)
					{
						schematic.ReplaceVoxel(voxel, left);
						colorChanged++;
						continue;
					}

					if (front == back && front == top && front == bottom && front != 0 && voxel.Color != front)
					{
						schematic.ReplaceVoxel(voxel, front);
						colorChanged++;
						continue;
					}
				}
			}

			Console.WriteLine("[INFO] Color changed: " + colorChanged);
			Console.WriteLine("[INFO] Done.");
			return schematic;
		}
	}
}
