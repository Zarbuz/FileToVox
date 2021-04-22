﻿using System;
using System.Collections.Generic;
using System.Text;
using FileToVox.Generator.Shaders;
using FileToVox.Schematics;

namespace FileToVox.Utils
{
	public static class ShaderUtils
	{
		#region PublicMethods

		public static Schematic ApplyShader(Schematic schematic, ShaderStep shaderStep)
		{
			switch (shaderStep.ShaderType)
			{
				case ShaderType.FIX_HOLES:
					return ApplyShaderFillHoles(schematic);
				case ShaderType.FIX_LONELY:
					return ApplyShaderLonely(schematic);
				case ShaderType.CASE:
					return ApplyShaderCase(schematic, shaderStep.Iterations);
			}

			return schematic;
		}

		#endregion

		#region ShaderLonely

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
		#endregion

		#region ShaderFillHoles

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


		#endregion

		#region ShaderCase

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

		#endregion
	}
}
