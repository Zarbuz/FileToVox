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
				int max = schematicA.Length * schematicA.Width;
				int index = 0;
				for (int z = 0; z < schematicA.Length; z++)
				{
					for (int x = 0; x < schematicA.Width; x++)
					{
						int maxHeight = 0;
						for (int y = schematicA.Height - 1; y >= 0; y--)
						{
							if (schematicA.GetColorAtVoxelIndex(x, y, z) != 0)
							{
								maxHeight = y;
								break;
							}
						}

						if (maxHeight != 0)
						{
							for (int y = 0; y < schematicB.Height; y++)
							{
								uint color = schematicB.GetColorAtVoxelIndex(x, y, z);
								if (color != 0)
								{
									resultSchematic.AddVoxel(x, y + maxHeight + heightmapStep.OffsetMerge, z, color);
								}
							}
						}

						progressbar.Report(index++ / (double)max);

					}
				}
			}

			Console.WriteLine("[INFO] Done");
			return resultSchematic;
		}
	}
}
