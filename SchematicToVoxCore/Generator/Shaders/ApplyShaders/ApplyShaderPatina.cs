using FileToVox.Schematics;
using FileToVox.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Generator.Shaders
{
	public static partial class ShaderUtils
	{
		private static Schematic ApplyShaderPatina(Schematic schematic, ShaderStep shaderStep)
		{
			for (int i = 0; i < shaderStep.Iterations; i++)
			{
				Console.WriteLine("[INFO] Process iteration: " + i);
				schematic = ProcessShaderPatina(schematic);
			}

			Console.WriteLine("[INFO] Done.");
			return schematic;
		}

		private static Schematic ProcessShaderPatina(Schematic schematic)
		{
			using (ProgressBar progressBar = new ProgressBar())
			{
				int index = 0;
				List<Voxel> allVoxels = schematic.GetAllVoxels();
				int colorChanged = 0;
				foreach (Voxel voxel in allVoxels)
				{
					if (Grows(schematic, voxel))
					{
						uint newColor = GetCrowColor(schematic, voxel);
						schematic.ReplaceVoxel(voxel, newColor);
						colorChanged++;
					}

					progressBar.Report(index++ / (float)(allVoxels.Count));
				}
				Console.WriteLine("COLOR CHANGED: " + colorChanged);
			}

			return schematic;
		}

		private static bool IsGrowColor(Voxel voxel)
		{
			return voxel.PalettePosition >= MathF.Min(mShaderStep.TargetColorIndex, mShaderStep.TargetColorIndex + mShaderStep.AdditionalColorRange) && voxel.PalettePosition <= MathF.Max(mShaderStep.TargetColorIndex, mShaderStep.TargetColorIndex + mShaderStep.AdditionalColorRange);
		}

		private static float Random(Voxel voxel, float seed)
		{
			int x = voxel.X / Program.CHUNK_SIZE ;
			int y = voxel.Y / Program.CHUNK_SIZE ;
			int z = voxel.Z / Program.CHUNK_SIZE ;
			float n = x * y * z + seed;
			return MathF.Abs(Fract(MathF.Sin((1 / MathF.Tan(n) + seed * 1235.342f))));
		}

		private static float Fract(float value)
		{
			float result = value - MathF.Floor(value);
			return result;
		}

		private static float MinDistanceToWall(Schematic schematic, Voxel voxel, int distance)
		{
			float minDistance = distance + 1;

			for (int x = -distance; x <= distance; x++)
			{
				for (int y = -distance; y <= distance; y++)
				{
					for (int z = -distance; z <= distance; z++)
					{
						if (schematic.GetVoxel(voxel.X + x, voxel.Y + y, voxel.Z + z, out Voxel foundVoxel))
						{
							if (!IsGrowColor(foundVoxel))
							{
								float d = MathF.Sqrt(MathF.Pow(x, 2) + MathF.Pow(y, 2) + MathF.Pow(z, 2));
								if (minDistance > d)
								{
									minDistance = d;
								}
							}
						}
					}
				}
			}

			return minDistance;
		}

		private static uint GetCrowColor(Schematic schematic, Voxel voxel)
		{
			float distance = MinDistanceToWall(schematic, voxel, mShaderStep.Thickness);
			float index = mShaderStep.TargetColorIndex + mShaderStep.AdditionalColorRange * (distance / MathF.Sqrt(MathF.Pow(mShaderStep.Thickness, 2) * 3));
			return schematic.GetColorAtPaletteIndex((int)index);
		}
		private static bool HasWallNextToIt(Schematic schematic, Voxel voxel, int distance)
		{
			for (int x = -distance; x <= distance; x++)
			{
				for (int y = -distance; y <= distance; y++)
				{
					for (int z = -distance; z <= distance; z++)
					{
						if (schematic.GetVoxel(voxel.X + x, voxel.Y + y, voxel.Z + z, out Voxel foundVoxel))
						{
							if (!IsGrowColor(foundVoxel))
							{
								return true;
							}
						}
					}
				}
			}

			return false;
		}

		private static int GetFlatNeighbors(Schematic schematic, Voxel voxel)
		{
			int neighbors = 0;

			if (schematic.GetVoxel(voxel.X + 1, voxel.Y, voxel.Z + 1, out Voxel foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X + 1, voxel.Y, voxel.Z - 1, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X - 1, voxel.Y, voxel.Z + 1, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X - 1, voxel.Y, voxel.Z - 1, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X + 1, voxel.Y, voxel.Z, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X - 1, voxel.Y, voxel.Z, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y, voxel.Z - 1, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y, voxel.Z + 1, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			return neighbors;
		}

		private static int GetNeighbors(Schematic schematic, Voxel voxel)
		{
			int neighbors = 0;

			if (schematic.GetVoxel(voxel.X, voxel.Y + 1, voxel.Z, out Voxel foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y - 1, voxel.Z, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X + 1, voxel.Y, voxel.Z, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X - 1, voxel.Y, voxel.Z, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y, voxel.Z - 1, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y, voxel.Z + 1, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			return neighbors;
		}

		private static bool GrowsDown(Schematic schematic, Voxel voxel)
		{
			if (schematic.GetVoxel(voxel.X, voxel.Y + 1, voxel.Z, out Voxel foundVoxel) && IsGrowColor(foundVoxel))
			{
				return true;
			}

			if (schematic.GetVoxel(voxel.X + 1, voxel.Y, voxel.Z, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				return true;
			}

			if (schematic.GetVoxel(voxel.X - 1, voxel.Y, voxel.Z, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				return true;
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y, voxel.Z - 1, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				return true;
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y, voxel.Z + 1, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				return true;
			}

			return false;
		}

		private static bool Grows(Schematic schematic, Voxel voxel)
		{
			int neighbors = GetNeighbors(schematic, voxel);
			float r = Random(voxel, mShaderStep.Seed + neighbors);

			if (HasWallNextToIt(schematic, voxel, 1))
			{
				int x = voxel.X / Program.CHUNK_SIZE;
				int z = voxel.Z / Program.CHUNK_SIZE;
				if (schematic.GetVoxel(voxel.X, voxel.Y - 1, voxel.Z, out Voxel foundVoxel) && IsGrowColor(foundVoxel) && (x + z) % 13 == 0)
				{
					return true;
				}

				if (schematic.GetVoxel(voxel.X, voxel.Y + 1, voxel.Z, out foundVoxel) && IsGrowColor(foundVoxel) && (x + z) % 27 == 0)
				{
					return true;
				}

				if (schematic.GetVoxel(voxel.X, voxel.Y - 2, voxel.Z, out foundVoxel) && IsGrowColor(foundVoxel) && (x + z) % 13 == 0)
				{
					return true;
				}

				if (schematic.GetVoxel(voxel.X, voxel.Y + 2, voxel.Z, out foundVoxel) && IsGrowColor(foundVoxel) && (x + z) % 27 == 0)
				{
					return true;
				}

				if (GetFlatNeighbors(schematic, voxel) > 3)
				{
					return true;
				}
			}

			if (r < mShaderStep.Density - (MinDistanceToWall(schematic, voxel, mShaderStep.Thickness) / mShaderStep.Thickness) * (mShaderStep.Density / 2)
				&& HasWallNextToIt(schematic, voxel, mShaderStep.Thickness)
				&& neighbors > 0
				&& (GrowsDown(schematic, voxel) || Random(voxel, mShaderStep.Seed + 5.567f) < 0.3f))
			{
				return true;
			}

			return false;

		}




	}
}
