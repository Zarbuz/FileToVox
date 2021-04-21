using System;
using System.Collections.Generic;
using System.Text;
using FileToVox.Schematics;

namespace FileToVox.Utils
{
	public static class ShaderUtils
	{
		#region PublicMethods

		public static Schematic FixLonelyVoxels(Schematic schematic)
		{
			Console.WriteLine("[LOG] Started to delete lonely voxels...");
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
						schematic.RemoveVoxel(x, y, z);
					}

					progressBar.Report(index++ / (float)schematic.BlockDict.Count);

				}
			}
			Console.WriteLine("[LOG] Done.");
			return schematic;
		}

		public static Schematic FillHoles(Schematic schematic)
		{
			Console.WriteLine("[LOG] Started to fill holes...");
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

					schematic = Check1X1X1Hole(schematic, x, y, z);
					schematic = Check1X2X1Hole(schematic, x, y, z);
					schematic = Check2X1X1Hole(schematic, x, y, z);
					schematic = Check1X1X2Hole(schematic, x, y, z);

					progressBar.Report(index++ / (float)schematic.BlockDict.Count);
				}
			}

			Console.WriteLine("[LOG] Done.");
			return schematic;
		}

		#endregion

		#region PrivateMethods

		/// <summary>
		/// .X.
		/// X0X
		/// .X.
		/// </summary>
		/// <param name="schematic"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="blocks"></param>
		/// <returns></returns>
		private static Schematic Check1X1X1Hole(Schematic schematic, int x, int y, int z)
		{
			uint left = schematic.GetColorAtVoxelIndex(x-1, y, z);
			uint right = schematic.GetColorAtVoxelIndex(x+1, y, z);

			uint front = schematic.GetColorAtVoxelIndex(x, z - 1, x);
			uint back = schematic.GetColorAtVoxelIndex(x, y, z + 1);

			uint top = schematic.GetColorAtVoxelIndex(x, y +1, z);
			uint bottom = schematic.GetColorAtVoxelIndex(x, y - 1, z);


			if (left != 0 && right != 0 && front != 0 && back != 0)
			{
				schematic.AddVoxel(x, y, z, left);
			}

			if (top != 0 && bottom != 0 && left != 0 && right != 0)
			{
				schematic.AddVoxel(x, y, z, top);
			}

			if (front != 0 && back != 0 && top != 0 && bottom != 0)
			{
				schematic.AddVoxel(x, y, z, front);
			}

			return schematic;
		}

		/// <summary>
		/// X0X
		/// X0X
		/// .X.
		/// </summary>
		/// <param name="blocks"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		private static Schematic Check1X2X1Hole(Schematic schematic, int x, int y, int z)
		{
			uint left = schematic.GetColorAtVoxelIndex(x-1, y, z);
			uint right = schematic.GetColorAtVoxelIndex(x+1, y, z);

			uint top = schematic.GetColorAtVoxelIndex(x, y +1, z);
			uint bottom = schematic.GetColorAtVoxelIndex(x, y - 1, z);

			uint front = schematic.GetColorAtVoxelIndex(x, z - 1, x);
			uint back = schematic.GetColorAtVoxelIndex(x, y, z + 1);

			uint diagonalLeft = schematic.GetColorAtVoxelIndex(x -1, y - 1, z);
			uint diagonalRight = schematic.GetColorAtVoxelIndex(x +1, y - 1, z);

			uint diagonalLeft2 = schematic.GetColorAtVoxelIndex(x, y - 1, z + 1);
			uint diagonalRight2 = schematic.GetColorAtVoxelIndex(x, y - 1, z - 1);


			if (bottom == 0 && top != 0 && right != 0 && left != 0 && diagonalRight != 0 && diagonalLeft != 0)
			{
				schematic.AddVoxel(x, y, z, top);
			}

			if (bottom == 0 && top != 0 && front != 0 && back != 0 && diagonalRight2 != 0 && diagonalLeft2 != 0)
			{
				schematic.AddVoxel(x, y, z, top);
			}

			return schematic;
		}


		/// <summary>
		/// .XX
		/// X00
		/// .XX
		/// </summary>
		/// <param name="blocks"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		private static Schematic Check2X1X1Hole(Schematic schematic, int x, int y, int z)
		{
			uint left = schematic.GetColorAtVoxelIndex(x-1, y, z);
			uint right = schematic.GetColorAtVoxelIndex(x+1, y, z);

			uint top = schematic.GetColorAtVoxelIndex(x, y +1, z);
			uint bottom = schematic.GetColorAtVoxelIndex(x, y - 1, z);

			uint diagonalLeft = schematic.GetColorAtVoxelIndex(x, y - 1, z + 1);
			uint diagonalRight = schematic.GetColorAtVoxelIndex(x, y - 1, z - 1);

			if (top != 0 && bottom != 0 && right == 0 && left != 0 && diagonalRight != 0 && diagonalLeft != 0)
			{
				schematic.AddVoxel(x, y, z, bottom);
			}

			return schematic;
		}

		private static Schematic Check1X1X2Hole(Schematic schematic, int x, int y, int z)
		{
			uint left = schematic.GetColorAtVoxelIndex(x-1, y, z);
			uint right = schematic.GetColorAtVoxelIndex(x+1, y, z);

			uint front = schematic.GetColorAtVoxelIndex(x, z - 1, x);
			uint back = schematic.GetColorAtVoxelIndex(x, y, z + 1);

			uint diagonalLeft = schematic.GetColorAtVoxelIndex(x +1, y, z + 1);
			uint diagonalRight = schematic.GetColorAtVoxelIndex(x -1, y, z + 1);

			if (back != 0 && front == 0 && left != 0 && right != 0 && diagonalRight != 0 && diagonalLeft != 0)
			{
				schematic.AddVoxel(x, y, z, back);
			}

			return schematic;
		}

		#endregion

	}
}
