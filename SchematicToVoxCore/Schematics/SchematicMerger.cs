using System;
using System.Collections.Generic;
using FileToVox.Generator.Heightmap.Data;

namespace FileToVox.Schematics
{
	public static class SchematicMerger
	{
		public static Schematic Merge(Schematic schematicA, Schematic schematicB, PlacementMode placementMode)
		{
			switch (placementMode)
			{
				case PlacementMode.ADDITIVE:
					return MergeAdditive(schematicA, schematicB);
				case PlacementMode.REPLACE:
					return MergeReplace(schematicA, schematicB);
				case PlacementMode.SUBSTRACT:
					return MergeSubstract(schematicA, schematicB);
				//case PlacementMode.TOP_ONLY:
				//	return MergeTopOnly(schematicA, schematicB);
				default:
					return MergeAdditive(schematicA, schematicB);
			}
		}

		private static Schematic MergeAdditive(Schematic schematicA, Schematic schematicB)
		{
			Schematic resultSchematic = new Schematic(schematicA.BlockDict);
			foreach (KeyValuePair<ulong, Voxel> voxel in schematicB.BlockDict)
			{
				resultSchematic.AddVoxel(voxel.Value);
			}

			return resultSchematic;
		}

		private static Schematic MergeReplace(Schematic schematicA, Schematic schematicB)
		{
			Schematic resultSchematic = new Schematic(schematicA.BlockDict);
			foreach (KeyValuePair<ulong, Voxel> voxel in schematicB.BlockDict)
			{
				if (resultSchematic.GetColorAtVoxelIndex(voxel.Value.X, voxel.Value.Y, voxel.Value.Z) != 0)
				{
					resultSchematic.ReplaceVoxel(voxel.Value.X, voxel.Value.Y, voxel.Value.Z, voxel.Value.Color);
				}
			}

			return resultSchematic;
		}

		private static Schematic MergeSubstract(Schematic schematicA, Schematic schematicB)
		{
			Schematic resultSchematic = new Schematic(schematicA.BlockDict);
			foreach (KeyValuePair<ulong, Voxel> voxel in schematicB.BlockDict)
			{
				if (resultSchematic.GetColorAtVoxelIndex(voxel.Value.X, voxel.Value.Y, voxel.Value.Z) != 0)
				{
					resultSchematic.RemoveVoxel(voxel.Value.X, voxel.Value.Y, voxel.Value.Z);
				}
			}

			return resultSchematic;
		}

		//private static Schematic MergeTopOnly(Schematic schematicA, Schematic schematicB)
		//{
		//	Schematic resultSchematic = new Schematic(schematicA.BlockDict);
		//	foreach (KeyValuePair<ulong, Voxel> voxel in schematicB.BlockDict)
		//	{
		//		if (resultSchematic.GetColorAtVoxelIndex(voxel.Value.X, voxel.Value.Y, voxel.Value.Z) != 0)
		//		{
		//			resultSchematic.RemoveVoxel(voxel.Value.X, voxel.Value.Y, voxel.Value.Z);
		//		}
		//	}

		//	return resultSchematic;
		//}
	}
}
