﻿using FileToVox.Extensions;
using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Linq;
using Color = System.Drawing.Color;

namespace FileToVox.Utils
{
	public static class ImageUtils
	{
		#region PublicMethods

		public static Schematic WriteSchematicFromImage(MagickImage bitmap, MagickImage colorBitmap, LoadImageParam loadImageParam)
		{
			if (colorBitmap != null)
			{
				if (bitmap.Height != colorBitmap.Height || bitmap.Width != colorBitmap.Width)
				{
					throw new ArgumentException("[ERROR] Image color is not the same size of the original image");
				}

				if (loadImageParam.ColorLimit != 256 || colorBitmap.UniqueColors().TotalColors > 256)
				{
					colorBitmap.Quantize(new QuantizeSettings()
					{
						Colors = loadImageParam.ColorLimit,
					});
				}

			}
			else if (loadImageParam.EnableColor)
			{
				bitmap.Quantize(new QuantizeSettings()
				{
					Colors = loadImageParam.ColorLimit
				});

			}

			Schematic schematic = WriteSchematicIntern(bitmap, colorBitmap, loadImageParam);
			return schematic;
		}

		#endregion

		#region PrivateMethods

		private static Schematic WriteSchematicIntern(MagickImage bitmap, MagickImage bitmapColor, LoadImageParam loadImageParam)
		{
			Schematic schematic = new Schematic();

			MagickImage grayscale = new MagickImage(loadImageParam.TexturePath);
			grayscale.Grayscale();
			int depth = grayscale.Depth;

			IPixelCollection<ushort> pixelCollectionBitmap = bitmap.GetPixels();
			IPixelCollection<ushort> pixelCollectionGrayscale = grayscale.GetPixels();
			IPixelCollection<ushort> pixelCollectionBitmapColor = bitmapColor != null ? bitmapColor.GetPixels() : null;
			//DirectBitmap directBitmapBlack = new DirectBitmap(bitmapBlack, heightmapStep.Height);
			//DirectBitmap directBitmap = new DirectBitmap(bitmap, 1);
			//DirectBitmap directBitmapColor = new DirectBitmap(bitmapColor, 1);


			if (bitmap.Width > Schematic.MAX_WORLD_WIDTH || bitmap.Height > Schematic.MAX_WORLD_LENGTH)
			{
				throw new ArgumentException($"Image is too big (max size ${Schematic.MAX_WORLD_WIDTH}x${Schematic.MAX_WORLD_LENGTH} px)");
			}

			using (ProgressBar progressbar = new ProgressBar())
			{
				Console.WriteLine("[INFO] Started to write schematic from picture...");
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
						IPixel<ushort> pixel = pixelCollectionBitmap.GetPixel(x, y);
						Color color = Color.FromArgb(pixel.GetChannel(4), pixel.GetChannel(0), pixel.GetChannel(1), pixel.GetChannel(2));
						Color finalColor;

						if (pixelCollectionBitmapColor != null)
						{
							IPixel<ushort> p = pixelCollectionBitmapColor.GetPixel(x, y);
							Color c = Color.FromArgb(p.GetChannel(4), p.GetChannel(0), p.GetChannel(1), p.GetChannel(2));
							finalColor = c;
						}
						else if (loadImageParam.EnableColor)
						{
							finalColor = color;
						}
						else
						{
							finalColor = Color.White;
						}

						if (color.A != 0)
						{
							if (loadImageParam.Height != 1)
							{
								if (loadImageParam.Excavate)
								{
									GenerateFromMinNeighborParam param = new()
									{
										X = x,
										Y = y,
										PixelCollection = pixelCollectionGrayscale,
										GrayscaleImage = grayscale,
										Color = finalColor,
										Height = loadImageParam.Height
									};

									GenerateFromMinNeighbor(ref schematic, param);
								}
								else
								{
									int computeHeight = GetHeight(depth, pixelCollectionGrayscale.GetPixel(x, y), loadImageParam.Height);
									AddMultipleVoxels(ref schematic, h, computeHeight, x, y, finalColor);
								}
							}
							else
							{
								AddSingleVoxel(ref schematic, x, h, y, finalColor);
							}
						}
						progressbar.Report((i++ / (float)size));
					}
				}
			}

			Console.WriteLine("[INFO] Done.");
			return schematic;
		}

		private static void AddMultipleVoxels(ref Schematic schematic, int min, int max, int x, int y, Color color)
		{
			for (int i = min; i < max; i++)
			{
				schematic.AddVoxel(new Voxel((ushort)x, (ushort)i, (ushort)y, color.ColorToUInt()));
			}
		}

		private static void AddSingleVoxel(ref Schematic schematic, int x, int y, int z, Color color)
		{
			ushort finalX = (ushort)(x);
			ushort finalY = (ushort)(y);
			ushort finalZ = (ushort)(z);
			Voxel voxel = new(finalX, finalY, finalZ, color.ColorToUInt());

			schematic.AddVoxel(voxel);
		}

		private static void GenerateFromMinNeighbor(ref Schematic schematic, GenerateFromMinNeighborParam param)
		{
			int depth = param.GrayscaleImage.Depth;

			int computeHeight = GetHeight(depth, param.PixelCollection.GetPixel(param.X, param.Y), param.Height);
			if (param.X - 1 >= 0 && param.X + 1 < param.GrayscaleImage.Width && param.Y - 1 >= 0 && param.Y + 1 < param.GrayscaleImage.Height)
			{
				int heightLeft = GetHeight(depth, param.PixelCollection.GetPixel(param.X - 1, param.Y), param.Height);
				int heightTop = GetHeight(depth, param.PixelCollection.GetPixel(param.X, param.Y - 1), param.Height);
				int heightRight = GetHeight(depth, param.PixelCollection.GetPixel(param.X + 1, param.Y), param.Height);
				int heightBottom = GetHeight(depth, param.PixelCollection.GetPixel(param.X, param.Y + 1), param.Height);

				var list = new List<int>
				{
					heightLeft, heightTop, heightRight, heightBottom
				};

				int min = list.Min();
				if (min < computeHeight)
				{
					AddMultipleVoxels(ref schematic, min, computeHeight, param.X, param.Y, param.Color);
				}
				else
				{
					int finalHeight = (computeHeight - 1 < 0) ? 0 : computeHeight - 1;
					AddSingleVoxel(ref schematic, param.X, finalHeight, param.Y, param.Color);
				}
			}
			else
			{
				AddMultipleVoxels(ref schematic, 0, computeHeight, param.X, param.Y, param.Color);
			}
		}

		private static int GetHeight(int depth, IPixel<ushort> pixel, int heightFactor)
		{
			int average = (depth == 48 ? 196605 : 765);
			float intensity = (pixel.GetChannel(0) + pixel.GetChannel(1) + pixel.GetChannel(2)) / (float)average;
			int height = (int)(intensity * heightFactor);
			return height;
		}
		

		#endregion
	}

	public class GenerateFromMinNeighborParam
	{
		public MagickImage GrayscaleImage;
		public IPixelCollection<ushort> PixelCollection;
		public Color Color;
		public int X;
		public int Y;
		public int Height;
	}

	public class LoadImageParam
	{
		public int Height { get; set; }
		public bool Excavate { get; set; }
		public bool EnableColor { get; set; }
		public string TexturePath { get; set; }
		public string ColorTexturePath { get; set; }
		public int ColorLimit { get; set; }
	}
}
