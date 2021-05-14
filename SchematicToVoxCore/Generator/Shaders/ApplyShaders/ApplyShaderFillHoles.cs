using FileToVox.Generator.Shaders.Data;
using FileToVox.Schematics;
using FileToVox.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileToVox.Generator.Shaders
{
	public class ApplyShaderFixHoles : IShaderGenerator
	{
		private bool mShouldBreak;
		public Schematic ApplyShader(Schematic schematic, ShaderStep shaderStep)
		{
			ShaderFixHoles shaderFixHoles = shaderStep as ShaderFixHoles;
			for (int i = 0; i < shaderFixHoles.Iterations; i++)
			{
				Console.WriteLine("[INFO] Process iteration: " + i);
				schematic = ProcessShaderFixHoles(schematic);
				if (mShouldBreak)
				{
					break;
				}
			}

			return schematic;
		}

		private Schematic ProcessShaderFixHoles(Schematic schematic)
		{
			List<Voxel> allVoxels = schematic.GetAllVoxels();
			int index = 0;
			int fixedHoles = 0;
			int total = (int)(schematic.RegionDict.Values.Count(region => region.BlockDict.Count > 0) * MathF.Pow(Program.CHUNK_SIZE, 3));
			Console.WriteLine("[INFO] Count voxel before: " + allVoxels.Count);
			using (ProgressBar progressBar = new ProgressBar())
			{
				foreach (Region region in schematic.RegionDict.Values.Where(region => region.BlockDict.Count > 0))
				{
					for (int x = region.X; x < region.X + Program.CHUNK_SIZE; x++)
					{
						for (int y = region.Y; y < region.Y + Program.CHUNK_SIZE; y++)
						{
							for (int z = region.Z; z < region.Z + Program.CHUNK_SIZE; z++)
							{
								progressBar.Report(index++ / (float)total);

								if (!region.GetVoxel(x, y, z, out Voxel voxel))
								{
									uint left = region.GetColorAtVoxelIndex(x - 1, y, z);
									uint right = region.GetColorAtVoxelIndex(x + 1, y, z);

									uint top = region.GetColorAtVoxelIndex(x, y + 1, z);
									uint bottom = region.GetColorAtVoxelIndex(x, y - 1, z);

									uint front = region.GetColorAtVoxelIndex(x, y, z + 1);
									uint back = region.GetColorAtVoxelIndex(x, y, z - 1);

									//1x1
									if (left != 0 && right != 0 && front != 0 && back != 0)
									{
										schematic.AddVoxel(x, y, z, left);
										fixedHoles++;
										continue;
									}

									if (left != 0 && right != 0 && top != 0 && bottom != 0)
									{
										schematic.AddVoxel(x, y, z, top);
										fixedHoles++;
										continue;
									}

									if (front != 0 && back != 0 && top != 0 && bottom != 0)
									{
										schematic.AddVoxel(x, y, z, front);
										fixedHoles++;
										continue;
									}

									//Edges horizontal bottom
									if (left != 0 && right != 0 && bottom != 0 && front != 0)
									{
										schematic.AddVoxel(x, y, z, front);
										fixedHoles++;
										continue;
									}

									if (left != 0 && right != 0 && bottom != 0 && back != 0)
									{
										schematic.AddVoxel(x, y, z, back);
										fixedHoles++;
										continue;
									}

									if (front != 0 && back != 0 && bottom != 0 && left != 0)
									{
										schematic.AddVoxel(x, y, z, front);
										fixedHoles++;
										continue;
									}

									if (front != 0 && back != 0 && bottom != 0 && right != 0)
									{
										schematic.AddVoxel(x, y, z, right);
										fixedHoles++;
										continue;
									}

									//Edges horizontal top
									if (left != 0 && right != 0 && top != 0 && front != 0)
									{
										schematic.AddVoxel(x, y, z, front);
										fixedHoles++;
										continue;
									}

									if (left != 0 && right != 0 && top != 0 && back != 0)
									{
										schematic.AddVoxel(x, y, z, back);
										fixedHoles++;
										continue;
									}

									if (front != 0 && back != 0 && top != 0 && left != 0)
									{
										schematic.AddVoxel(x, y, z, front);
										fixedHoles++;
										continue;
									}

									if (front != 0 && back != 0 && top != 0 && right != 0)
									{
										schematic.AddVoxel(x, y, z, right);
										fixedHoles++;
										continue;
									}


									//Edges vertical (4)
									if (left != 0 && top != 0 && bottom != 0 && front != 0)
									{
										schematic.AddVoxel(x, y, z, left);
										fixedHoles++;
										continue;
									}

									if (left != 0 && top != 0 && bottom != 0 && back != 0)
									{
										schematic.AddVoxel(x, y, z, back);
										fixedHoles++;
										continue;
									}

									if (right != 0 && top != 0 && bottom != 0 && front != 0)
									{
										schematic.AddVoxel(x, y, z, right);
										fixedHoles++;
										continue;
									}

									if (right != 0 && top != 0 && bottom != 0 && back != 0)
									{
										schematic.AddVoxel(x, y, z, back);
										fixedHoles++;
										continue;
									}

									////Edges bottom (3)
									//if (left != 0 && front != 0 && bottom != 0)
									//{
									//	schematic.AddVoxel(x, y, z, left);
									//	fixedHoles++;
									//	continue;
									//}

									//if (right != 0 && front != 0 && bottom != 0)
									//{
									//	schematic.AddVoxel(x, y, z, right);
									//	fixedHoles++;
									//	continue;
									//}

									//if (left != 0 && back != 0 && bottom != 0)
									//{
									//	schematic.AddVoxel(x, y, z, left);
									//	fixedHoles++;
									//	continue;
									//}

									//if (right != 0 && back != 0 && bottom != 0)
									//{
									//	schematic.AddVoxel(x, y, z, right);
									//	fixedHoles++;
									//	continue;
									//}

								}
							}
						}
					}
				}
			}

			if (fixedHoles == 0)
			{
				mShouldBreak = true;
				Console.WriteLine("[INFO] NO VOXEL CHANGED, BREAK");
			}

			Console.WriteLine("[INFO] Fixed holes: " + fixedHoles);
			Console.WriteLine("[INFO] Done.");
			return schematic;
		}
	}
}
