﻿using System;
using System.Collections.Generic;
using FileToVox.Generator.Heightmap.Data;
using FileToVox.Schematics.Tools;
using FileToVox.Utils;

namespace FileToVox.Schematics
{
	public static class SchematicMerger
	{
		public static Schematic Merge(Schematic schematicA, Schematic schematicB, HeightmapStep heightmapStep)
		{
			switch (heightmapStep.PlacementMode)
			{
				case PlacementMode.ADDITIVE:
					return MergeAdditive(schematicA, schematicB);
				case PlacementMode.REPLACE:
					return MergeReplace(schematicA, schematicB);
				case PlacementMode.SUBSTRACT:
					return MergeSubstract(schematicA, schematicB);
				case PlacementMode.TOP_ONLY:
					return MergeTopOnly(schematicA, schematicB, heightmapStep);
				default:
					return MergeAdditive(schematicA, schematicB);
			}
		}

		private static Schematic MergeAdditive(Schematic schematicA, Schematic schematicB)
		{
			Console.WriteLine("[INFO] Start to merge schematic with additive mode");

			List<Voxel> allVoxels = schematicA.GetAllVoxels();
			Schematic resultSchematic = new Schematic(allVoxels);
			using (ProgressBar progressbar = new ProgressBar())
			{
				int index = 0;
				List<Voxel> allVoxelsB = schematicB.GetAllVoxels();
				foreach (var voxel in allVoxelsB)
				{
					resultSchematic.AddVoxel(voxel);
					progressbar.Report(index++ / (float)schematicB.TotalVoxels);
				}
			}

			Console.WriteLine("[INFO] Done");

			return resultSchematic;
		}

		private static Schematic MergeReplace(Schematic schematicA, Schematic schematicB)
		{
			Console.WriteLine("[INFO] Start to merge schematic with replace mode");
			List<Voxel> allVoxels = schematicA.GetAllVoxels();
			Schematic resultSchematic = new Schematic(allVoxels);
			using (ProgressBar progressbar = new ProgressBar())
			{
				int index = 0;
				List<Voxel> allVoxelsB = schematicB.GetAllVoxels();
				foreach (var voxel in allVoxelsB)
				{
					if (resultSchematic.GetColorAtVoxelIndex(voxel.X, voxel.Y, voxel.Z) != 0)
					{
						resultSchematic.ReplaceVoxel(voxel.X, voxel.Y, voxel.Z, voxel.Color);
					}
					progressbar.Report(index++ / (float)schematicB.TotalVoxels);
				}
			}

			Console.WriteLine("[INFO] Done");
			return resultSchematic;
		}

		private static Schematic MergeSubstract(Schematic schematicA, Schematic schematicB)
		{
			Console.WriteLine("[INFO] Start to merge schematic with subtract mode");

			List<Voxel> allVoxels = schematicA.GetAllVoxels();
			Schematic resultSchematic = new Schematic(allVoxels);
			using (ProgressBar progressbar = new ProgressBar())
			{
				int index = 0;
				List<Voxel> allVoxelsB = schematicB.GetAllVoxels();
				foreach (var voxel in allVoxelsB)
				{
					if (resultSchematic.GetColorAtVoxelIndex(voxel.X, voxel.Y, voxel.Z) != 0)
					{
						resultSchematic.RemoveVoxel(voxel.X, voxel.Y, voxel.Z);
					}
					progressbar.Report(index++ / (float)schematicB.TotalVoxels);

				}
			}

			Console.WriteLine("[INFO] Done");
			return resultSchematic;
		}

		private static Schematic MergeTopOnly(Schematic schematicA, Schematic schematicB, HeightmapStep step)
		{
			Console.WriteLine("[INFO] Start to merge schematic with top only mode");
			List<Voxel> allVoxels = schematicA.GetAllVoxels();
			Schematic resultSchematic = new Schematic(allVoxels);
			using (ProgressBar progressbar = new ProgressBar())
			{
				//int max = schematicA.Length * schematicA.Width;
				int max = schematicA.TotalVoxels + schematicB.TotalVoxels;
				int index = 0;

				Dictionary<int, List<int>> tops = new Dictionary<int, List<int>>();

				foreach (Voxel voxel in allVoxels)
				{
					int x = voxel.X;
					int y = voxel.Y;
					int z = voxel.Z;

					if (x == 0 || y == 0 || z == 0)
						continue;

					Vector3Int position;
					int index2d = Schematic.GetVoxelIndex2DFromRotation(x, y, z,step.RotationMode);

					switch (step.RotationMode)
					{
						case RotationMode.X:
							position = new Vector3Int(x + 1, y, z);
							break;
						case RotationMode.Y:
							position = new Vector3Int(x, y + 1, z);
							break;
						case RotationMode.Z:
							position = new Vector3Int(x, y, z + 1);
							break;
						default:
							position = new Vector3Int(x, y + 1, z);
							break;
					}
					if (schematicA.GetColorAtVoxelIndex(position) == 0)
					{
						if (!tops.ContainsKey(index2d))
						{
							tops[index2d] = new List<int>();
						}

						if (step.RotationMode == RotationMode.Y)
						{
							tops[index2d].Add(y);
						}
						else if (step.RotationMode == RotationMode.X)
						{
							tops[index2d].Add(x);
						}
						else if (step.RotationMode == RotationMode.Z)
						{
							tops[index2d].Add(z);
						}
					}

					progressbar.Report(index++ / (double)max);
				}

				List<Voxel> allVoxelsB = schematicB.GetAllVoxels();
				foreach (Voxel voxel in allVoxelsB)
				{
					int x = voxel.X;
					int y = voxel.Y;
					int z = voxel.Z;

					int index2d = Schematic.GetVoxelIndex2DFromRotation(x, y, z, step.RotationMode);

					if (tops.ContainsKey(index2d))
					{
						foreach (int maxHeight in tops[index2d])
						{
							switch (step.RotationMode)
							{
								case RotationMode.X:
									resultSchematic.AddVoxel(x + maxHeight + step.OffsetMerge, y, z, voxel.Color);
									break;
								case RotationMode.Y:
									resultSchematic.AddVoxel(x, y + maxHeight + step.OffsetMerge, z, voxel.Color);
									break;
								case RotationMode.Z:
									resultSchematic.AddVoxel(x, y, z + maxHeight + step.OffsetMerge, voxel.Color);
									break;
							}
						}
					}
					
					progressbar.Report(index++ / (double)max);

				}
			}

			Console.WriteLine("[INFO] Done");
			return resultSchematic;
		}
	}
}
