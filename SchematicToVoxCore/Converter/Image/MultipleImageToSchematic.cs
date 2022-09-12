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
			Bitmap bitmapColor = null;
			if (mInputColorFile != null)
			{
				bitmapColor = new Bitmap(mInputColorFile);
				if (bitmapColor.Width > 256 || bitmapColor.Height > 1)
				{
					throw new ArgumentException("[ERROR] The input color file must have a dimension of 256x1 px");
				}
			}

			for (int i = 0; i < mImages.Count; i++)
			{
				string file = mImages[i];
				Console.WriteLine("[INFO] Reading file: " + file);
				MagickImage bitmap = new MagickImage(file);
				var pixels = bitmap.GetPixels();

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
								color = bitmapColor.GetPixel((int)(range * (bitmapColor.Width - 1)), 0);
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
				IPixel<ushort> pixelLeft = pixels.GetPixel(x - 1, y);
				Color left = Color.FromArgb(pixelLeft.GetChannel(4), pixelLeft.GetChannel(0), pixelLeft.GetChannel(1), pixelLeft.GetChannel(2));

				IPixel<ushort> pixelTop = pixels.GetPixel(x , y -1);
				Color top = Color.FromArgb(pixelTop.GetChannel(4), pixelTop.GetChannel(0), pixelTop.GetChannel(1), pixelTop.GetChannel(2));

				IPixel<ushort> pixelRight = pixels.GetPixel(x + 1, y);
				Color right = Color.FromArgb(pixelRight.GetChannel(4), pixelRight.GetChannel(0), pixelRight.GetChannel(1), pixelRight.GetChannel(2));

				IPixel<ushort> pixelBottom = pixels.GetPixel(x, y + 1);
				Color bottom = Color.FromArgb(pixelBottom.GetChannel(4), pixelBottom.GetChannel(0), pixelBottom.GetChannel(1), pixelBottom.GetChannel(2));

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
