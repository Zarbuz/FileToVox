using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using FileToVox.Converter.Image;
using FileToVox.Extensions;
using FileToVoxCommon.Generator.Heightmap.Data;
using FileToVoxCore.Schematics;

namespace FileToVox.Utils
{
	public static class ImageUtils
	{
		public static Bitmap ConvertToFormat32(this Bitmap bitmap)
		{
			Bitmap clone = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
			using (Graphics gr = Graphics.FromImage(clone))
			{
				gr.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
			}

			return clone;
		}

		public static Schematic WriteSchematicFromImage(Bitmap bitmap, Bitmap colorBitmap, HeightmapStep heightmapStep)
		{
			if (colorBitmap != null)
			{
				if (bitmap.Height != colorBitmap.Height || bitmap.Width != colorBitmap.Width)
				{
					throw new ArgumentException("[ERROR] Image color is not the same size of the original image");
				}

				if (heightmapStep.ColorLimit != 256 || colorBitmap.CountColor() > 256)
				{
					Quantizer.Quantizer quantizer = new Quantizer.Quantizer();
					colorBitmap = quantizer.QuantizeImage(colorBitmap, 10, 70, heightmapStep.ColorLimit);
				}

			}
			else if (heightmapStep.EnableColor)
			{
				if (heightmapStep.ColorLimit != 256 || bitmap.CountColor() > 256)
				{
					Quantizer.Quantizer quantizer = new Quantizer.Quantizer();
					bitmap = quantizer.QuantizeImage(bitmap, 10, 70, heightmapStep.ColorLimit);
				}
			}

			Schematic schematic = WriteSchematicIntern(bitmap, colorBitmap, heightmapStep);
			return schematic;
		}

