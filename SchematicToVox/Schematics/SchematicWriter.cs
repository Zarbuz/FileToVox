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
        public static Schematic WriteSchematic(string path)
        {
            return WriteSchematicFromImage(path);
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
    }
}
