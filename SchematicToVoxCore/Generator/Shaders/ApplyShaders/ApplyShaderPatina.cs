using FileToVox.Schematics;
using FileToVox.Utils;
using System;
using System.Collections.Generic;

namespace FileToVox.Generator.Shaders
{
	public static partial class ShaderUtils
	{
		private static ShaderStep mShaderStep;

		private static Schematic ApplyShaderPatina(Schematic schematic, ShaderStep shaderStep)
		{
			mShaderStep = shaderStep;
			List<Voxel> allVoxels = schematic.GetAllVoxels();
			using (ProgressBar progressBar = new ProgressBar())
			{
				int index = 0;

				foreach (Voxel voxel in allVoxels)
				{
					if (CanGrow(schematic, voxel))
					{
						schematic.ReplaceVoxel(voxel, GetCrowColor(schematic, voxel));
					}

					progressBar.Report(index++ / (float)allVoxels.Count);
				}
			}

			Console.WriteLine("[INFO] Done.");
			return schematic;
		}

		private static uint GetCrowColor(Schematic schematic, Voxel voxel)
		{
			float distance = MinDistanceToWall(schematic, voxel, mShaderStep.Thickness);
			return schematic.GetColorAtPaletteIndex((int)(mShaderStep.TargetColorIndex +
														   mShaderStep.AdditionalColorRange *
														   (distance / MathF.Sqrt(MathF.Pow(mShaderStep.Thickness, 2) *
															   3))));
		}

		private static bool CanGrow(Schematic schematic, Voxel voxel)
		{
			int neighbors = GetNeighbors(schematic, voxel);
			float r = Random(voxel, mShaderStep.Seed + neighbors);

			if (HasWallNextToIt(schematic, voxel, 1))
			{
				Voxel foundVoxel;
				if (schematic.GetVoxel(voxel.X, voxel.Y - 1, voxel.Z, out foundVoxel))
				{
					if (IsGrowColor(foundVoxel) && MathUtils.Mod(voxel.X + voxel.Y, 13) == 0)
					{
						return true;
					}
				}

				if (schematic.GetVoxel(voxel.X, voxel.Y + 1, voxel.Z, out foundVoxel))
				{
					if (IsGrowColor(foundVoxel) && MathUtils.Mod(voxel.X + voxel.Y, 27) == 0)
					{
						return true;
					}
				}


				if (schematic.GetVoxel(voxel.X, voxel.Y - 2, voxel.Z, out foundVoxel))
				{
					if (IsGrowColor(foundVoxel) && MathUtils.Mod(voxel.X + voxel.Y, 13) == 0)
					{
						return true;
					}
				}

				if (schematic.GetVoxel(voxel.X, voxel.Y + 2, voxel.Z, out foundVoxel))
				{
					if (IsGrowColor(foundVoxel) && MathUtils.Mod(voxel.X + voxel.Y, 27) == 0)
					{
						return true;
					}
				}

				if (GetFlatNeighbors(schematic, voxel) > 3)
				{
					return true;
				}

				//if (r < mShaderStep.Density -
				//	(MinDistanceToWall(schematic, voxel, mShaderStep.Thickness) / mShaderStep.Thickness) *
				//	(mShaderStep.Density / 2)
				//	&& HasWallNextToIt(schematic, voxel, mShaderStep.Thickness) && neighbors > 0 &&
				//	(GrowsDown(schematic, voxel) || Random(voxel, mShaderStep.Seed + 5.567f) < 0.3f))
				//{
				//	return true;
				//}
			}

			return false;

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

		private static bool GrowsDown(Schematic schematic, Voxel voxel)
		{
			if (schematic.GetVoxel(voxel.X, voxel.Y + 1, voxel.Z, out Voxel foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
			{
				return true;
			}
			}

			if (schematic.GetVoxel(voxel.X + 1, voxel.Y, voxel.Z, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
			{
				return true;
			}
			}

			if (schematic.GetVoxel(voxel.X - 1, voxel.Y, voxel.Z, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
			{
				return true;
			}
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y, voxel.Z - 1, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
			{
				return true;
			}
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y, voxel.Z + 1, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
			{
				return true;
			}
			}

			return false;
		}

		private static int GetFlatNeighbors(Schematic schematic, Voxel voxel)
		{
			int neighbors = 0;

			Voxel foundVoxel;


			if (schematic.GetVoxel(voxel.X + 1, voxel.Y, voxel.Z + 1, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			if (schematic.GetVoxel(voxel.X + 1, voxel.Y, voxel.Z - 1, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			if (schematic.GetVoxel(voxel.X - 1, voxel.Y, voxel.Z + 1, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			if (schematic.GetVoxel(voxel.X - 1, voxel.Y, voxel.Z - 1, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			if (schematic.GetVoxel(voxel.X + 1, voxel.Y, voxel.Z, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			if (schematic.GetVoxel(voxel.X - 1, voxel.Y, voxel.Z, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y, voxel.Z - 1, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y, voxel.Z + 1, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			return neighbors;
		}

		private static bool IsGrowColor(Voxel voxel)
		{
			return voxel.PalettePosition >= MathF.Min(mShaderStep.TargetColorIndex, mShaderStep.TargetColorIndex + mShaderStep.AdditionalColorRange) && voxel.PalettePosition <= MathF.Max(mShaderStep.TargetColorIndex, mShaderStep.TargetColorIndex + mShaderStep.AdditionalColorRange);
		}


		private static float Random(Voxel voxel, float seed)
		{
			float n = voxel.X*voxel.Y*voxel.Z + seed;
			return MathF.Abs(Fract(MathF.Sin((float)(1 / MathF.Tan(n) + seed * 1235.342))));
		}

		private static float Fract(float value)
		{
			return (float)(value - Math.Truncate(value));
		}

		private static int GetNeighbors(Schematic schematic, Voxel voxel)
		{
			int neighbors = 0;

			Voxel foundVoxel;

			if (schematic.GetVoxel(voxel.X + 1, voxel.Y + 1, voxel.Z, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y - 1, voxel.Z, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			if (schematic.GetVoxel(voxel.X + 1, voxel.Y, voxel.Z, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			if (schematic.GetVoxel(voxel.X - 1, voxel.Y, voxel.Z, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y, voxel.Z - 1, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}

			if (schematic.GetVoxel(voxel.X, voxel.Y, voxel.Z + 1, out foundVoxel))
			{
				if (IsGrowColor(foundVoxel))
				{
					neighbors++;
				}
			}


			return neighbors;
		}
	}
}
