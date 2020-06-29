using FileToVox.Schematics;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace FileToVox.Converter.Image
{
    public abstract class ImageToSchematic : AbstractToSchematic
    {
        protected readonly bool _excavate;
        protected readonly int _maxHeight;
        protected readonly bool _color;
        protected readonly bool _top;
        protected readonly string _colorPath;
        protected readonly int _colorLimit;

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
            _colorPath = colorPath;
            _maxHeight = height;
            _excavate = excavate;
            _color = color;
            _top = top;
            _colorLimit = colorLimit;
        }

        protected void AddMultipleBlocks(ref Schematic schematic, int minZ, int maxZ, int x, int y, Color color)
        {
            for (int z = minZ; z < maxZ; z++)
            {
                AddBlock(ref schematic, new Block((ushort)x, (ushort)z, (ushort)y, color.ColorToUInt()));
            }
        }

        protected void AddBlock(ref Schematic schematic, Block block)
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

        protected int GetHeight(Color color)
        {
            int intensity = (int)(color.R + color.G + color.B);
            float position = intensity / (float)765;
            return (int)(position * _maxHeight);
        }

        protected void GenerateFromMinNeighbor(ref Schematic schematic, Bitmap blackBitmap, Color color, int x, int y)
        {
            int height = GetHeight(blackBitmap.GetPixel(x, y));
            try
            {
                if (x - 1 >= 0 && x + 1 < blackBitmap.Width && y - 1 >= 0 && y + 1 < blackBitmap.Height)
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
                            new Block((ushort)x, (ushort)finalHeight, (ushort)y, color.ColorToUInt()));
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
