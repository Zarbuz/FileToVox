using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Schematics
{
    public static class SchematicWriter
    {
        private const int HEIGHT_SIZE_HEIGHTMAP = 100;

        public static Schematic WriteSchematic(string path, bool heightmap)
        {
            if (!heightmap)
                return WriteSchematicFromImage(path);
            else
                return WriteSchematicFromHeightmap(path);
        }

        private static Schematic WriteSchematicFromImage(string path)
        {
            FileInfo info = new FileInfo(path);
            Bitmap bitmap = new Bitmap(info.FullName);

            Schematic schematic = new Schematic
            {
                Width = (short)bitmap.Width,
                Heigth = 1,
                Length = (short)bitmap.Height,
                Blocks = new List<HashSet<Block>>()
            };
            SchematicReader.LengthSchematic = schematic.Length;
            SchematicReader.WidthSchematic = schematic.Width;
            schematic.Blocks.Add(new HashSet<Block>());
            int size = schematic.Width * schematic.Length;
            for (int i = 0; i < size; i++)
            {
                int x = i % schematic.Width;
                int y = i / schematic.Width;
                var color = bitmap.GetPixel(x, y);
                if (color.A != 0)
                {
                    Block block = new Block(x, 1, y, 1, 1, color);
                    schematic.Blocks[0].Add(block);
                }
            }

            return schematic;
        }

        private static Schematic WriteSchematicFromHeightmap(string path)
        {
            FileInfo info = new FileInfo(path);
            Bitmap bitmap = new Bitmap(info.FullName);

            Schematic schematic = new Schematic
            {
                Width = (short)bitmap.Width,
                Heigth = 2000, //TEMP
                Length = (short)bitmap.Height,
                Blocks = new List<HashSet<Block>>()
            };

            SchematicReader.LengthSchematic = schematic.Length;
            SchematicReader.WidthSchematic = schematic.Width;
            SchematicReader.HeightSchematic = schematic.Heigth;
            schematic.Blocks.Add(new HashSet<Block>());

            int size = schematic.Width * schematic.Length;
            int global = 0;
            for (int i = 0; i < size; i++)
            {
                int x = i % schematic.Width;
                int y = i / schematic.Width;
                var color = bitmap.GetPixel(x, y);
                int intensity = color.R + color.G + color.B;
                float position = intensity / (float)765;
                int height = (int)(position * HEIGHT_SIZE_HEIGHTMAP);
                if (color.A != 0)
                {
                    for (int z = 0; z < height; z++)
                    {
                        Block block = new Block(x, z, y, 1, 1, color);
                        try
                        {
                            schematic.Blocks[global].Add(block);
                        }
                        catch (OutOfMemoryException)
                        {
                            global++;
                            schematic.Blocks.Add(new HashSet<Block>());
                            schematic.Blocks[global].Add(block);
                        }

                    }
                }
            }
            return schematic;
        }

        private static bool IsBlockConnectedToAir(Schematic schematic, Block block, int minY, int maxY)
        {
            if (block.X - 1 >= 0 && block.X + 1 < schematic.Width && block.Y - 1 >= minY && block.Y + 1 < maxY && block.Z - 1 >= 0 && block.Z < schematic.Length)
            {
                int indexLeftX = (block.Y * schematic.Length + block.Z) * schematic.Width + (block.X - 1);
                int indexRightX = (block.Y * schematic.Length + block.Z) * schematic.Width + (block.X + 1);

                int indexTop = ((block.Y + 1) * schematic.Length + block.Z) * schematic.Width + block.X;
                int indexBottom = ((block.Y - 1) * schematic.Length + block.Z) * schematic.Width + block.X;

                int indexAhead = (block.Y * schematic.Length + block.Z + 1) * schematic.Width + block.X;
                int indexBehind = (block.Y * schematic.Length + block.Z - 1) * schematic.Width + block.X;
                return (schematic.Blocks[indexLeftX] == 0 || schematic.Blocks[indexRightX] == 0
                    || schematic.Blocks[indexTop] == 0 || schematic.Blocks[indexBottom] == 0
                    || schematic.Blocks[indexAhead] == 0 || schematic.Blocks[indexBehind] == 0);

            }
            return false;
        }
    }
}
