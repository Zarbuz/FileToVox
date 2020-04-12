using FileToVox.Extensions;
using FileToVox.Schematics;
using FileToVox.Utils;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FileToVox.Converter.Image
{
	public class PNGToSchematic : ImageToSchematic
    {
        public PNGToSchematic(string path, string colorPath, int height, bool excavate, bool color, bool top, int colorLimit)
            : base(path, colorPath, height, excavate, color, top, colorLimit)
        {
        }

        public override Schematic WriteSchematic()
        {
            return WriteSchematicFromImage();
        }

        private Schematic WriteSchematicFromImage()
        {
            Bitmap bitmap = new Bitmap(new FileInfo(_path).FullName);
            Bitmap clone = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
            }
            Bitmap bitmapColor = new Bitmap(bitmap.Width, bitmap.Height); //default initialization
            Quantizer.Quantizer quantizer = new Quantizer.Quantizer();

            if (_colorPath != null)
            {
                bitmapColor = new Bitmap(new FileInfo(_colorPath).FullName);
                if (bitmap.Height != bitmapColor.Height || bitmap.Width != bitmapColor.Width)
                {
                    throw new ArgumentException("[ERROR] Image color is not the same size of the original image");
                }

                clone = new Bitmap(bitmapColor.Width, bitmapColor.Height, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(clone))
                {
                    gr.DrawImage(bitmapColor, new Rectangle(0, 0, clone.Width, clone.Height));
                }

                System.Drawing.Image image = quantizer.QuantizeImage(clone, 10, 70, _colorLimit);
                bitmapColor = new Bitmap(image);
            }
            else if (_color)
            {
                System.Drawing.Image image = quantizer.QuantizeImage(clone, 10, 70, _colorLimit);
                bitmap = new Bitmap(image);
            }

            Bitmap bitmapBlack = Grayscale.MakeGrayscale3(bitmap);

            if (bitmap.Width > 2000 || bitmap.Height > 2000)
            {
                throw new Exception("Image is too big (max size 2000x2000 px)");
            }

            Schematic schematic = new Schematic
            {
                Width = (ushort)(bitmap.Width + 1),
                Length = (ushort)(bitmap.Height + 1),
                Height = (ushort)(_maxHeight + 1),
                Blocks = new HashSet<Block>()
            };

            LoadedSchematic.LengthSchematic = schematic.Length;
            LoadedSchematic.WidthSchematic = schematic.Width;
            LoadedSchematic.HeightSchematic = schematic.Height;

            using (ProgressBar progressbar = new ProgressBar())
            {
                Console.WriteLine("[LOG] Started to write schematic from picture...");
                Console.WriteLine("[INFO] Picture Width: " + bitmap.Width);
                Console.WriteLine("[INFO] Picture Height: " + bitmap.Height);

                int size = bitmap.Width * bitmap.Height;
                int i = 0;
                for (int x = 0; x < bitmap.Width; x++)
                {
                    for (int y = 0; y < bitmap.Height; y++)
                    {
                        Color color = bitmap.GetPixel(x, y);
                        Color finalColor = (_colorPath != null) ? bitmapColor.GetPixel(x, y) : (_color) ? color : Color.White;
                        if (color.A != 0)
                        {
                            if (_maxHeight != 1)
                            {
                                if (_excavate)
                                {
                                    GenerateFromMinNeighbor(ref schematic, bitmapBlack, finalColor, x, y);
                                }
                                else
                                {
                                    int height = GetHeight(bitmapBlack.GetPixel(x, y));
                                    if (_top)
                                    {
                                        int finalHeight = (height - 1 < 0) ? 0 : height - 1;
                                        AddBlock(ref schematic, new Block((ushort)x, (ushort)finalHeight, (ushort)y, finalColor.ColorToUInt()));
                                    }
                                    else
                                    {
                                        AddMultipleBlocks(ref schematic, 0, height, x, y, finalColor);
                                    }
                                }
                            }
                            else
                            {
                                Block block = new Block((ushort)x, 0, (ushort)y, finalColor.ColorToUInt());
                                AddBlock(ref schematic, block);
                            }
                        }
                        progressbar.Report((i++ / (float)size));
                    }
                }
            }

            Console.WriteLine("[LOG] Done.");
            return schematic;
        }
    }
}
