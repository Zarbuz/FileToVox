using FileToVox.Extensions;
using FileToVox.Schematics;
using FileToVox.Schematics.Tools;
using FileToVox.Utils;
using MoreLinq;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FileToVox.Converter.PointCloud
{
	public abstract class PointCloudToSchematic : AbstractToSchematic
	{
		protected readonly List<Voxel> _blocks = new List<Voxel>();
		protected readonly float _scale;
		protected readonly bool _flood;
		protected readonly int _colorLimit;
		protected readonly bool _holes;
		protected readonly bool _lonely;
		protected PointCloudToSchematic(string path, float scale, int colorLimit, bool holes, bool flood, bool lonely) : base(path)
		{
			_scale = scale;
			_colorLimit = colorLimit;
			_flood = flood;
			_holes = holes;
			_lonely = lonely;
		}

		protected abstract BodyDataDTO ReadContentFile();

		protected void VoxelizeData(BodyDataDTO data)
		{
			Vector3 minX = data.BodyVertices.MinBy(t => t.X);
			Vector3 minY = data.BodyVertices.MinBy(t => t.Y);
			Vector3 minZ = data.BodyVertices.MinBy(t => t.Z);

			float min = Math.Abs(Math.Min(minX.X, Math.Min(minY.Y, minZ.Z)));
			for (int i = 0; i < data.BodyVertices.Count; i++)
			{
				data.BodyVertices[i] += new Vector3(min, min, min);
				data.BodyVertices[i] = new Vector3((float)Math.Truncate(data.BodyVertices[i].X * _scale), (float)Math.Truncate(data.BodyVertices[i].Y * _scale), (float)Math.Truncate(data.BodyVertices[i].Z * _scale));
			}

			HashSet<Vector3> set = new HashSet<Vector3>();
			List<Vector3> vertices = new List<Vector3>();
			List<Color> colors = new List<Color>();

			Console.WriteLine("[LOG] Started to voxelize data...");
			using (ProgressBar progressbar = new ProgressBar())
			{
				for (int i = 0; i < data.BodyVertices.Count; i++)
				{
					if (!set.Contains(data.BodyVertices[i]))
					{
						set.Add(data.BodyVertices[i]);
						vertices.Add(data.BodyVertices[i]);
						colors.Add(data.BodyColors[i]);
					}
					progressbar.Report(i / (float)data.BodyVertices.Count);
				}
			}
			Console.WriteLine("[LOG] Done.");

			minX = vertices.MinBy(t => t.X);
			minY = vertices.MinBy(t => t.Y);
			minZ = vertices.MinBy(t => t.Z);

			min = Math.Min(minX.X, Math.Min(minY.Y, minZ.Z));
			for (int i = 0; i < vertices.Count; i++)
			{
				float max = Math.Max(vertices[i].X, Math.Max(vertices[i].Y, vertices[i].Z));
				if (/*max - min < 8000 && */max - min >= 0)
				{
					vertices[i] -= new Vector3(min, min, min);
					_blocks.Add(new Voxel((ushort)vertices[i].X, (ushort)vertices[i].Y, (ushort)vertices[i].Z, colors[i].ColorToUInt()));
				}
			}
		}

		protected void FillHoles(ref Schematic schematic)
		{
			Console.WriteLine("[LOG] Started to fill holes...");
			int max = schematic.Width * schematic.Height * schematic.Length * 2;
			int index = 0;
			using (ProgressBar progressBar = new ProgressBar())
			{
				for (int i = 0; i < 2; i++)
				{
					for (ushort y = 0; y < schematic.Height; y++)
					{
						for (ushort z = 0; z < schematic.Length; z++)
						{
							for (ushort x = 0; x < schematic.Width; x++)
							{
								ulong voxelIndex = Schematic.GetVoxelIndex(x, y, z);
								if (!schematic.BlockDict.ContainsKey(voxelIndex) && x > 0 && x <= schematic.Width && y > 0 && y <= schematic.Height && z > 0 && z <= schematic.Length)
								{
									Check1X1X1Hole(ref schematic, x, y, z);
									Check1X2X1Hole(ref schematic, x, y, z);
									Check2X1X1Hole(ref schematic, x, y, z);
									Check1X1X2Hole(ref schematic, x, y, z);
								}

								progressBar.Report(index / (float)max);
								index++;
							}
						}
					}
				}
			}

			Console.WriteLine("[LOG] Done.");
		}

		protected void FillInvisiblesVoxels(ref Schematic schematic)
		{
			int max = schematic.Width * schematic.Height * schematic.Length;
			int index = 0;
			uint white = Color.White.ColorToUInt();
			using (ProgressBar progressBar = new ProgressBar())
			{
				Console.WriteLine("[LOG] Started to fill all invisibles voxels... [1/2]");

				for (ushort y = 0; y < schematic.Height; y++)
				{
					for (ushort z = 0; z < schematic.Length; z++)
					{
						bool fill = false;
						for (ushort x = 0; x < schematic.Width; x++)
						{
							if (schematic.GetColorAtVoxelIndex(x, y, z) != 0 && !fill && HasHoleInLine(schematic, schematic.Width, (ushort)(x + 1), y, z))
							{
								fill = true;
							}
							else if (schematic.GetColorAtVoxelIndex(x, y, z) == 0 && fill)
							{
								schematic.AddVoxel(x, y, z, white);
							}
							else if (schematic.GetColorAtVoxelIndex(x, y, z) != 0 && fill && !HasHoleInLine(schematic, schematic.Width, (ushort)(x + 1), y, z))
							{
								fill = false;
							}

							progressBar.Report(index / (float)max);
							index++;
						}
					}
				}

				index = 0;

				Console.WriteLine("[LOG] Started to fill all invisibles voxels... [2/2]");

				for (int i = 0; i < 10; i++)
				{
					for (ushort y = 0; y < schematic.Height; y++)
					{
						for (ushort z = 0; z < schematic.Length; z++)
						{
							for (ushort x = 0; x < schematic.Width; x++)
							{
								if (schematic.GetColorAtVoxelIndex(x, y, z) == white && x - 1 >= 0 && x + 1 < schematic.Width && y - 1 >= 0 && y + 1 <= schematic.Height && z - 1 >= 0 && z <= schematic.Length)
								{
									uint left = schematic.GetColorAtVoxelIndex(x - 1, y, z);
									uint right = schematic.GetColorAtVoxelIndex(x + 1, y, z);
									uint top = schematic.GetColorAtVoxelIndex(x, y + 1, z);
									uint bottom = schematic.GetColorAtVoxelIndex(x, y - 1, z);
									uint front = schematic.GetColorAtVoxelIndex(x, y, z - 1);
									uint back = schematic.GetColorAtVoxelIndex(x, y, z + 1);

									if (left == 0 || right == 0 || top == 0 || bottom == 0 || front == 0 || back == 0)
									{
										schematic.RemoveVoxel(x, y, z);
									}
								}

								progressBar.Report(index / (float)(max * 10));
								index++;
							}
						}
					}

				}
			}
			Console.WriteLine("[LOG] Done.");
		}

		protected void FixLonelyVoxels(ref Schematic schematic)
		{
			Console.WriteLine("[LOG] Started to delete lonely voxels...");
			int max = schematic.Width * schematic.Height * schematic.Length * 2;
			int index = 0;
			using (ProgressBar progressBar = new ProgressBar())
			{
				for (ushort y = 0; y < schematic.Height; y++)
				{
					for (ushort z = 0; z < schematic.Length; z++)
					{
						for (ushort x = 0; x < schematic.Width; x++)
						{
							if (schematic.GetColorAtVoxelIndex(x, y, z) != 0 && x > 0 && x < schematic.Width && y > 0 && y < schematic.Height && z > 0 && z < schematic.Length)
							{
								if (schematic.GetColorAtVoxelIndex(x - 1, y, z) == 0 
								    && schematic.GetColorAtVoxelIndex(x+1, y, z) == 0 
								    && schematic.GetColorAtVoxelIndex(x, y  -1, z) == 0
								    && schematic.GetColorAtVoxelIndex(x, y +1, z) == 0 
								    && schematic.GetColorAtVoxelIndex(x, y, z - 1) == 0 
								    && schematic.GetColorAtVoxelIndex(x, y, z + 1) == 0)
								{
									schematic.RemoveVoxel(x, y, z);
								}
							}

							progressBar.Report(index / (float)max);
							index++;
						}
					}
				}
			}
			Console.WriteLine("[LOG] Done.");
		}
		public override Schematic WriteSchematic()
		{
			float minX = _blocks.MinBy(t => t.X).X;
			float minY = _blocks.MinBy(t => t.Y).Y;
			float minZ = _blocks.MinBy(t => t.Z).Z;
			
			List<Voxel> list = Quantization.ApplyQuantization(_blocks, _colorLimit);
			list.ApplyOffset(new Vector3(minX, minY, minZ));

			Schematic schematic = new Schematic(list.ToVoxelDictionary());
			
			if (_holes)
				FillHoles(ref schematic);
			if (_flood)
				FillInvisiblesVoxels(ref schematic);
			if (_lonely)
				FixLonelyVoxels(ref schematic);

			return schematic;
		}

		#region Private Static

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
		private static void Check1X1X1Hole(ref Schematic schematic, ushort x, ushort y, ushort z)
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
		private static void Check1X2X1Hole(ref Schematic schematic, ushort x, ushort y, ushort z)
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
		private static void Check2X1X1Hole(ref Schematic schematic, ushort x, ushort y, ushort z)
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
		}

		private static void Check1X1X2Hole(ref Schematic schematic, ushort x, ushort y, ushort z)
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
		}

		private static bool HasHoleInLine(Schematic schematic, ushort width, ushort startX, ushort y, ushort z)
		{
			for (int x = startX; x < width; x++)
			{
				if (schematic.GetColorAtVoxelIndex(x, y, z) != 0)
					return true;
			}

			return false;
		}


		#endregion
	}
}
