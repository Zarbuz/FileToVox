using System;
using System.Collections.Generic;
using System.Linq;
using FileToVoxCommon.Generator.Shaders.Data;
using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;

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
			int total = (int)(schematic.RegionDict.Values.Count(region => region.BlockDict.Count > 0) * MathF.Pow(Schematic.CHUNK_SIZE, 3));
			Console.WriteLine("[INFO] Count voxel before: " + allVoxels.Count);
			using (ProgressBar progressBar = new ProgressBar())
			{
				foreach (Region region in schematic.RegionDict.Values.Where(region => region.BlockDict.Count > 0))
				{
					for (int x = region.X; x < region.X + Schematic.CHUNK_SIZE; x++)
					{
						for (int y = region.Y; y < region.Y + Schematic.CHUNK_SIZE; y++)
						{
							for (int z = region.Z; z < region.Z + Schematic.CHUNK_SIZE; z++)
							{
								progressBar.Report(index++ / (float)total);

								if (!region.GetVoxel(x, y, z, out Voxel voxel))
								{
									uint left = region.GetColorAtVoxelIndex(x - 1, y, z);
									uint left2 = region.GetColorAtVoxelIndex(x - 2, y, z);

									uint right = region.GetColorAtVoxelIndex(x + 1, y, z);
									uint right2 = region.GetColorAtVoxelIndex(x + 1, y, z);

									uint top = region.GetColorAtVoxelIndex(x, y + 1, z);
									uint bottom = region.GetColorAtVoxelIndex(x, y - 1, z);

									uint front = region.GetColorAtVoxelIndex(x, y, z + 1);
									uint front2 = region.GetColorAtVoxelIndex(x, y, z + 2);

									uint back = region.GetColorAtVoxelIndex(x, y, z - 1);
									uint back2 = region.GetColorAtVoxelIndex(x, y, z - 2);

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

									/*
									 
									 **
									*10*
									 **
									 
									 */

									uint frontRight = region.GetColorAtVoxelIndex(x + 1, y, z + 1);
									uint backRight = region.GetColorAtVoxelIndex(x + 1, y, z - 1);


									if (left != 0 && front != 0 && right == 0 && right2 != 0 && back != 0 && frontRight != 0 && backRight != 0)
									{
										schematic.AddVoxel(x, y, z, left);
										fixedHoles++;
										continue;
									}

									/*
									 
									 **
									*01*
									 **
									 
									*/
									uint frontLeft = region.GetColorAtVoxelIndex(x - 1, y, z + 1);
									uint backLeft = region.GetColorAtVoxelIndex(x - 1, y, z - 1);

									if (right != 0 && front != 0 && back != 0 && left == 0 && frontLeft != 0 && left2 != 0 && backLeft != 0)
									{
										schematic.AddVoxel(x, y, z, right);
										fixedHoles++;
										continue;
									}

									/*
									 
									*
								   *1*
								   *0*
								    *
									 
									*/

									if (left != 0 && right != 0 && front != 0 && back == 0 && backLeft != 0 && backRight != 0 && back2 != 0)
									{
										schematic.AddVoxel(x, y, z, right);
										fixedHoles++;
										continue;
									}

									/*
									 
									*
								   *0*
								   *1*
								    *
									 
									*/

									if (left != 0 && right != 0 && front == 0 && back != 0 && frontLeft != 0 && frontRight != 0 && front2 != 0)
									{
										schematic.AddVoxel(x, y, z, right);
										fixedHoles++;
										continue;
									}

									/*
									 
									 **
									*10*
									*00*
									 **
									 
									*/

									uint backRight2 = region.GetColorAtVoxelIndex(x + 2, y, z - 1);
									uint back2Right = region.GetColorAtVoxelIndex(x + 1, y, z - 2);


									if (left != 0 && front != 0 && right == 0 && frontRight != 0 && right2 != 0 &&
									    back == 0 && backLeft != 0 && backRight == 0 && back2 != 0 && backRight2 != 0 &&
									    back2Right != 0)
									{
										schematic.AddVoxel(x, y, z, left);
										fixedHoles++;
										continue;
									}

									/*
									 
									 **
									*01*
									*00*
									 **
									 
									*/

									uint backLeft2 = region.GetColorAtVoxelIndex(x - 2, y, z - 1);
									uint back2Left = region.GetColorAtVoxelIndex(x - 1, y, z - 2);

									if (right != 0 && front != 0 && left == 0 && left2 != 0 && frontLeft != 0 &&
									    back == 0 && backRight != 0 && backLeft == 0 && backLeft2 != 0 && back2 != 0 &&
									    back2Left != 0)
									{
										schematic.AddVoxel(x, y, z, right);
										fixedHoles++;
										continue;
									}
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
