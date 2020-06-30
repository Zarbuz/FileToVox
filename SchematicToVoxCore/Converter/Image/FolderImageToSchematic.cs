using FileToVox.Schematics;
using FileToVox.Utils;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace FileToVox.Converter.Image
{
	public class FolderImageToSchematic : AbstractToSchematic
    {
        private readonly bool _excavate;
        private readonly string _inputColorFile;
        public FolderImageToSchematic(string path, bool excavate, string inputColorFile) : base(path)
        {
            _excavate = excavate;
            _inputColorFile = inputColorFile;
        }

        public override Schematic WriteSchematic()
        {
            int height = Directory.GetFiles(_path, "*.png").Length;
            Console.WriteLine("[INFO] Count files in the folder : " + height);

            Schematic schematic = new Schematic();
            schematic.Height = (ushort) height;
            schematic.Width = (ushort)height;
            schematic.Length = (ushort)height;
            schematic.Blocks = new HashSet<Block>();

            LoadedSchematic.LengthSchematic = schematic.Length;
            LoadedSchematic.WidthSchematic = schematic.Width;
            LoadedSchematic.HeightSchematic = schematic.Height;
            Bitmap bitmapColor = null;
            if (_inputColorFile != null)
            {
	            bitmapColor = new Bitmap(_inputColorFile);
	            if (bitmapColor.Width > 256 || bitmapColor.Height > 1)
	            {
		            throw new ArgumentException("[ERROR] The input color file must have a dimension of 256x1 px");
	            }
            }

            using (ProgressBar progressbar = new ProgressBar())
            {
                string[] files = Directory.GetFiles(_path);
                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    Bitmap bitmap = new Bitmap(file);
                    DirectBitmap directBitmap = new DirectBitmap(bitmap);
                    for (int x = 0; x < directBitmap.Width; x++)
                    {
                        for (int y = 0; y < directBitmap.Height; y++)
                        {
                            Color color = directBitmap.GetPixel(x, y);
                            if (color != Color.Empty && color != Color.Transparent && color != Color.Black && (color.R != 0 && color.G != 0 && color.B != 0))
                            {
	                            if (_inputColorFile != null)
	                            {
		                            double distance = Math.Sqrt(Math.Pow((height / 2) - x, 2) + Math.Pow((height / 2) - y, 2));
		                            float range = (float) Math.Abs(distance / (height / 2)); //
		                            range = range > 1 ? 1 : range;
		                            color = bitmapColor.GetPixel((int)(range * (bitmapColor.Width - 1)), 0);
	                            }

                                if (_excavate)
                                {
                                    CheckNeighbor(ref schematic, directBitmap, color, i, x, y);
                                }
                                else
                                {
                                    schematic.Blocks.Add(new Block((ushort) x, (ushort) i, (ushort) y, color.ColorToUInt()));
                                }
                            }
                        }
                    }
                    directBitmap.Dispose();
                    progressbar.Report(i / (float)files.Length);
                }
            }
            Console.WriteLine("[LOG] Done.");
            return schematic;
        }

        private void CheckNeighbor(ref Schematic schematic, DirectBitmap bitmap, Color color, int i, int x, int y)
        {
            if (x - 1 >= 0 && x + 1 < bitmap.Width && y - 1 >= 0 && y + 1 < bitmap.Height)
            {
                Color left = bitmap.GetPixel(x - 1, y);
                Color top = bitmap.GetPixel(x, y - 1);
                Color right = bitmap.GetPixel(x + 1, y);
                Color bottom = bitmap.GetPixel(x, y + 1);

                bool leftColor = left != Color.Empty && left != Color.Transparent && left != Color.Black && (left.R != 0 && left.G != 0 && left.B != 0);
                bool topColor = top != Color.Empty && top != Color.Transparent && top != Color.Black && (top.R != 0 && top.G != 0 && top.B != 0);
                bool rightColor = right != Color.Empty && right != Color.Transparent && right != Color.Black && (right.R != 0 && right.G != 0 && right.B != 0);
                bool bottomColor = bottom != Color.Empty && bottom != Color.Transparent && bottom != Color.Black && (bottom.R != 0 && bottom.G != 0 && bottom.B != 0);

                if (!leftColor || !topColor || !rightColor || !bottomColor)
                {
	                schematic.Blocks.Add(new Block((ushort) x, (ushort) i, (ushort) y, color.ColorToUInt()));
                }
            }
        }
    }
}
