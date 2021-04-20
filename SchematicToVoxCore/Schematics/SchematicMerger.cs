using System;
using System.Collections.Generic;
using FileToVox.Generator.Heightmap.Data;
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
					progressbar.Report(index++/ (float)schematicB.BlockDict.Count);

				}
			}
		
			Console.WriteLine("[INFO] Done");
			return resultSchematic;
		}

		private static Schematic MergeTopOnly(Schematic schematicA, Schematic schematicB, HeightmapStep heightmapStep)
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

					if (schematicA.GetColorAtVoxelIndex(x, y + 1, z) == 0)
					{
						int index2d = Schematic.GetVoxelIndex2D(x, z);
						if (!tops.ContainsKey(index2d))
						{
							tops[index2d] = new List<int>();
						}

						tops[index2d].Add(y);
					}

					progressbar.Report(index++ / (double)max);
				}

				foreach (KeyValuePair<ulong, Voxel> voxel in schematicB.BlockDict)
				{
					int x = voxel.Value.X;
					int y = voxel.Value.Y;
					int z = voxel.Value.Z;

					int index2d = Schematic.GetVoxelIndex2D(x, z);
					foreach (int maxHeight in tops[index2d])
					{
						resultSchematic.AddVoxel(x, y + maxHeight + heightmapStep.OffsetMerge, z, voxel.Value.Color);
					}
					progressbar.Report(index++ / (double)max);

				}
			}

			Console.WriteLine("[INFO] Done");
			return resultSchematic;
		}
	}
}
