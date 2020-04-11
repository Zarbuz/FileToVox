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
        protected readonly List<Block> _blocks = new List<Block>();
        protected readonly float _scale;
        protected readonly bool _flood;
        protected readonly int _colorLimit;
        protected readonly bool _holes;
        protected PointCloudToSchematic(string path, float scale, int colorLimit, bool holes, bool flood) : base(path)
        {
            _scale = scale;
            _colorLimit = colorLimit;
            _flood = flood;
            _holes = holes;
        }

        protected HashSet<Block> FillHoles(uint[,,] blocks, Schematic schematic)
        {
            Console.WriteLine("[LOG] Started to fill holes...");
            int max = schematic.Width * schematic.Heigth * schematic.Length * 2;
            int index = 0;
            using (ProgressBar progressBar = new ProgressBar())
            {
                for (int i = 0; i < 2; i++)
                {
                    for (ushort y = 0; y < schematic.Heigth; y++)
                    {
                        for (ushort z = 0; z < schematic.Length; z++)
                        {
                            for (ushort x = 0; x < schematic.Width; x++)
                            {
                                if (blocks[x, y, z] == 0 && x > 0 && x < schematic.Width && y > 0 &&
                                    y < schematic.Heigth && z > 0 && z < schematic.Length)
                                {
                                    blocks = Check1X1X1Hole(blocks, x, y, z);
                                    blocks = Check1X2X1Hole(blocks, x, y, z);
                                    blocks = Check2X1X1Hole(blocks, x, y, z);
                                    blocks = Check1X1X2Hole(blocks, x, y, z);
                                }

                                progressBar.Report(index / (float) max);
                                index++;
                            }
                        }
                    }
                }
            }

            return blocks.ToHashSetFrom3DArray();
        }

        protected HashSet<Block> FillInvisiblesVoxels(uint[,,] blocks, Schematic schematic)
        {
            int max = schematic.Width * schematic.Heigth * schematic.Length;
            int index = 0;
            uint white = Color.White.ColorToUInt();
			using (ProgressBar progressBar = new ProgressBar())
			{
				Console.WriteLine("[LOG] Started to fill all invisibles voxels... [1/2]");

				for (ushort y = 0; y < schematic.Heigth; y++)
				{
					for (ushort z = 0; z < schematic.Length; z++)
					{
						bool fill = false;
						for (ushort x = 0; x < schematic.Width; x++)
						{
							if (blocks[x, y, z] != 0 && !fill && HasHoleInLine(blocks, schematic.Width, (ushort) (x + 1), y, z))
							{
								fill = true;
							}
							else if (blocks[x, y, z] == 0 && fill)
							{
								blocks[x, y, z] = white;
							}
							else if (blocks[x, y, z] != 0 && fill && !HasHoleInLine(blocks, schematic.Width, (ushort)(x+1), y, z))
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
					for (ushort y = 0; y < schematic.Heigth; y++)
					{
						for (ushort z = 0; z < schematic.Length; z++)
						{
							for (ushort x = 0; x < schematic.Width; x++)
							{
								if (blocks[x, y, z] == white && x - 1 >= 0 && x + 1 < schematic.Width && y - 1 >= 0 &&
								    y + 1 < schematic.Heigth && z - 1 >= 0 && z < schematic.Length)
								{
									uint left = blocks[x - 1, y, z];
									uint right = blocks[x + 1, y, z];
									uint top = blocks[x, y + 1, z];
									uint bottom = blocks[x, y - 1, z];
									uint front = blocks[x, y, z - 1];
									uint back = blocks[x, y, z + 1];

									if (left == 0 || right == 0 || top == 0 || bottom == 0 || front == 0 || back == 0)
									{
										blocks[x, y, z] = 0;
									}
								}

								progressBar.Report(index / (float) (max * 10));
								index++;
							}
						}
					}

				}
			}

			return blocks.ToHashSetFrom3DArray();
		}

       

        public override Schematic WriteSchematic()
        {
	        float minX = _blocks.MinBy(t => t.X).X;
	        float minY = _blocks.MinBy(t => t.Y).Y;
	        float minZ = _blocks.MinBy(t => t.Z).Z;

	        float maxX = _blocks.MaxBy(t => t.X).X;
	        float maxY = _blocks.MaxBy(t => t.Y).Y;
	        float maxZ = _blocks.MaxBy(t => t.Z).Z;

	        Schematic schematic = new Schematic()
	        {
		        Length = (ushort)(Math.Abs(maxZ - minZ) + 1),
		        Width = (ushort)(Math.Abs(maxX - minX) + 1),
		        Heigth = (ushort)(Math.Abs(maxY - minY) + 1),
		        Blocks = new HashSet<Block>()
	        };

	        LoadedSchematic.LengthSchematic = schematic.Length;
	        LoadedSchematic.WidthSchematic = schematic.Width;
	        LoadedSchematic.HeightSchematic = schematic.Heigth;
	        List<Block> list = Quantization.ApplyQuantization(_blocks, _colorLimit);
	        list.ApplyOffset(new Vector3(minX, minY, minZ));
	        HashSet<Block> hashSet = list.ToHashSet();
			if (_holes)
				hashSet = FillHoles(hashSet.To3DArray(schematic), schematic);
	        if (_flood)
		        hashSet = FillInvisiblesVoxels(hashSet.To3DArray(schematic), schematic);
	        schematic.Blocks = hashSet;

	        return schematic;
        }

        #region Private Static

		/// <summary>
		/// .X.
		/// X0X
		/// .X.
		/// </summary>
		/// <param name="blocks"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <returns></returns>
		private static uint[,,] Check1X1X1Hole(uint[,,] blocks, ushort x, ushort y, ushort z)
        {
            uint left = blocks[x - 1, y, z];
            uint right = blocks[x + 1, y, z];

            uint front = blocks[x, y, z - 1];
            uint back = blocks[x, y, z + 1];

            uint top = blocks[x, y + 1, z];
            uint bottom = blocks[x, y - 1, z];


            if (left != 0 && right != 0 && front != 0 && back != 0)
            {
                blocks[x, y, z] = left;
            }

            if (top != 0 && bottom != 0)
            {
                blocks[x, y, z] = top;
            }

            return blocks;
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
        private static uint[,,] Check1X2X1Hole(uint[,,] blocks, ushort x, ushort y, ushort z)
        {
            uint left = blocks[x - 1, y, z];
            uint right = blocks[x + 1, y, z];

            uint top = blocks[x, y - 1, z];
            uint bottom = blocks[x, y + 1, z];

            uint front = blocks[x, y, z - 1];
            uint back = blocks[x, y, z + 1];

            uint diagonalLeft = blocks[x - 1, y - 1, z];
            uint diagonalRight = blocks[x + 1, y - 1, z];

            uint diagonalLeft2 = blocks[x, y - 1, z + 1];
            uint diagonalRight2 = blocks[x, y - 1, z - 1];


            if (bottom == 0 && top != 0 && right != 0 && left != 0 && diagonalRight != 0 && diagonalLeft != 0)
            {
                blocks[x, y, z] = top;
            }

            if (bottom == 0 && top != 0 && front != 0 && back != 0 && diagonalRight2 != 0 && diagonalLeft2 != 0)
            {
                blocks[x, y, z] = top;
            }

            return blocks;
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
        private static uint[,,] Check2X1X1Hole(uint[,,] blocks, ushort x, ushort y, ushort z)
        {
            uint left = blocks[x - 1, y, z];
            uint right = blocks[x + 1, y, z];

            uint top = blocks[x, y - 1, z];
            uint bottom = blocks[x, y + 1, z];

            uint diagonalLeft = blocks[x, y + 1, z + 1];
            uint diagonalRight = blocks[x, y - 1, z + 1];

            if (top != 0 && bottom != 0 && right == 0 && left != 0 && diagonalRight != 0 && diagonalLeft != 0)
            {
                blocks[x, y, z] = bottom;
            }

            return blocks;
        }

        private static uint[,,] Check1X1X2Hole(uint[,,] blocks, ushort x, ushort y, ushort z)
        {
            uint left = blocks[x - 1, y, z];
            uint right = blocks[x + 1, y, z];

            uint front = blocks[x, y, z - 1];
            uint back = blocks[x, y, z + 1];

            uint diagonalLeft = blocks[x + 1, y, z + 1];
            uint diagonalRight = blocks[x - 1, y, z + 1];

            if (back != 0 && front == 0 && left != 0 && right != 0 && diagonalRight != 0 && diagonalLeft != 0)
            {
                blocks[x, y, z] = back;
            }

            return blocks;
        }

        private static bool HasHoleInLine(uint[,,] blocks, ushort width, ushort startX, ushort y, ushort z)
        {
	        for (int x = startX; x < width; x++)
	        {
		        if (blocks[x, y, z] != 0)
			        return true;
	        }

	        return false;
        }

       
        #endregion
	}
}
