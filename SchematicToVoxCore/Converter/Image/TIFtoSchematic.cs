using FileToVox.Schematics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using BitMiracle.LibTiff.Classic;
using FileToVox.Extensions;
using FileToVox.Utils;
using nQuant;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Converter.Image
{
    public class TIFtoSchematic : ImageToSchematic
    {
        public TIFtoSchematic(string path, string colorPath, int height, bool excavate, bool color, bool top) : base(path, colorPath, height, excavate, color, top)
        {
        }

        public override Schematic WriteSchematic()
        {
            return WriteSchematicFromImage();
        }

        private Schematic WriteSchematicFromImage()
        {
            Bitmap bitmap = ConvertTifToBitmap(_path);
            Bitmap bitmapColor = new Bitmap(bitmap.Width, bitmap.Height); //default initialization
            WuQuantizer quantizer = new WuQuantizer();

            if (_colorPath != null)
            {
                bitmapColor = ConvertTifToBitmap(_colorPath);
                if (bitmap.Height != bitmapColor.Height || bitmap.Width != bitmapColor.Width)
                {
                    throw new ArgumentException("[ERROR] Image color is not the same size of the original image");
                }

                System.Drawing.Image image = quantizer.QuantizeImage(bitmapColor);
                bitmapColor = new Bitmap(image);
            }
            else if (_color)
            {
                System.Drawing.Image image = quantizer.QuantizeImage(bitmap);
                bitmap = new Bitmap(image);
            }

            Bitmap bitmapBlack = Grayscale.MakeGrayscale3(bitmap);

            if (bitmap.Width > 2016 || bitmap.Height > 2016)
            {
                throw new Exception("Image is too big (max size 2016x2016 px)");
            }

            Schematic schematic = new Schematic
            {
                Width = (ushort)bitmap.Width,
                Length = (ushort)bitmap.Height,
                Heigth = (ushort)_maxHeight,
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
                                Block block = new Block((ushort)x, (ushort)1, (ushort)y, finalColor.ColorToUInt());
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

        private Bitmap ConvertTifToBitmap(string path)
        {
            using (Tiff tiff = Tiff.Open(path, "r"))
            {
                FieldValue[] value = tiff.GetField(TiffTag.IMAGEWIDTH);
                int width = value[0].ToInt();

                value = tiff.GetField(TiffTag.IMAGELENGTH);
                int height = value[0].ToInt();

                int[] raster = new int[height * width];
                if (!tiff.ReadRGBAImage(width, height, raster))
                {
                    throw new Exception("Could not read image");
                }

                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                byte[] bytes = new byte[bmpData.Stride * bmpData.Height];

                for (int y = 0; y < bmp.Height; y++)
                {
                    int rasterOffset = y * bmp.Width;
                    int bitsOffset = (bmp.Height - y - 1) * bmpData.Stride;

                    for (int x = 0; x < bmp.Width; x++)
                    {
                        int rgba = raster[rasterOffset++];
                        bytes[bitsOffset++] = (byte)((rgba >> 16) & 0xff);
                        bytes[bitsOffset++] = (byte)((rgba >> 8) & 0xff);
                        bytes[bitsOffset++] = (byte)(rgba & 0xff);
                        bytes[bitsOffset++] = (byte)((rgba >> 24) & 0xff);
                    }
                }

                System.Runtime.InteropServices.Marshal.Copy(bytes, 0, bmpData.Scan0, bytes.Length);
                bmp.UnlockBits(bmpData);

                return bmp;
            }
        }
    }
}