		public static Schematic WriteSchematicIntern(Bitmap bitmap, Bitmap bitmapColor, HeightmapStep heightmapStep)
		{
			Schematic schematic = new Schematic();

			Bitmap bitmapBlack = Grayscale.MakeGrayscale3(bitmap);
			DirectBitmap directBitmapBlack = new DirectBitmap(bitmapBlack, heightmapStep.Height);
			DirectBitmap directBitmap = new DirectBitmap(bitmap, 1);
			DirectBitmap directBitmapColor = new DirectBitmap(bitmapColor, 1);


			if (bitmap.Width > Schematic.MAX_WORLD_WIDTH || bitmap.Height > Schematic.MAX_WORLD_LENGTH)
			{
				throw new ArgumentException($"Image is too big (max size ${Schematic.MAX_WORLD_WIDTH}x${Schematic.MAX_WORLD_LENGTH} px)");
			}

			using (FileToVoxCore.Utils.ProgressBar progressbar = new FileToVoxCore.Utils.ProgressBar())
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
						Color color = directBitmap.GetPixel(x, y);
						Color finalColor = !string.IsNullOrEmpty(heightmapStep.ColorTexturePath) ? directBitmapColor.GetPixel(x, y) : (heightmapStep.EnableColor) ? color : Color.White;
						if (color.A != 0)
						{
							if (heightmapStep.Height != 1)
							{
								if (heightmapStep.Excavate)
								{
									GenerateFromMinNeighbor(ref schematic, directBitmapBlack, w, h, finalColor, x, y, heightmapStep.Height, heightmapStep.Offset, heightmapStep.RotationMode, heightmapStep.Reverse);
								}
								else
								{
									int computeHeight = directBitmapBlack.GetHeight(x, y) + heightmapStep.Offset;
									AddMultipleBlocks(ref schematic, heightmapStep.Offset, computeHeight, x, y, finalColor, heightmapStep.RotationMode);
								}
							}
							else
							{
								AddSingleVoxel(ref schematic, x, heightmapStep.Offset, y, finalColor, heightmapStep.RotationMode, heightmapStep.Reverse);
							}
						}
						progressbar.Report((i++ / (float)size));
					}
				}
			}

			Console.WriteLine("[INFO] Done.");
			return schematic;
		}

		public static void AddMultipleBlocks(ref Schematic schematic, int min, int max, int x, int y, Color color, RotationMode rotationMode)
		{
			switch (rotationMode)
			{
				case RotationMode.X:
					for (int i = min; i < max; i++)
					{
						AddBlock(ref schematic, new Voxel((ushort)i, (ushort)y, (ushort)x, color.ColorToUInt()));
					}
					break;
				case RotationMode.Y:
					for (int i = min; i < max; i++)
					{
						AddBlock(ref schematic, new Voxel((ushort)x, (ushort)i, (ushort)y, color.ColorToUInt()));
					}
					break;
				case RotationMode.Z:
					for (int i = min; i < max; i++)
					{
						AddBlock(ref schematic, new Voxel((ushort)y, (ushort)x, (ushort)i, color.ColorToUInt()));
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(rotationMode), rotationMode, null);
			}
		}

		public static void AddSingleVoxel(ref Schematic schematic, int x, int y, int z, Color color, RotationMode rotationMode, bool reverse)
		{
			Voxel voxel;
			ushort finalY;
			ushort finalX;
			ushort finalZ;
			switch (rotationMode)
			{
				case RotationMode.X:
					finalY = (ushort)(!reverse ? y : Schematic.MAX_WORLD_WIDTH - y);
					finalX = (ushort)(x);
					finalZ = (ushort)(z);
					voxel = new Voxel(finalY, finalZ, finalX, color.ColorToUInt());
					break;
				case RotationMode.Y: //historic 
					finalX = (ushort)(x);
					finalY = (ushort)(!reverse ? y : Schematic.MAX_WORLD_HEIGHT - y);
					finalZ = (ushort)(z);
					voxel = new Voxel(finalX, finalY, finalZ, color.ColorToUInt());
					break;
				case RotationMode.Z:
					finalX = (ushort)(x);
					finalY = (ushort)(!reverse ? y : Schematic.MAX_WORLD_LENGTH - y);
					finalZ = (ushort)(z);
					voxel = new Voxel(finalZ, finalX, finalY, color.ColorToUInt());
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(rotationMode), rotationMode, null);
			}
			AddBlock(ref schematic, voxel);

		}

		public static void AddBlock(ref Schematic schematic, Voxel voxel)
		{
			schematic.AddVoxel(voxel);
		}

		public static void GenerateFromMinNeighbor(ref Schematic schematic, DirectBitmap blackBitmap, int w, int h, Color color, int x, int y, int height, int offset, RotationMode rotationMode, bool reverse)
		{
			int computeHeight = blackBitmap.GetHeight(x, y) + offset;
			try
			{
				if (x - 1 >= 0 && x + 1 < w && y - 1 >= 0 && y + 1 < h)
				{
					int heightLeft = blackBitmap.GetHeight(x - 1, y) + offset;
					int heightTop = blackBitmap.GetHeight(x, y - 1) + offset;
					int heightRight = blackBitmap.GetHeight(x + 1, y) + offset;
					int heightBottom = blackBitmap.GetHeight(x, y + 1) + offset;

					var list = new List<int>
						{
							heightLeft, heightTop, heightRight, heightBottom
						};

					int min = list.Min();
					if (min < computeHeight)
					{
						AddMultipleBlocks(ref schematic, min, computeHeight, x, y, color, rotationMode);
					}
					else
					{
						int finalHeight = (computeHeight - 1 < 0) ? 0 : computeHeight - 1;
						AddSingleVoxel(ref schematic, x, finalHeight, y, color, rotationMode, reverse);
					}
				}
				else
				{
					AddMultipleBlocks(ref schematic, offset, computeHeight, x, y, color, rotationMode);
				}
			}
			catch (IndexOutOfRangeException)
			{
				Console.WriteLine($"[ERROR] x: {x}, y: {y}, schematic width: {schematic.Width}, schematic length: {schematic.Length}");
			}
		}
	}
}
