using fNbt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicReader
{
    public static class SchematicReader
    {
        public static short WidthSchematic;
        public static short LengthSchematic;
        public static short HeightSchematic;

        private static int _ignore_min_y;
        private static int _ignore_max_y;

        public static Schematic LoadSchematic(string path, int min, int max)
        {
            NbtFile file = new NbtFile(path);
            _ignore_min_y = min;
            _ignore_max_y = max;
            return LoadSchematic(file);
        }

        private static Schematic LoadSchematic(NbtFile nbtFile)
        {
            RawSchematic raw = LoadRaw(nbtFile);
            List<HashSet<Block>> blocks = GetBlocks(raw);
            string name = Path.GetFileNameWithoutExtension(nbtFile.FileName);
            Schematic schematic = new Schematic(name, raw.Width, raw.Heigth, raw.Length, blocks, raw.TileEntities);
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
                        LengthSchematic = raw.Length;
                        break;
                    case "Materials": //String
                        raw.Materials = tag.StringValue;
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
                        raw.TileEntities = GetTileEntities(tag);
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

        private static List<TileEntity> GetTileEntities(NbtTag tileEntitiesList)
        {
            List<TileEntity> list = new List<TileEntity>();
            NbtList TileEntities = tileEntitiesList as NbtList;
            if (TileEntities != null)
            {
                foreach (NbtCompound compTag in TileEntities)
                {
                    NbtTag xTag = compTag["x"];
                    NbtTag yTag = compTag["y"];
                    NbtTag zTag = compTag["z"];
                    NbtTag idTag = compTag["id"];
                    TileEntity entity = new TileEntity(xTag.IntValue, yTag.IntValue, zTag.IntValue, idTag.StringValue);

                    if (entity.ID == "Sign")
                    {
                        NbtTag Text1Tag = compTag["Text1"];
                        NbtTag Text2Tag = compTag["Text2"];
                        NbtTag Text3Tag = compTag["Text3"];
                        NbtTag Text4Tag = compTag["Text4"];
                        Sign sign = new Sign(xTag.IntValue, yTag.IntValue, zTag.IntValue, Text1Tag.StringValue, Text2Tag.StringValue, Text3Tag.StringValue, Text4Tag.StringValue);
                        list.Add(sign);
                        continue;
                    }

                    list.Add(entity);
                }
            }
            return list;
        }

        private static List<HashSet<Block>> GetBlocks(RawSchematic rawSchematic)
        {
            if (rawSchematic.Heigth > 2016 || rawSchematic.Length > 2016 || rawSchematic.Width > 2016)
                throw new Exception("Schematic too big");

            Console.WriteLine("Started to read all blocks of the schematic ...");
            Console.WriteLine("[INFO] Raw schematic Width: " + rawSchematic.Width);
            Console.WriteLine("[INFO] Raw schematic Length: " + rawSchematic.Length);
            Console.WriteLine("[INFO] Raw schematic Height: " + rawSchematic.Heigth);
            Console.WriteLine("[INFO] Raw schematic total blocks " + rawSchematic.Data.Length);

            //Sorted by height (bottom to top) then length then width -- the index of the block at X,Y,Z is (Y×length + Z)×width + X.
            List<HashSet<Block>> blocks = new List<HashSet<Block>>();
            blocks.Add(new HashSet<Block>());
            int global = 0;

            int minY = Math.Max(_ignore_min_y, 0);
            int maxY = Math.Min(_ignore_max_y, rawSchematic.Heigth);

            for (int Y = minY; Y < maxY; Y++)
            {
                for (int Z = 0; Z < rawSchematic.Length; Z++)
                {
                    for (int X = 0; X < rawSchematic.Width; X++)
                    {
                        int index = (Y * rawSchematic.Length + Z) * rawSchematic.Width + X;
                        Block block = new Block(X, Y, Z, rawSchematic.Blocks[index], rawSchematic.Data[index]);
                        try
                        {
                            if (block.BlockID != 0) //don't add air block
                            {
                                blocks[global].Add(block);
                            }
                        }
                        catch (OutOfMemoryException e)
                        {
                            global++;
                            blocks.Add(new HashSet<Block>());
                            blocks[global].Add(block);
                        }
                    }
                }
            }
            return blocks;
        }
    }
}
