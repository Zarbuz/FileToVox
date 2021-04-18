using FileToVox.Schematics;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Xml.Linq;
using FileToVox.Converter.Image;
using FileToVox.Extensions;
using FileToVox.Generator.Heightmap.Data;

namespace FileToVox.Utils
{
	public static class ImageUtils
	{
		public static Schematic WriteSchematicFromImage(Bitmap bitmap, Bitmap colorBitmap, HeightmapStep heightmapStep)
		{
			Bitmap clone = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
			using (Graphics gr = Graphics.FromImage(clone))
			{
				gr.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
			}
			Bitmap bitmapColor = new Bitmap(bitmap.Width, bitmap.Height); //default initialization
			Quantizer.Quantizer quantizer = new Quantizer.Quantizer();

			if (colorBitmap != null)
			{
				if (bitmap.Height != bitmapColor.Height || bitmap.Width != bitmapColor.Width)
				{
					throw new ArgumentException("[ERROR] Image color is not the same size of the original image");
				}

				clone = new Bitmap(bitmapColor.Width, bitmapColor.Height, PixelFormat.Format32bppArgb);
				using (Graphics gr = Graphics.FromImage(clone))
				{
					gr.DrawImage(bitmapColor, new Rectangle(0, 0, clone.Width, clone.Height));
				}

				if (heightmapStep.ColorLimit != 256 || bitmapColor.CountColor() > 256)
				{
					Image image = quantizer.QuantizeImage(clone, 10, 70, heightmapStep.ColorLimit);
					bitmapColor = new Bitmap(image);
				}

			}
			else if (heightmapStep.EnableColor)
			{
				if (heightmapStep.ColorLimit != 256 || clone.CountColor() > 256)
				{
					Image image = quantizer.QuantizeImage(clone, 10, 70, heightmapStep.ColorLimit);
					bitmap = new Bitmap(image);
				}
			}

			Schematic schematic = WriteSchematicIntern(bitmap, bitmapColor, heightmapStep);
			return schematic;
		}

        public static Schematic WriteSchematicIntern(Bitmap bitmap, Bitmap bitmapColor, HeightmapStep heightmapStep)
        {
            Schematic schematic = new Schematic();

            Bitmap bitmapBlack = Grayscale.MakeGrayscale3(bitmap);
            DirectBitmap directBitmapBlack = new DirectBitmap(bitmapBlack);
            DirectBitmap directBitmap = new DirectBitmap(bitmap);
            DirectBitmap directBitmapColor = new DirectBitmap(bitmapColor);
            if (bitmap.Width > Schematic.MAX_WORLD_WIDTH || bitmap.Height > Schematic.MAX_WORLD_LENGTH)
            {
                throw new ArgumentException($"Image is too big (max size ${Schematic.MAX_WORLD_WIDTH}x${Schematic.MAX_WORLD_LENGTH} px)");
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
                        Color finalColor = (heightmapStep.ColorTexturePath != null) ? directBitmapColor.GetPixel(x, y) : (heightmapStep.EnableColor) ? color : Color.White;
                        if (color.A != 0)
                        {
                            if (heightmapStep.Height != 1)
                            {
                                if (heightmapStep.Excavate)
                                {
                                    GenerateFromMinNeighbor(ref schematic, directBitmapBlack, w, h, finalColor, x, y, heightmapStep.Height, heightmapStep.Offset);
                                }
                                else
                                {
                                    int computeHeight = GetHeight(directBitmapBlack.GetPixel(x, y), heightmapStep.Height) + heightmapStep.Offset;
	                                AddMultipleBlocks(ref schematic, heightmapStep.Offset, computeHeight, x, y, finalColor);
                                }
                            }
                            else
                            {
	                            AddSingleVoxel(ref schematic, x, heightmapStep.Offset, y, finalColor);
                            }
                        }
                        progressbar.Report((i++ / (float)size));
                    }
                }
            }

            Console.WriteLine("[LOG] Done.");
            return schematic;
        }

        public static void AddMultipleBlocks(ref Schematic schematic, int min, int max, int x, int y, Color color)
        {
            for (int i = min; i < max; i++)
            {
                AddBlock(ref schematic, new Voxel((ushort)x, (ushort)i, (ushort)y, color.ColorToUInt()));
            }
        }

        public static void AddSingleVoxel(ref Schematic schematic, int x, int y, int z, Color color)
        {
	        Voxel voxel = new Voxel((ushort) x, (ushort) y, (ushort) z, color.ColorToUInt());
            AddBlock(ref schematic, voxel);
        }

        public static void AddBlock(ref Schematic schematic, Voxel voxel)
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

        public static int GetHeight(Color color, int height)
        {
            int intensity = (int)(color.R + color.G + color.B);
            float position = intensity / (float)765;
            return (int)(position * height);
        }

        public static void GenerateFromMinNeighbor(ref Schematic schematic, DirectBitmap blackBitmap, int w, int h, Color color, int x, int y, int height, int offset)
        {
            int computeHeight = GetHeight(blackBitmap.GetPixel(x, y), height) + offset;
            try
            {
                if (x - 1 >= 0 && x + 1 < w && y - 1 >= 0 && y + 1 < h)
                {
                    Color colorLeft = blackBitmap.GetPixel(x - 1, y);
                    Color colorTop = blackBitmap.GetPixel(x, y - 1);
                    Color colorRight = blackBitmap.GetPixel(x + 1, y);
                    Color colorBottom = blackBitmap.GetPixel(x, y + 1);

                    int heightLeft = GetHeight(colorLeft, height) + offset;
                    int heightTop = GetHeight(colorTop, height) + offset;
                    int heightRight = GetHeight(colorRight, height) + offset;
                    int heightBottom = GetHeight(colorBottom, height) + offset;

                    var list = new List<int>
                        {
                            heightLeft, heightTop, heightRight, heightBottom
                        };

                    int min = list.Min();
                    if (min < computeHeight)
                    {
                        AddMultipleBlocks(ref schematic, min, computeHeight, x, y, color);
                    }
                    else
                    {
                        int finalHeight = (computeHeight - 1 < 0) ? 0 : computeHeight - 1;
                        AddSingleVoxel(ref schematic, x, finalHeight, y, color);
                    }
                }
                else
                {
                    AddMultipleBlocks(ref schematic, offset, computeHeight, x, y, color);
                }
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine($"[ERROR] x: {x}, y: {y}, schematic width: {schematic.Width}, schematic length: {schematic.Length}");
            }
        }
    }
}
