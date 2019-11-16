using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using FileToVox.Schematics;
using FileToVox.Utils;
using nQuant;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Converter
{
    public class PNGToSchematic : BaseToSchematic
    {
        private bool _excavate;
        private int _maxHeight;
        private bool _color;
        private bool _top;
        private string _colorPath;

        public PNGToSchematic(string path, string colorPath, int height, bool excavate, bool color, bool top)
            : base(path)
        {
            _excavate = excavate;
            _maxHeight = height;
            _color = color;
            _top = top;
            _colorPath = colorPath;
        }

        public override Schematic WriteSchematic()
        {
            return WriteSchematicFromImage();
        }

        private Schematic WriteSchematicFromImage()
        {
            Bitmap bitmap = new Bitmap(new FileInfo(_path).FullName);
            Bitmap bitmapColor = new Bitmap(bitmap.Width, bitmap.Height); //default initialization
            WuQuantizer quantizer = new WuQuantizer();

            if (_colorPath != null)
            {
                bitmapColor = new Bitmap(new FileInfo(_colorPath).FullName);
                if (bitmap.Height != bitmapColor.Height || bitmap.Width != bitmapColor.Width)
                {
                    throw new ArgumentException("[ERROR] Image color is not the same size of the original image");
                }

                Image image = quantizer.QuantizeImage(bitmapColor);
                bitmapColor = new Bitmap(image);
            }
            else if (_color)
            {
                Image image = quantizer.QuantizeImage(bitmap);
                bitmap = new Bitmap(image);
            }

            Bitmap bitmapBlack = MakeGrayscale3(bitmap);

            if (bitmap.Width > 2016 || bitmap.Height > 2016)
            {
                throw new Exception("Image is too big (max size 2016x2016 px)");
            }

            Schematic schematic = new Schematic
            {
                Width = (short)bitmap.Width,
                Length = (short)bitmap.Height,
                Heigth = (short)_maxHeight,
                Blocks = new HashSet<Block>()
            };

            LoadedSchematic.LengthSchematic = schematic.Length;
            LoadedSchematic.WidthSchematic = schematic.Width;
            LoadedSchematic.HeightSchematic = schematic.Heigth;


            using (ProgressBar progressbar = new ProgressBar())
            {
                Console.WriteLine("[LOG] Started to write schematic from picture...");
                Console.WriteLine("[INFO] Picture Width: " + schematic.Width);
                Console.WriteLine("[INFO] Picture Length: " + schematic.Length);

                int size = schematic.Width * schematic.Length;
                int i = 0;
                for (int x = 0; x < schematic.Width; x++)
                {
                    for (int y = 0; y < schematic.Length; y++)
                    {
                        Color color = bitmap.GetPixel(x, y);
                        Color colorGray = bitmapBlack.GetPixel(x, y);
                        Color finalColor = (_colorPath != null) ? bitmapColor.GetPixel(x, y) : (_color) ? color : colorGray;
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
                                    int height = GetHeight(colorGray);
                                    if (_top)
                                    {
                                        int finalHeight = (height - 1 < 0) ? 0 : height - 1;
                                        AddBlock(ref schematic, new Block((short)x, (short)finalHeight, (short)y, finalColor.ColorToUInt()));
                                    }
                                    else
                                    {
                                        AddMultipleBlocks(ref schematic, 0, height, x, y, finalColor);
                                    }
                                }
                            }
                            else
                            {
                                Block block = new Block((short)x, (short)1, (short)y, color.ColorToUInt());
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

        private Bitmap MakeGrayscale3(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            Graphics g = Graphics.FromImage(newBitmap);

            //create the grayscale ColorMatrix
            ColorMatrix colorMatrix = new ColorMatrix(
               new float[][]
               {
                 new float[] {.3f, .3f, .3f, 0, 0},
                 new float[] {.59f, .59f, .59f, 0, 0},
                 new float[] {.11f, .11f, .11f, 0, 0},
                 new float[] {0, 0, 0, 1, 0},
                 new float[] {0, 0, 0, 0, 1}
               });

            //create some image attributes
            ImageAttributes attributes = new ImageAttributes();

            //set the color matrix attribute
            attributes.SetColorMatrix(colorMatrix);

            //draw the original image on the new image
            //using the grayscale color matrix
            g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
               0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);

            //dispose the Graphics object
            g.Dispose();
            return newBitmap;
        }

        private void AddMultipleBlocks(ref Schematic schematic, int minZ, int maxZ, int x, int y, Color color)
        {
            for (int z = minZ; z < maxZ; z++)
            {
                AddBlock(ref schematic, new Block((short)x, (short)z, (short)y, color.ColorToUInt()));
            }
        }

        private void AddBlock(ref Schematic schematic, Block block)
        {
            try
            {
                schematic.Blocks.Add(block);
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine($"[ERROR] OutOfMemoryException. Block: {block.ToString()}");
            }
        }

        private int GetHeight(Color color)
        {
            int intensity = (int)(color.R + color.G + color.B);
            float position = intensity / (float)765;
            return (int)(position * _maxHeight);
        }

        private void GenerateFromMinNeighbor(ref Schematic schematic, Bitmap blackBitmap, Color color, int x, int y)
        {
            int height = GetHeight(blackBitmap.GetPixel(x, y));
            try
            {
                if (x - 1 >= 0 && x + 1 < blackBitmap.Width && y - 1 >= 0 && y + 1 < blackBitmap.Height)
                {
                    var colorLeft = blackBitmap.GetPixel(x - 1, y);
                    var colorTop = blackBitmap.GetPixel(x, y - 1);
                    var colorRight = blackBitmap.GetPixel(x + 1, y);
                    var colorBottom = blackBitmap.GetPixel(x, y + 1);

                    int heightLeft = GetHeight(colorLeft);
                    int heightTop = GetHeight(colorTop);
                    int heightRight = GetHeight(colorRight);
                    int heightBottom = GetHeight(colorBottom);

                    var list = new List<int>
                        {
                            heightLeft, heightTop, heightRight, heightBottom
                        };

                    int min = list.Min();
                    if (min < height)
                    {
                        AddMultipleBlocks(ref schematic, list.Min(), height, x, y, color);
                    }
                    else
                    {
                        int finalHeight = (height - 1 < 0) ? 0 : height - 1;
                        AddBlock(ref schematic,
                            new Block((short)x, (short)finalHeight, (short)y, color.ColorToUInt()));
                    }
                }
                else
                {
                    AddMultipleBlocks(ref schematic, 0, height, x, y, color);
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine($"[ERROR] x: {x}, y: {y}, schematic width: {schematic.Width}, schematic length: {schematic.Length}");
            }
        }
    }
}
