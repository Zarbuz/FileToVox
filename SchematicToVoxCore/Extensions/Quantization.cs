﻿using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using Color = FileToVoxCore.Drawing.Color;

namespace FileToVox.Extensions
{
	public static class Quantization
	{

		public static void Quantize(MagickImage image, QuantizeSettings settings)
		{
			if (Program.DisableQuantization())
			{
				Console.WriteLine("[WARNING] By disabling quantization, only the first 255 unique colors will be taken into account");
			}
			image.Quantize(settings);
		}

		public static List<Voxel> ApplyQuantization(List<Voxel> blocks, int colorLimit)
		{
			Quantizer.Quantizer quantizer = new Quantizer.Quantizer();
			try
			{
				if (blocks.Count == 0)
				{
					Console.WriteLine("[WARNING] No voxels to quantize, skipping this part...");
					return blocks;
				}

				Console.WriteLine("[INFO] Started quantization of all colors ...");
				using (ProgressBar progressBar = new ProgressBar())
				{
	                using (Bitmap bitmap = CreateBitmapFromColors(blocks))
					{
		                using (Bitmap quantized = quantizer.QuantizeImage(bitmap, 10, 70, colorLimit))
						{
						//Console.WriteLine(quantized.PixelFormat);
						//Bitmap reducedBitmap = new Bitmap(quantized);
			                int width = quantized.Size.Width;
						for (int i = 0; i < blocks.Count; i++)
						{
							int x = i % width;
							int y = i / width;
				                blocks[i] = new Voxel(blocks[i].X, blocks[i].Y, blocks[i].Z, quantized.GetPixel(x, y).ColorToUInt());
							progressBar.Report(i / (float)blocks.Count);
						}
					}
	                }

				}

				Console.WriteLine("[INFO] Done.");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}

			return blocks;
		}

		private static Bitmap CreateBitmapFromColors(List<Voxel> blocks)
		{
            int width = blocks.Count;

            Bitmap bitmap = new Bitmap(width, 1);

			for (int i = 0; i < blocks.Count; i++)
			{
				Voxel voxel = blocks[i];
				Color color = voxel.Color.UIntToColor();
				int x = i % width;
				int y = i / width;
                bitmap.SetPixel(x, y, color.ToSystemDrawingColor());
			}

            return bitmap;
		}
	}
}
