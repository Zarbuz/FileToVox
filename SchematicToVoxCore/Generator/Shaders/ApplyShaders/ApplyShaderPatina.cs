using FileToVox.Schematics;
using FileToVox.Utils;
using System;
using System.Collections.Generic;
using FileToVoxCommon.Generator.Shaders.Data;

namespace FileToVox.Generator.Shaders
{
	// Based on Patina by @Patrick Seeber : https://github.com/patStar/voxelShader/blob/master/shader/patina.txt
	public class ApplyShaderPatina : IShaderGenerator
	{
		private bool mShouldBreak;
		private ShaderPatina mShaderPatina;

		public Schematic ApplyShader(Schematic schematic, ShaderStep shaderStep)
		{
			mShaderPatina = shaderStep as ShaderPatina;
			for (int i = 0; i < mShaderPatina.Iterations; i++)
			{
				Console.WriteLine("[INFO] Process iteration: " + i);
				schematic = ProcessShaderPatina(schematic);
				if (mShouldBreak)
				{
					break;
				}
			}

			Console.WriteLine("[INFO] Done.");
			return schematic;
		}


		private Schematic ProcessShaderPatina(Schematic schematic)
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
						if (voxel.Color != newColor)
						{
							schematic.ReplaceVoxel(voxel, newColor);
							colorChanged++;
						}
					}

					progressBar.Report(index++ / (float)(allVoxels.Count));
				}
				Console.WriteLine("COLOR CHANGED: " + colorChanged);

				if (colorChanged == 0)
				{
					mShouldBreak = true;
					Console.WriteLine("[INFO] NO COLORS CHANGED, BREAK");
				}
			}

			return schematic;
		}

		private bool IsGrowColor(Voxel voxel)
		{
			return voxel.PalettePosition >= MathF.Min(mShaderPatina.TargetColorIndex, mShaderPatina.TargetColorIndex + mShaderPatina.AdditionalColorRange) && voxel.PalettePosition <= MathF.Max(mShaderPatina.TargetColorIndex, mShaderPatina.TargetColorIndex + mShaderPatina.AdditionalColorRange);
		}

		private float Random(Voxel voxel, float seed)
		{
			int x = voxel.X + 1;
			int y = voxel.Y + 1;
			int z = voxel.Z + 1;
			float n = x * y * z + seed;
			return MathF.Abs(Fract(MathF.Sin((1 / MathF.Tan(n) + seed * 1235.342f))));
		}

		private float Fract(float value)
		{
			float result = value - MathF.Floor(value);
			return result;
		}

		private float MinDistanceToWall(Schematic schematic, Voxel voxel, int distance)
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

		private uint GetCrowColor(Schematic schematic, Voxel voxel)
		{
			float distance = MinDistanceToWall(schematic, voxel, mShaderPatina.Thickness);
			float index = mShaderPatina.TargetColorIndex + mShaderPatina.AdditionalColorRange * (distance / MathF.Sqrt(MathF.Pow(mShaderPatina.Thickness, 2) * 3));
			return schematic.GetColorAtPaletteIndex((int)index);
		}
		private bool HasWallNextToIt(Schematic schematic, Voxel voxel, int distance)
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

		private int GetFlatNeighbors(Schematic schematic, Voxel voxel)
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

			if (schematic.GetVoxel(voxel.X, voxel.Y - 1, voxel.Z, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y + 1, voxel.Z, out foundVoxel) && IsGrowColor(foundVoxel))
			{
				neighbors++;
			}

			return neighbors;
		}

		private int GetNeighbors(Schematic schematic, Voxel voxel)
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

		private bool GrowsDown(Schematic schematic, Voxel voxel)
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

		private bool Grows(Schematic schematic, Voxel voxel)
		{
			int neighbors = GetNeighbors(schematic, voxel);
			float r = Random(voxel, mShaderPatina.Seed + neighbors);

			if (HasWallNextToIt(schematic, voxel, 1))
			{
				int x = voxel.X + 1;
				int z = voxel.Z + 1;
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

			if (r < mShaderPatina.Density - MinDistanceToWall(schematic, voxel, mShaderPatina.Thickness) / mShaderPatina.Thickness * (mShaderPatina.Density / 2)
				&& HasWallNextToIt(schematic, voxel, mShaderPatina.Thickness)
				&& neighbors > 0
				&& (GrowsDown(schematic, voxel) || Random(voxel, mShaderPatina.Seed + 5.567f) < 0.3f))
			{
				return true;
			}

			return false;

		}


	}
}
