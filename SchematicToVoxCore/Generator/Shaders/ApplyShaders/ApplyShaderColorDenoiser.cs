using System;
using System.Collections.Generic;
using System.Linq;
using FileToVoxCommon.Generator.Shaders.Data;
using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;

namespace FileToVox.Generator.Shaders.ApplyShaders
{
	public class ApplyShaderColorDenoiser : IShaderGenerator
	{
		private bool mShouldBreak;
		public Schematic ApplyShader(Schematic schematic, ShaderStep shaderStep)
		{
			ShaderColorDenoiser shaderColorDenoiser = shaderStep as ShaderColorDenoiser;
			for (int i = 0; i < shaderColorDenoiser.Iterations; i++)
			{
				Console.WriteLine("[INFO] Process iteration: " + i);
				if (shaderColorDenoiser.StrictMode)
				{
					schematic = ProcessShaderColorDenoiserWithStrictMode(schematic);
				}
				else
				{
					schematic = ProcessShaderColorDenoiserWithColorRange(schematic, shaderColorDenoiser.ColorRange);
				}
				if (mShouldBreak)
				{
					break;
				}
			}

			return schematic;
		}

		private Schematic ProcessShaderColorDenoiserWithStrictMode(Schematic schematic)
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

			if (colorChanged == 0)
			{
				mShouldBreak = true;
				Console.WriteLine("[INFO] NO COLORS CHANGED, BREAK");
			}

			Console.WriteLine("[INFO] Color changed: " + colorChanged);
			Console.WriteLine("[INFO] Done.");
			return schematic;
		}

		private Schematic ProcessShaderColorDenoiserWithColorRange(Schematic schematic, int colorRange)
		{
			int colorChanged = 0;
			int index = 0;
			using (ProgressBar progressBar = new ProgressBar())
			{
				List<Voxel> voxels = schematic.GetAllVoxels();
				foreach (Voxel voxel in voxels)
				{
					progressBar.Report(index++ / (float)voxels.Count);

					int x = voxel.X;
					int y = voxel.Y;
					int z = voxel.Z;

					Voxel left = null;
					Voxel right= null;
					Voxel top = null;
					Voxel bottom= null;
					Voxel front= null;
					Voxel back= null;

					if (schematic.GetVoxel(x - 1, y, z, out Voxel v))
					{
						left = v;
					}

					if (schematic.GetVoxel(x + 1, y, z, out v))
					{
						right = v;
					}

					if (schematic.GetVoxel(x, y + 1, z, out v))
					{
						top = v;
					}

					if (schematic.GetVoxel(x, y - 1, z, out v))
					{
						bottom = v;
					}

					if (schematic.GetVoxel(x, y, z + 1, out v))
					{
						front = v;
					}

					if (schematic.GetVoxel(x, y, z - 1, out v))
					{
						back = v;
					}

					List<Voxel> list = new List<Voxel>() {right, left, top, bottom, front, back};
					list = list.Where(v => v != null).ToList();
					if (DistanceAverage(schematic, voxel, list) <= colorRange)
					{
						schematic.ReplaceVoxel(voxel, GetDominantColor(list));
						colorChanged++;
					}

					
				}
			}

			if (colorChanged == 0)
			{
				mShouldBreak = true;
				Console.WriteLine("[INFO] NO COLORS CHANGED, BREAK");
			}

			Console.WriteLine("[INFO] Color changed: " + colorChanged);
			Console.WriteLine("[INFO] Done.");
			return schematic;
		}

		private uint GetDominantColor(List<Voxel> voxels)
		{
			Dictionary<uint, int> mostUsedColors = new Dictionary<uint, int>();
			foreach (Voxel voxel in voxels)
			{
				if (!mostUsedColors.ContainsKey(voxel.Color))
				{
					mostUsedColors.Add(voxel.Color, 0);
				}
				mostUsedColors[voxel.Color]++;
			}

			return mostUsedColors.OrderByDescending(t => t.Value).First().Key;
		}

		private float DistanceAverage(Schematic schematic, Voxel currentVoxel, List<Voxel> voxels)
		{
			float sum = voxels.Where(v => v != null).Aggregate<Voxel, float>(0, (current, voxel) => current + Distance(schematic.GetPaletteIndex(currentVoxel.Color), schematic.GetPaletteIndex(voxel.Color)));
			sum /= voxels.Count(v => v != null);
			return sum;
		}

		private int Distance(int currentVoxelColorIndex, int targetVoxelColorIndex)
		{
			return Math.Abs(currentVoxelColorIndex - targetVoxelColorIndex);
		}
	}
}
