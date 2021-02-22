using FileToVox.Schematics;
using fNbt;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;

namespace FileToVox.Converter
{
	public class SchematicToSchematic : AbstractToSchematic
    {
        private readonly int mIgnoreMinY;
        private readonly int mIgnoreMaxY;
        private readonly int mScale;

        private readonly bool mExcavate;
        private readonly Dictionary<Tuple<int, int>, Color> mColors = new Dictionary<Tuple<int, int>, Color>();

        public SchematicToSchematic(string path, int min, int max, bool excavate, float scale) : base(path)
        {
            mIgnoreMinY = min;
            mIgnoreMaxY = max;
            mScale = (int)scale;
            mExcavate = excavate;
            LoadBlocks();
        }

        public override Schematic WriteSchematic()
        {
            NbtFile file = new NbtFile(_path);
            return LoadSchematic(file);
        }

        private void LoadBlocks()
        {
            try
            {
                string line;
                int counter = 0;
                // Read the file and display it line by line.  
                StreamReader file = new StreamReader(@"schematics/config.txt");
                Console.WriteLine("[LOG] Started to read config.txt for loading blocks colors");
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
                Console.WriteLine("[LOG] Done.");
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
            HashSet<Block> blocks = GetBlocks(raw);
            string name = Path.GetFileNameWithoutExtension(nbtFile.FileName);
            Schematic schematic = new Schematic((ushort)raw.Width, (ushort)raw.Heigth, (ushort)raw.Length, blocks);

            schematic.Width *= (ushort)mScale;
            schematic.Height *= (ushort)mScale;
            schematic.Length *= (ushort)mScale;
            return schematic;
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
                        LoadedSchematic.WidthSchematic = (ushort)raw.Width;
                        break;
                    case "Height": //Short
                        raw.Heigth = tag.ShortValue;
                        LoadedSchematic.HeightSchematic = (ushort)raw.Heigth;
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

        private HashSet<Block> GetBlocks(RawSchematic rawSchematic)
        {
            if (rawSchematic.Heigth > 2016 || rawSchematic.Length > 2016 || rawSchematic.Width > 2016)
            {
                throw new Exception("Schematic is too big");
            }

            Console.WriteLine($"[LOG] Started to read all blocks of the schematic...");

            LoadedSchematic.WidthSchematic = (ushort)(rawSchematic.Width * mScale);
            LoadedSchematic.LengthSchematic = (ushort)(rawSchematic.Length * mScale);
            LoadedSchematic.HeightSchematic = (ushort)(rawSchematic.Heigth * mScale);

            //Sorted by height (bottom to top) then length then width -- the index of the block at X,Y,Z is (Y×length + Z)×width + X.
            ConcurrentBag<Block> blocks = new ConcurrentBag<Block>();

            int minY = Math.Max(mIgnoreMinY, 0);
            int maxY = rawSchematic.Heigth <= mIgnoreMaxY ? Math.Min(mIgnoreMaxY, rawSchematic.Heigth) : rawSchematic.Heigth;

            Parallel.For(minY, (maxY * mScale), y =>
            {
                for (int z = 0; z < (rawSchematic.Length * mScale); z++)
                {
                    for (int x = 0; x < (rawSchematic.Width * mScale); x++)
                    {
                        int yProgress = (y / mScale);
                        int zProgress = (z / mScale);
                        int xProgress = (x / mScale);
                        int index = (yProgress * rawSchematic.Length + zProgress) * rawSchematic.Width + xProgress;
                        int blockId = rawSchematic.Blocks[index];
                        if (blockId != 0)
                        {
                            Block block = new Block((ushort) x, (ushort) y, (ushort) z,
                                GetBlockColor(rawSchematic.Blocks[index],
                                    rawSchematic.Data[index]).ColorToUInt());
                            if ((mExcavate && IsBlockConnectedToAir(rawSchematic, block, minY, maxY) || !mExcavate))
                            {
                                blocks.Add(block);
                            }
                        }
                    }
                }
            });
            Console.WriteLine($"[LOG] Done.");
            return blocks.ToHashSet();
        }

        private bool IsBlockConnectedToAir(RawSchematic rawSchematic, Block block, int minY, int maxY)
        {
            if (block.X - 1 >= 0 && block.X + 1 < rawSchematic.Width && block.Y - 1 >= minY && block.Y + 1 < maxY && block.Z - 1 >= 0 && block.Z < rawSchematic.Length)
            {
                int indexLeftX = (block.Y * rawSchematic.Length + block.Z) * rawSchematic.Width + (block.X - 1);
                int indexRightX = (block.Y * rawSchematic.Length + block.Z) * rawSchematic.Width + (block.X + 1);

                int indexTop = ((block.Y + 1) * rawSchematic.Length + block.Z) * rawSchematic.Width + block.X;
                int indexBottom = ((block.Y - 1) * rawSchematic.Length + block.Z) * rawSchematic.Width + block.X;

                int indexAhead = (block.Y * rawSchematic.Length + block.Z + 1) * rawSchematic.Width + block.X;
                int indexBehind = (block.Y * rawSchematic.Length + block.Z - 1) * rawSchematic.Width + block.X;
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
