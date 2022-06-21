using fNbt;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using FileToVox.Extensions;
using FileToVoxCore.Extensions;
using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;

namespace FileToVox.Converter
{
	public class SchematicToSchematic : AbstractToSchematic
	{
		private readonly bool mExcavate;
		private readonly Dictionary<Tuple<int, int>, Color> mColors = new Dictionary<Tuple<int, int>, Color>();

		public SchematicToSchematic(string path, bool excavate) : base(path)
		{
			mExcavate = excavate;
			LoadBlocks();
		}

		public override Schematic WriteSchematic()
		{
			NbtFile file = new NbtFile(PathFile);
			return LoadSchematic(file);
		}

		private void LoadBlocks()
		{
			try
			{
				string line;
				int counter = 0;
				// Read the file and display it line by line.  
				StreamReader file = new StreamReader(@"./schematics/config.txt");
				Console.WriteLine("[INFO] Started to read config.txt for loading blocks colors");
				while ((line = file.ReadLine()) != null)
				{
					if (line[0] == '#')
						continue;

					string[] values = line.Split(' '); //6 values expected
					if (values.Length < 6)
						throw new Exception("Line is not well formated: " + line);

					int id = Convert.ToInt32(values[0]);
					int metadata = Convert.ToInt32(values[1]);
					int a = Convert.ToInt32(values[2]);
					int r = Convert.ToInt32(values[3]);
					int g = Convert.ToInt32(values[4]);
					int b = Convert.ToInt32(values[5]);

					mColors.Add(new Tuple<int, int>(id, metadata), Color.FromArgb(a, r, g, b));
					counter++;
				}
				Console.WriteLine("[INFO] Loaded blocks: " + counter);
				Console.WriteLine("[INFO] Done.");
				file.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine("[ERROR] LoadBlocks failed: " + e);
			}
		}

		private Schematic LoadSchematic(NbtFile nbtFile)
		{
			RawSchematic raw = LoadRaw(nbtFile);
			return CreateSchematic(raw);
		}

		private RawSchematic LoadRaw(NbtFile nbtFile)
		{
			RawSchematic raw = new RawSchematic();
			var rootTag = nbtFile.RootTag;

			foreach (NbtTag tag in rootTag.Tags)
			{
				switch (tag.Name)
				{
					case "Width": //Short
						raw.Width = tag.ShortValue;
						break;
					case "Height": //Short
						raw.Heigth = tag.ShortValue;
						break;
					case "Length": //Short
						raw.Length = tag.ShortValue;
						break;
					case "Blocks": //ByteArray
						raw.Blocks = tag.ByteArrayValue;
						break;
					case "Data": //ByteArray
						raw.Data = tag.ByteArrayValue;
						break;
					case "Entities": //List
						break; //Ignore
					case "TileEntities": //List
						break;
					case "Icon": //Compound
						break; //Ignore
					case "SchematicaMapping": //Compound
						tag.ToString();
						break; //Ignore
					default:
						break;
				}
			}
			return raw;
		}

		private Schematic CreateSchematic(RawSchematic rawSchematic)
		{
			if (rawSchematic.Heigth > Schematic.MAX_WORLD_HEIGHT || rawSchematic.Length > Schematic.MAX_WORLD_LENGTH || rawSchematic.Width > Schematic.MAX_WORLD_WIDTH)
			{
				throw new Exception("Schematic is too big");
			}

			Console.WriteLine($"[INFO] Started to read all blocks of the schematic...");

			//Sorted by height (bottom to top) then length then width -- the index of the block at X,Y,Z is (Y×length + Z)×width + X.
			Schematic schematic = new Schematic();

			int total = (rawSchematic.Heigth) * (rawSchematic.Length) * (rawSchematic.Width);
			int indexProgress = 0;
			using (ProgressBar progressbar = new ProgressBar())
			{
				for (int y = 0; y < (rawSchematic.Heigth); y++)
				{
					for (int z = 0; z < (rawSchematic.Length); z++)
					{
						for (int x = 0; x < (rawSchematic.Width); x++)
						{
							int index = (y * rawSchematic.Length + z) * rawSchematic.Width + x;
							int blockId = rawSchematic.Blocks[index];
							if (blockId != 0)
							{
								Voxel voxel = new Voxel((ushort) x, (ushort) y, (ushort) z, GetBlockColor(rawSchematic.Blocks[index], rawSchematic.Data[index]).ColorToUInt());
								if ((mExcavate && IsBlockConnectedToAir(rawSchematic, voxel, 0, rawSchematic.Heigth) || !mExcavate))
								{
									if (!schematic.ContainsVoxel(x, y, z))
									{
										schematic.AddVoxel(x, y, z, voxel.Color);
									}
								}
							}

							progressbar.Report(indexProgress++ / (float)total);
						}
					}
				}
			}

			Console.WriteLine($"[INFO] Done.");
			return schematic;
		}

		private bool IsBlockConnectedToAir(RawSchematic rawSchematic, Voxel voxel, int minY, int maxY)
		{
			if (voxel.X - 1 >= 0 && voxel.X + 1 < rawSchematic.Width && voxel.Y - 1 >= minY && voxel.Y + 1 < maxY && voxel.Z - 1 >= 0 && voxel.Z < rawSchematic.Length)
			{
				int indexLeftX = (voxel.Y * rawSchematic.Length + voxel.Z) * rawSchematic.Width + (voxel.X - 1);
				int indexRightX = (voxel.Y * rawSchematic.Length + voxel.Z) * rawSchematic.Width + (voxel.X + 1);

				int indexTop = ((voxel.Y + 1) * rawSchematic.Length + voxel.Z) * rawSchematic.Width + voxel.X;
				int indexBottom = ((voxel.Y - 1) * rawSchematic.Length + voxel.Z) * rawSchematic.Width + voxel.X;

				int indexAhead = (voxel.Y * rawSchematic.Length + voxel.Z + 1) * rawSchematic.Width + voxel.X;
				int indexBehind = (voxel.Y * rawSchematic.Length + voxel.Z - 1) * rawSchematic.Width + voxel.X;
				return (rawSchematic.Blocks[indexLeftX] == 0 || rawSchematic.Blocks[indexRightX] == 0
					|| rawSchematic.Blocks[indexTop] == 0 || rawSchematic.Blocks[indexBottom] == 0
					|| rawSchematic.Blocks[indexAhead] == 0 || rawSchematic.Blocks[indexBehind] == 0);

			}
			return false;
		}

		private Color GetBlockColor(int blockID, int data)
		{
			if (mColors.TryGetValue(new Tuple<int, int>(blockID, data), out Color color))
			{
				return color;
			}
			return mColors[new Tuple<int, int>(blockID, 0)];
		}


	}
}
