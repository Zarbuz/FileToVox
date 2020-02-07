using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using FileToVox.Schematics;
using fNbt;
using Motvin.Collections;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Converter
{
    public class SchematicToSchematic : AbstractToSchematic
    {
        private readonly int _ignoreMinY;
        private readonly int _ignoreMaxY;
        private readonly int _scale;

        private readonly bool _excavate;
        private readonly Dictionary<Tuple<int, int>, Color> _colors = new Dictionary<Tuple<int, int>, Color>();

        public SchematicToSchematic(string path, int min, int max, bool excavate, int scale) : base(path)
        {
            _ignoreMinY = min;
            _ignoreMaxY = max;
            _scale = scale;
            _excavate = excavate;
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

                    _colors.Add(new Tuple<int, int>(id, metadata), Color.FromArgb(a, r, g, b));
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
            FastHashSet<Block> blocks = GetBlocks(raw);
            string name = Path.GetFileNameWithoutExtension(nbtFile.FileName);
            Schematic schematic = new Schematic(name, (ushort)raw.Width, (ushort)raw.Heigth, (ushort)raw.Length, blocks);

            schematic.Width *= (ushort)_scale;
            schematic.Heigth *= (ushort)_scale;
            schematic.Length *= (ushort)_scale;
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

        private FastHashSet<Block> GetBlocks(RawSchematic rawSchematic)
        {
            if (rawSchematic.Heigth > 2016 || rawSchematic.Length > 2016 || rawSchematic.Width > 2016)
            {
                throw new Exception("Schematic is too big");
            }

            Console.WriteLine($"[LOG] Started to read all blocks of the schematic...");

            LoadedSchematic.WidthSchematic = (ushort)(rawSchematic.Width * _scale);
            LoadedSchematic.LengthSchematic = (ushort)(rawSchematic.Length * _scale);
            LoadedSchematic.HeightSchematic = (ushort)(rawSchematic.Heigth * _scale);

            //Sorted by height (bottom to top) then length then width -- the index of the block at X,Y,Z is (Y×length + Z)×width + X.
            ConcurrentBag<Block> blocks = new ConcurrentBag<Block>();

            int minY = Math.Max(_ignoreMinY, 0);
            int maxY = rawSchematic.Heigth <= _ignoreMaxY ? Math.Min(_ignoreMaxY, rawSchematic.Heigth) : rawSchematic.Heigth;

            Parallel.For(minY, (maxY * _scale), y =>
            {
                for (int z = 0; z < (rawSchematic.Length * _scale); z++)
                {
                    for (int x = 0; x < (rawSchematic.Width * _scale); x++)
                    {
                        int yProgress = (y / _scale);
                        int zProgress = (z / _scale);
                        int xProgress = (x / _scale);
                        int index = (yProgress * rawSchematic.Length + zProgress) * rawSchematic.Width + xProgress;
                        int blockId = rawSchematic.Blocks[index];
                        if (blockId != 0)
                        {
                            Block block = new Block((ushort) x, (ushort) y, (ushort) z,
                                GetBlockColor(rawSchematic.Blocks[index],
                                    rawSchematic.Data[index]).ColorToUInt());
                            if ((_excavate && IsBlockConnectedToAir(rawSchematic, block, minY, maxY) || !_excavate))
                            {
                                blocks.Add(block);
                            }
                        }
                    }
                }
            });
            Console.WriteLine($"[LOG] Done.");
            return blocks.ToHashSetFast();
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
            if (_colors.TryGetValue(new Tuple<int, int>(blockID, data), out Color color))
            {
                return color;
            }
            return _colors[new Tuple<int, int>(blockID, 0)];
        }


    }
}
