using FileToVox.Extensions;
using FileToVoxCore.Schematics;
using ImageMagick;
using nQuant;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace FileToVox.Converter.Image
{
	public class MultipleImageToSchematic : AbstractToSchematic
	{
		private readonly bool mExcavate;
		private readonly string mInputColorFile;
		private readonly int mColorLimit;
		private readonly List<string> mImages;
		public MultipleImageToSchematic(List<string> images, bool excavate, string inputColorFile, int colorLimit)
		{
			mImages = images;
			mExcavate = excavate;
			mInputColorFile = inputColorFile;
			mColorLimit = colorLimit;
		}

		public override Schematic WriteSchematic()
		{
			int height = mImages.Count;
			Console.WriteLine("[INFO] Total images to process: " + mImages.Count);

			List<Voxel> blocks = new List<Voxel>();
			IPixelCollection<ushort> pixelsColor = null;
			int colorWidth = 0;
			if (mInputColorFile != null)
			{
				MagickImage bitmapColor = new MagickImage(mInputColorFile);
				if (bitmapColor.Width > 256 || bitmapColor.Height > 1)
				{
					throw new ArgumentException("[ERROR] The input color file must have a dimension of 256x1 px");
				}

				colorWidth = bitmapColor.Width;
				pixelsColor = bitmapColor.GetPixels();
			}

			for (int i = 0; i < mImages.Count; i++)
			{
				string file = mImages[i];
				Console.WriteLine("[INFO] Reading file: " + file);
				MagickImage bitmap = new MagickImage(file);
				IPixelCollection<ushort> pixels = bitmap.GetPixels();

				for (int x = 0; x < bitmap.Width; x++)
				{
					for (int y = 0; y < bitmap.Height; y++)
					{
						IPixel<ushort> pixel = pixels.GetPixel(x, y);
						Color color = Color.FromArgb(pixel.GetChannel(4), pixel.GetChannel(0), pixel.GetChannel(1), pixel.GetChannel(2));

						if (color != Color.Empty && color != Color.Transparent && color != Color.Black && (color.R != 0 && color.G != 0 && color.B != 0))
						{
							if (mInputColorFile != null)
							{
								double distance = Math.Sqrt(Math.Pow((height / 2) - x, 2) + Math.Pow((height / 2) - y, 2));
								float range = (float)Math.Abs(distance / (height / 2)); //
								range = range > 1 ? 1 : range;
								color = pixelsColor.GetPixel((int)(range * (colorWidth - 1)), 0).GetPixelColor();
							}

							if (mExcavate)
							{
								CheckNeighbor(ref blocks, bitmap, color, i, x, y);
							}
							else
							{
								blocks.Add(new Voxel((ushort)x, (ushort)i, (ushort)y, color.ColorToUInt()));
							}
						}
					}
				}
			}

			List<Voxel> list = Quantization.ApplyQuantization(blocks, mColorLimit);
			Schematic schematic = new Schematic(list);

			Console.WriteLine("[INFO] Done.");
			return schematic;
		}

		private void CheckNeighbor(ref List<Voxel> blocks, MagickImage bitmap, Color color, int i, int x, int y)
		{
			var pixels = bitmap.GetPixels();

			if (x - 1 >= 0 && x + 1 < bitmap.Width && y - 1 >= 0 && y + 1 < bitmap.Height)
			{
				Color left = pixels.GetPixel(x - 1, y).GetPixelColor();

				Color top = pixels.GetPixel(x, y - 1).GetPixelColor();

				Color right = pixels.GetPixel(x + 1, y).GetPixelColor();

				Color bottom = pixels.GetPixel(x, y + 1).GetPixelColor();

				bool leftColor = left != Color.Empty && left != Color.Transparent && left != Color.Black && (left.R != 0 && left.G != 0 && left.B != 0);
				bool topColor = top != Color.Empty && top != Color.Transparent && top != Color.Black && (top.R != 0 && top.G != 0 && top.B != 0);
				bool rightColor = right != Color.Empty && right != Color.Transparent && right != Color.Black && (right.R != 0 && right.G != 0 && right.B != 0);
				bool bottomColor = bottom != Color.Empty && bottom != Color.Transparent && bottom != Color.Black && (bottom.R != 0 && bottom.G != 0 && bottom.B != 0);

				if (!leftColor || !topColor || !rightColor || !bottomColor)
				{
					blocks.Add(new Voxel((ushort)x, (ushort)i, (ushort)y, color.ColorToUInt()));
				}
			}
		}
	}
}
