using FileToVox.Schematics;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using FileToVox.Extensions;
using FileToVox.Utils;

namespace FileToVox.Converter.Image
{
    public abstract class ImageToSchematic : AbstractToSchematic
    {
        protected readonly bool Excavate;
        protected readonly int MaxHeight;
        protected readonly bool Color;
        protected readonly bool Top;
        protected readonly string ColorPath;
        protected readonly int ColorLimit;

        [StructLayout(LayoutKind.Explicit)]
        public struct RGB
        {
	        // Structure of pixel for a 24 bpp bitmap
	        [FieldOffset(0)] public byte blue;
	        [FieldOffset(1)] public byte green;
	        [FieldOffset(2)] public byte red;
	        [FieldOffset(3)] public byte alpha;
	        [FieldOffset(0)] public int argb;
        }

        protected ImageToSchematic(string path, string colorPath, int height, bool excavate, bool color, bool top, int colorLimit) : base(path)
        {
            ColorPath = colorPath;
            MaxHeight = height;
            Excavate = excavate;
            Color = color;
            Top = top;
            ColorLimit = colorLimit;
        }

        protected void AddMultipleBlocks(ref Schematic schematic, int minZ, int maxZ, int x, int y, Color color)
        {
            for (int z = minZ; z < maxZ; z++)
            {
                AddBlock(ref schematic, new Voxel((ushort)x, (ushort)z, (ushort)y, color.ColorToUInt()));
            }
        }

        protected void AddBlock(ref Schematic schematic, Voxel voxel)
        {
            try
            {
                schematic.Blocks.Add(voxel);
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine($"[ERROR] OutOfMemoryException. Block: {voxel.ToString()}");
            }
        }

        protected int GetHeight(Color color)
        {
            int intensity = (int)(color.R + color.G + color.B);
            float position = intensity / (float)765;
            return (int)(position * MaxHeight);
        }

        protected void GenerateFromMinNeighbor(ref Schematic schematic, DirectBitmap blackBitmap, int w, int h, Color color, int x, int y)
        {
            int height = GetHeight(blackBitmap.GetPixel(x, y));
            try
            {
                if (x - 1 >= 0 && x + 1 < w && y - 1 >= 0 && y + 1 < h)
                {
                    Color colorLeft = blackBitmap.GetPixel(x - 1, y);
                    Color colorTop = blackBitmap.GetPixel(x, y - 1);
                    Color colorRight = blackBitmap.GetPixel(x + 1, y);
                    Color colorBottom = blackBitmap.GetPixel(x, y + 1);

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
                            new Voxel((ushort)x, (ushort)finalHeight, (ushort)y, color.ColorToUInt()));
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

        protected Schematic WriteSchematicIntern(Bitmap bitmap, Bitmap bitmapColor)
        {
	        Schematic schematic = new Schematic();

	        Bitmap bitmapBlack = Grayscale.MakeGrayscale3(bitmap);
            DirectBitmap directBitmapBlack = new DirectBitmap(bitmapBlack);
            DirectBitmap directBitmap = new DirectBitmap(bitmap);
            DirectBitmap directBitmapColor = new DirectBitmap(bitmapColor);
	        if (bitmap.Width > 2000 || bitmap.Height > 2000)
	        {
		        throw new Exception("Image is too big (max size 2000x2000 px)");
	        }

            using (ProgressBar progressbar = new ProgressBar())
            {
                Console.WriteLine("[LOG] Started to write schematic from picture...");
                Console.WriteLine("[INFO] Picture Width: " + bitmap.Width);
                Console.WriteLine("[INFO] Picture Height: " + bitmap.Height);

                int size = bitmap.Width * bitmap.Height;
                int i = 0;
                int w = bitmap.Width;
                int h = bitmap.Height;
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        Color color = directBitmap.GetPixel(x, y);
                        Color finalColor = (ColorPath != null) ? directBitmapColor.GetPixel(x, y) : (Color) ? color : System.Drawing.Color.White;
                        if (color.A != 0)
                        {
                            if (MaxHeight != 1)
                            {
                                if (Excavate)
                                {
                                    GenerateFromMinNeighbor(ref schematic, directBitmapBlack, w, h, finalColor, x, y);
                                }
                                else
                                {
                                    int height = GetHeight(directBitmapBlack.GetPixel(x, y));
                                    if (Top)
                                    {
                                        int finalHeight = (height - 1 < 0) ? 0 : height - 1;
                                        AddBlock(ref schematic, new Voxel((ushort)x, (ushort)finalHeight, (ushort)y, finalColor.ColorToUInt()));
                                    }
                                    else
                                    {
                                        AddMultipleBlocks(ref schematic, 0, height, x, y, finalColor);
                                    }
                                }
                            }
                            else
                            {
                                Voxel voxel = new Voxel((ushort)x, 0, (ushort)y, finalColor.ColorToUInt());
                                AddBlock(ref schematic, voxel);
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
