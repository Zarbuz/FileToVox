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

			Schematic resultSchematic = new Schematic(schematicA.BlockDict);
			using (ProgressBar progressbar = new ProgressBar())
			{
				int index = 0;
				foreach (var voxel in schematicB.BlockDict)
				{
					resultSchematic.AddVoxel(voxel.Value);
					progressbar.Report(index++ / (float)schematicB.BlockDict.Count);
				}
			}

			Console.WriteLine("[INFO] Done");

			return resultSchematic;
		}

		private static Schematic MergeReplace(Schematic schematicA, Schematic schematicB)
		{
			Console.WriteLine("[INFO] Start to merge schematic with replace mode");

			Schematic resultSchematic = new Schematic(schematicA.BlockDict);
			using (ProgressBar progressbar = new ProgressBar())
			{
				int index = 0;
				foreach (var voxel in schematicB.BlockDict)
				{
					if (resultSchematic.GetColorAtVoxelIndex(voxel.Value.X, voxel.Value.Y, voxel.Value.Z) != 0)
					{
						resultSchematic.ReplaceVoxel(voxel.Value.X, voxel.Value.Y, voxel.Value.Z, voxel.Value.Color);
					}
					progressbar.Report(index++ / (float)schematicB.BlockDict.Count);
				}
			}

			Console.WriteLine("[INFO] Done");
			return resultSchematic;
		}

		private static Schematic MergeSubstract(Schematic schematicA, Schematic schematicB)
		{
			Console.WriteLine("[INFO] Start to merge schematic with subtract mode");

			Schematic resultSchematic = new Schematic(schematicA.BlockDict);
			using (ProgressBar progressbar = new ProgressBar())
			{
				int index = 0;
				foreach (var voxel in schematicB.BlockDict)
				{
					if (resultSchematic.GetColorAtVoxelIndex(voxel.Value.X, voxel.Value.Y, voxel.Value.Z) != 0)
					{
						resultSchematic.RemoveVoxel(voxel.Value.X, voxel.Value.Y, voxel.Value.Z);
					}
					progressbar.Report(index++ / (float)schematicB.BlockDict.Count);

				}
			}

			Console.WriteLine("[INFO] Done");
			return resultSchematic;
		}

		private static Schematic MergeTopOnly(Schematic schematicA, Schematic schematicB, HeightmapStep step)
		{
			Console.WriteLine("[INFO] Start to merge schematic with top only mode");

			Schematic resultSchematic = new Schematic(schematicA.BlockDict);
			using (ProgressBar progressbar = new ProgressBar())
			{
				//int max = schematicA.Length * schematicA.Width;
				int max = schematicA.BlockDict.Count + schematicB.BlockDict.Count;
				int index = 0;

				Dictionary<int, List<int>> tops = new Dictionary<int, List<int>>();
				foreach (KeyValuePair<ulong, Voxel> voxel in schematicA.BlockDict)
				{
					int x = voxel.Value.X;
					int y = voxel.Value.Y;
					int z = voxel.Value.Z;

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

				foreach (KeyValuePair<ulong, Voxel> voxel in schematicB.BlockDict)
				{
					int x = voxel.Value.X;
					int y = voxel.Value.Y;
					int z = voxel.Value.Z;

					int index2d = Schematic.GetVoxelIndex2DFromRotation(x, y, z, step.RotationMode);

					if (tops.ContainsKey(index2d))
					{
						foreach (int maxHeight in tops[index2d])
						{
							switch (step.RotationMode)
							{
								case RotationMode.X:
									resultSchematic.AddVoxel(x + maxHeight + step.OffsetMerge, y, z, voxel.Value.Color);
									break;
								case RotationMode.Y:
									resultSchematic.AddVoxel(x, y + maxHeight + step.OffsetMerge, z, voxel.Value.Color);
									break;
								case RotationMode.Z:
									resultSchematic.AddVoxel(x, y, z + maxHeight + step.OffsetMerge, voxel.Value.Color);
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
