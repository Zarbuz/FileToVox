using fNbt;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SchematicToVox.Extensions;

namespace SchematicToVox.Schematics
{
    public static class SchematicReader
    {
        public static short WidthSchematic;
        public static short LengthSchematic;
        public static short HeightSchematic;

        private static int _ignoreMinY;
        private static int _ignoreMaxY;
        private static int _scale;

        private static bool _excavate;

        public static Schematic LoadSchematic(string path, int min, int max, bool excavate, int scale)
        {
            NbtFile file = new NbtFile(path);
            _ignoreMinY = min;
            _ignoreMaxY = max;
            _scale = scale;
            _excavate = excavate;

            return LoadSchematic(file);
        }

        private static Schematic LoadSchematic(NbtFile nbtFile)
        {
            RawSchematic raw = LoadRaw(nbtFile);
            HashSet<Block> blocks = GetBlocks(raw);
            string name = Path.GetFileNameWithoutExtension(nbtFile.FileName);
            Schematic schematic = new Schematic(name, raw.Width, raw.Heigth, raw.Length, blocks);

            schematic.Width *= (short)_scale;
            schematic.Heigth *= (short)_scale;
            schematic.Length *= (short)_scale;
            return schematic;
        }

        private static RawSchematic LoadRaw(NbtFile nbtFile)
        {
            RawSchematic raw = new RawSchematic();
            var rootTag = nbtFile.RootTag;

            foreach (NbtTag tag in rootTag.Tags)
            {
                switch (tag.Name)
                {
                    case "Width": //Short
                        raw.Width = tag.ShortValue;
                        WidthSchematic = raw.Width;
                        break;
                    case "Height": //Short
                        raw.Heigth = tag.ShortValue;
                        HeightSchematic = raw.Heigth;
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

        private static HashSet<Block> GetBlocks(RawSchematic rawSchematic)
        {
            if (rawSchematic.Heigth > 2016 || rawSchematic.Length > 2016 || rawSchematic.Width > 2016)
            {
                throw new Exception("Schematic is too big");
            }

            Console.WriteLine("[LOG] Started to read all blocks of the schematic...");
            Console.WriteLine("[INFO] Raw schematic Width: " + rawSchematic.Width);
            Console.WriteLine("[INFO] Raw schematic Length: " + rawSchematic.Length);
            Console.WriteLine("[INFO] Raw schematic Height: " + rawSchematic.Heigth);
            Console.WriteLine("[INFO] Raw schematic total blocks " + rawSchematic.Data.Length);

            WidthSchematic = (short)(rawSchematic.Width * _scale);
            LengthSchematic = (short)(rawSchematic.Length * _scale);
            HeightSchematic = (short)(rawSchematic.Heigth * _scale);

            //Sorted by height (bottom to top) then length then width -- the index of the block at X,Y,Z is (Y×length + Z)×width + X.
            ConcurrentBag<Block> blocks = new ConcurrentBag<Block>();

            int minY = Math.Max(_ignoreMinY, 0);
            int maxY = Math.Min(_ignoreMaxY, rawSchematic.Heigth);

            Parallel.For(minY, (maxY * _scale), y =>
            {
                for (int z = 0; z < (rawSchematic.Length * _scale); z++)
                {
                    for (int x = 0; x < (rawSchematic.Width * _scale); x++)
                    {
                        int yProgress = y / _scale;
                        int zProgress = z / _scale;
                        int xProgress = x / _scale;
                        int index = (yProgress * rawSchematic.Length + zProgress) * rawSchematic.Width + xProgress;
                        int blockId = rawSchematic.Blocks[index];
                        if (blockId != 0)
                        {
                            Block block = new Block(x, y, z,
                                Extensions.Extensions.GetBlockColor(rawSchematic.Blocks[index],
                                    rawSchematic.Data[index]));
                            if ((_excavate && IsBlockConnectedToAir(rawSchematic, block, minY, maxY) || !_excavate))
                            {
                                blocks.Add(block);
                            }
                        }
                    }
                }

            });
            return blocks.ToHashSet();
        }

        private static bool IsBlockConnectedToAir(RawSchematic rawSchematic, Block block, int minY, int maxY)
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


    }
}
