using FileToVox.Utils;
using nQuant;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace FileToVox.Quantizer
{
	public abstract class QuantizerBase
	{
		private int mMaxColor = 256;

		public Bitmap QuantizeImage(Bitmap image, int alphaThreshold, int alphaFader, int maxColorCount)
		{
			if (image.PixelFormat != PixelFormat.Format32bppArgb)
			{
				image = image.ConvertToFormat32();
			}

			mMaxColor = maxColorCount + 1;
			mMaxColor = Math.Min(mMaxColor, 256);
			int colorCount = mMaxColor;
			ColorData moments = QuantizerBase.CalculateMoments(QuantizerBase.BuildHistogram(image, alphaThreshold, alphaFader));
			IEnumerable<Box> cubes = this.SplitData(ref colorCount, moments);
			QuantizedPalette quantizedPalette = this.GetQuantizedPalette(colorCount, moments, cubes, alphaThreshold);
			return ProcessImagePixels((Image)image, quantizedPalette);
		}

		private static Bitmap ProcessImagePixels(Image sourceImage, QuantizedPalette palette)
		{
			Bitmap bitmap = new Bitmap(sourceImage.Width, sourceImage.Height, PixelFormat.Format8bppIndexed);
			ColorPalette palette1 = bitmap.Palette;
			for (int index = 0; index < palette.Colors.Count; ++index)
				palette1.Entries[index] = palette.Colors[index];
			bitmap.Palette = palette1;
			BitmapData bitmapdata = (BitmapData)null;
			try
			{
				bitmapdata = bitmap.LockBits(Rectangle.FromLTRB(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
				int num1 = bitmapdata.Stride < 0 ? -bitmapdata.Stride : bitmapdata.Stride;
				int length1 = Math.Max(1, 1);
				int length2 = num1 * bitmap.Height;
				int num2 = 0;
				byte[] source = new byte[length2];
				byte[] numArray = new byte[length1];
				int index1 = 0;
				for (int index2 = 0; index2 < bitmap.Height; ++index2)
				{
					int num3 = 0;
					for (int index3 = 0; index3 < bitmap.Width; ++index3)
					{
						int num4 = num3 >> 3;
						numArray[0] = palette.PixelIndex[index1] == -1 ? (byte)(palette.Colors.Count - 1) : (byte)palette.PixelIndex[index1];
						++index1;
						for (int index4 = 0; index4 < length1; ++index4)
							source[num2 + index4 + num4] = numArray[index4];
						num3 += 8;
					}
					num2 += num1;
				}
				Marshal.Copy(source, 0, bitmapdata.Scan0, length2);
			}
			finally
			{
				if (bitmapdata != null)
					bitmap.UnlockBits(bitmapdata);
			}
			return bitmap;
		}

		private static ColorData BuildHistogram(
		  Bitmap sourceImage,
		  int alphaThreshold,
		  int alphaFader)
		{
			int width = sourceImage.Width;
			int height = sourceImage.Height;
			BitmapData bitmapdata = sourceImage.LockBits(Rectangle.FromLTRB(0, 0, width, height), ImageLockMode.ReadOnly, sourceImage.PixelFormat);
			ColorData colorData = new ColorData(32, width, height);
			try
			{
				int pixelFormatSize = Image.GetPixelFormatSize(sourceImage.PixelFormat);
				if (pixelFormatSize != 32)
					throw new QuantizationException(string.Format("Thie image you are attempting to quantize does not contain a 32 bit ARGB palette. This image has a bit depth of {0} with {1} colors.", (object)pixelFormatSize, (object)sourceImage.Palette.Entries.Length));
				int num1 = bitmapdata.Stride < 0 ? -bitmapdata.Stride : bitmapdata.Stride;
				int length = Math.Max(1, pixelFormatSize >> 3);
				int num2 = 0;
				byte[] destination = new byte[num1 * sourceImage.Height];
				byte[] numArray = new byte[length];
				Marshal.Copy(bitmapdata.Scan0, destination, 0, destination.Length);
				for (int index1 = 0; index1 < height; ++index1)
				{
					int num3 = 0;
					for (int index2 = 0; index2 < width; ++index2)
					{
						int num4 = num3 >> 3;
						for (int index3 = 0; index3 < length; ++index3)
							numArray[index3] = destination[num2 + index3 + num4];
						byte num5 = (byte)(((int)numArray[3] >> 3) + 1);
						byte num6 = (byte)(((int)numArray[2] >> 3) + 1);
						byte num7 = (byte)(((int)numArray[1] >> 3) + 1);
						byte num8 = (byte)(((int)numArray[0] >> 3) + 1);
						if ((int)numArray[3] > alphaThreshold)
						{
							if (numArray[3] < byte.MaxValue)
							{
								int num9 = (int)numArray[3] + (int)numArray[3] % alphaFader;
								numArray[3] = num9 > (int)byte.MaxValue ? byte.MaxValue : (byte)num9;
								num5 = (byte)(((int)numArray[3] >> 3) + 1);
							}
							++colorData.Weights[(int)num5, (int)num6, (int)num7, (int)num8];
							colorData.MomentsRed[(int)num5, (int)num6, (int)num7, (int)num8] += (long)numArray[2];
							colorData.MomentsGreen[(int)num5, (int)num6, (int)num7, (int)num8] += (long)numArray[1];
							colorData.MomentsBlue[(int)num5, (int)num6, (int)num7, (int)num8] += (long)numArray[0];
							colorData.MomentsAlpha[(int)num5, (int)num6, (int)num7, (int)num8] += (long)numArray[3];
							colorData.Moments[(int)num5, (int)num6, (int)num7, (int)num8] += (float)((int)numArray[3] * (int)numArray[3] + (int)numArray[2] * (int)numArray[2] + (int)numArray[1] * (int)numArray[1] + (int)numArray[0] * (int)numArray[0]);
						}
						colorData.AddPixel(new Pixel(numArray[3], numArray[2], numArray[1], numArray[0]), BitConverter.ToInt32(new byte[4]
						{
			  num5,
			  num6,
			  num7,
			  num8
						}, 0));
						num3 += pixelFormatSize;
					}
					num2 += num1;
				}
			}
			finally
			{
				sourceImage.UnlockBits(bitmapdata);
			}
			return colorData;
		}

		private static ColorData CalculateMoments(ColorData data)
		{
			for (int index1 = 1; index1 <= 32; ++index1)
			{
				long[,,] numArray1 = new long[33, 33, 33];
				long[,,] numArray2 = new long[33, 33, 33];
				long[,,] numArray3 = new long[33, 33, 33];
				long[,,] numArray4 = new long[33, 33, 33];
				long[,,] numArray5 = new long[33, 33, 33];
				float[,,] numArray6 = new float[33, 33, 33];
				for (int index2 = 1; index2 <= 32; ++index2)
				{
					long[] numArray7 = new long[33];
					long[] numArray8 = new long[33];
					long[] numArray9 = new long[33];
					long[] numArray10 = new long[33];
					long[] numArray11 = new long[33];
					float[] numArray12 = new float[33];
					for (int index3 = 1; index3 <= 32; ++index3)
					{
						long num1 = 0;
						long num2 = 0;
						long num3 = 0;
						long num4 = 0;
						long num5 = 0;
						float num6 = 0.0f;
						for (int index4 = 1; index4 <= 32; ++index4)
						{
							num1 += data.Weights[index1, index2, index3, index4];
							num2 += data.MomentsAlpha[index1, index2, index3, index4];
							num3 += data.MomentsRed[index1, index2, index3, index4];
							num4 += data.MomentsGreen[index1, index2, index3, index4];
							num5 += data.MomentsBlue[index1, index2, index3, index4];
							num6 += data.Moments[index1, index2, index3, index4];
							numArray7[index4] += num1;
							numArray8[index4] += num2;
							numArray9[index4] += num3;
							numArray10[index4] += num4;
							numArray11[index4] += num5;
							numArray12[index4] += num6;
							numArray1[index2, index3, index4] = numArray1[index2 - 1, index3, index4] + numArray7[index4];
							numArray2[index2, index3, index4] = numArray2[index2 - 1, index3, index4] + numArray8[index4];
							numArray3[index2, index3, index4] = numArray3[index2 - 1, index3, index4] + numArray9[index4];
							numArray4[index2, index3, index4] = numArray4[index2 - 1, index3, index4] + numArray10[index4];
							numArray5[index2, index3, index4] = numArray5[index2 - 1, index3, index4] + numArray11[index4];
							numArray6[index2, index3, index4] = numArray6[index2 - 1, index3, index4] + numArray12[index4];
							data.Weights[index1, index2, index3, index4] = data.Weights[index1 - 1, index2, index3, index4] + numArray1[index2, index3, index4];
							data.MomentsAlpha[index1, index2, index3, index4] = data.MomentsAlpha[index1 - 1, index2, index3, index4] + numArray2[index2, index3, index4];
							data.MomentsRed[index1, index2, index3, index4] = data.MomentsRed[index1 - 1, index2, index3, index4] + numArray3[index2, index3, index4];
							data.MomentsGreen[index1, index2, index3, index4] = data.MomentsGreen[index1 - 1, index2, index3, index4] + numArray4[index2, index3, index4];
							data.MomentsBlue[index1, index2, index3, index4] = data.MomentsBlue[index1 - 1, index2, index3, index4] + numArray5[index2, index3, index4];
							data.Moments[index1, index2, index3, index4] = data.Moments[index1 - 1, index2, index3, index4] + numArray6[index2, index3, index4];
						}
					}
				}
			}
			return data;
		}

		private static long Top(Box cube, int direction, int position, long[,,,] moment)
		{
			switch (direction)
			{
				case 0:
					return moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMaximum, position] - moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMinimum, position] - moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMaximum, position] + moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMinimum, position] - (moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMaximum, position] - moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMinimum, position] - moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMaximum, position] + moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMinimum, position]);
				case 1:
					return moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, position, (int)cube.BlueMaximum] - moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, position, (int)cube.BlueMaximum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, position, (int)cube.BlueMaximum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, position, (int)cube.BlueMaximum] - (moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, position, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, position, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, position, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, position, (int)cube.BlueMinimum]);
				case 2:
					return moment[(int)cube.AlphaMaximum, position, (int)cube.GreenMaximum, (int)cube.BlueMaximum] - moment[(int)cube.AlphaMaximum, position, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - moment[(int)cube.AlphaMinimum, position, (int)cube.GreenMaximum, (int)cube.BlueMaximum] + moment[(int)cube.AlphaMinimum, position, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - (moment[(int)cube.AlphaMaximum, position, (int)cube.GreenMaximum, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMaximum, position, (int)cube.GreenMinimum, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMinimum, position, (int)cube.GreenMaximum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMinimum, position, (int)cube.GreenMinimum, (int)cube.BlueMinimum]);
				case 3:
					return moment[position, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] - moment[position, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - moment[position, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] + moment[position, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - (moment[position, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] - moment[position, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] - moment[position, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] + moment[position, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMinimum]);
				default:
					return 0;
			}
		}

		private static long Bottom(Box cube, int direction, long[,,,] moment)
		{
			switch (direction)
			{
				case 0:
					return -moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] - (-moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMinimum]);
				case 1:
					return -moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] + moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - (-moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMinimum]);
				case 2:
					return -moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] + moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - (-moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMinimum]);
				case 3:
					return -moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - (-moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMinimum]);
				default:
					return 0;
			}
		}

		private static CubeCut Maximize(
		  ColorData data,
		  Box cube,
		  int direction,
		  byte first,
		  byte last,
		  long wholeAlpha,
		  long wholeRed,
		  long wholeGreen,
		  long wholeBlue,
		  long wholeWeight)
		{
			long num1 = QuantizerBase.Bottom(cube, direction, data.MomentsAlpha);
			long num2 = QuantizerBase.Bottom(cube, direction, data.MomentsRed);
			long num3 = QuantizerBase.Bottom(cube, direction, data.MomentsGreen);
			long num4 = QuantizerBase.Bottom(cube, direction, data.MomentsBlue);
			long num5 = QuantizerBase.Bottom(cube, direction, data.Weights);
			float result = 0.0f;
			byte? cutPoint = new byte?();
			for (byte index = first; (int)index < (int)last; ++index)
			{
				long num6 = num1 + QuantizerBase.Top(cube, direction, (int)index, data.MomentsAlpha);
				long num7 = num2 + QuantizerBase.Top(cube, direction, (int)index, data.MomentsRed);
				long num8 = num3 + QuantizerBase.Top(cube, direction, (int)index, data.MomentsGreen);
				long num9 = num4 + QuantizerBase.Top(cube, direction, (int)index, data.MomentsBlue);
				long num10 = num5 + QuantizerBase.Top(cube, direction, (int)index, data.Weights);
				if (num10 != 0L)
				{
					long num11 = (num6 * num6 + num7 * num7 + num8 * num8 + num9 * num9) / num10;
					long num12 = wholeAlpha - num6;
					long num13 = wholeRed - num7;
					long num14 = wholeGreen - num8;
					long num15 = wholeBlue - num9;
					long num16 = wholeWeight - num10;
					if (num16 != 0L)
					{
						long num17 = num12 * num12 + num13 * num13 + num14 * num14 + num15 * num15;
						long num18 = num11 + num17 / num16;
						if ((double)num18 > (double)result)
						{
							result = (float)num18;
							cutPoint = new byte?(index);
						}
					}
				}
			}
			return new CubeCut(cutPoint, result);
		}

		private bool Cut(ColorData data, ref Box first, ref Box second)
		{
			long wholeAlpha = QuantizerBase.Volume(first, data.MomentsAlpha);
			long wholeRed = QuantizerBase.Volume(first, data.MomentsRed);
			long wholeGreen = QuantizerBase.Volume(first, data.MomentsGreen);
			long wholeBlue = QuantizerBase.Volume(first, data.MomentsBlue);
			long wholeWeight = QuantizerBase.Volume(first, data.Weights);
			CubeCut cubeCut1 = QuantizerBase.Maximize(data, first, 3, (byte)((uint)first.AlphaMinimum + 1U), first.AlphaMaximum, wholeAlpha, wholeRed, wholeGreen, wholeBlue, wholeWeight);
			CubeCut cubeCut2 = QuantizerBase.Maximize(data, first, 2, (byte)((uint)first.RedMinimum + 1U), first.RedMaximum, wholeAlpha, wholeRed, wholeGreen, wholeBlue, wholeWeight);
			CubeCut cubeCut3 = QuantizerBase.Maximize(data, first, 1, (byte)((uint)first.GreenMinimum + 1U), first.GreenMaximum, wholeAlpha, wholeRed, wholeGreen, wholeBlue, wholeWeight);
			CubeCut cubeCut4 = QuantizerBase.Maximize(data, first, 0, (byte)((uint)first.BlueMinimum + 1U), first.BlueMaximum, wholeAlpha, wholeRed, wholeGreen, wholeBlue, wholeWeight);
			int num1;
			if ((double)cubeCut1.Value >= (double)cubeCut2.Value && (double)cubeCut1.Value >= (double)cubeCut3.Value && (double)cubeCut1.Value >= (double)cubeCut4.Value)
			{
				num1 = 3;
				byte? position = cubeCut1.Position;
				if (!(position.HasValue ? new int?((int)position.GetValueOrDefault()) : new int?()).HasValue)
					return false;
			}
			else
				num1 = (double)cubeCut2.Value < (double)cubeCut1.Value || (double)cubeCut2.Value < (double)cubeCut3.Value || (double)cubeCut2.Value < (double)cubeCut4.Value ? ((double)cubeCut3.Value < (double)cubeCut1.Value || (double)cubeCut3.Value < (double)cubeCut2.Value || (double)cubeCut3.Value < (double)cubeCut4.Value ? 0 : 1) : 2;
			second.AlphaMaximum = first.AlphaMaximum;
			second.RedMaximum = first.RedMaximum;
			second.GreenMaximum = first.GreenMaximum;
			second.BlueMaximum = first.BlueMaximum;
			switch (num1)
			{
				case 0:
					ref Box local1 = ref second;
					ref Box local2 = ref first;
					byte? position1 = cubeCut4.Position;
					int num2;
					byte num3 = (byte)(num2 = (int)position1.Value);
					local2.BlueMaximum = (byte)num2;
					int num4 = (int)num3;
					local1.BlueMinimum = (byte)num4;
					second.AlphaMinimum = first.AlphaMinimum;
					second.RedMinimum = first.RedMinimum;
					second.GreenMinimum = first.GreenMinimum;
					break;
				case 1:
					ref Box local3 = ref second;
					ref Box local4 = ref first;
					byte? position2 = cubeCut3.Position;
					int num5;
					byte num6 = (byte)(num5 = (int)position2.Value);
					local4.GreenMaximum = (byte)num5;
					int num7 = (int)num6;
					local3.GreenMinimum = (byte)num7;
					second.AlphaMinimum = first.AlphaMinimum;
					second.RedMinimum = first.RedMinimum;
					second.BlueMinimum = first.BlueMinimum;
					break;
				case 2:
					ref Box local5 = ref second;
					ref Box local6 = ref first;
					byte? position3 = cubeCut2.Position;
					int num8;
					byte num9 = (byte)(num8 = (int)position3.Value);
					local6.RedMaximum = (byte)num8;
					int num10 = (int)num9;
					local5.RedMinimum = (byte)num10;
					second.AlphaMinimum = first.AlphaMinimum;
					second.GreenMinimum = first.GreenMinimum;
					second.BlueMinimum = first.BlueMinimum;
					break;
				case 3:
					ref Box local7 = ref second;
					ref Box local8 = ref first;
					byte? position4 = cubeCut1.Position;
					int num11;
					byte num12 = (byte)(num11 = (int)position4.Value);
					local8.AlphaMaximum = (byte)num11;
					int num13 = (int)num12;
					local7.AlphaMinimum = (byte)num13;
					second.RedMinimum = first.RedMinimum;
					second.GreenMinimum = first.GreenMinimum;
					second.BlueMinimum = first.BlueMinimum;
					break;
			}
			first.Size = ((int)first.AlphaMaximum - (int)first.AlphaMinimum) * ((int)first.RedMaximum - (int)first.RedMinimum) * ((int)first.GreenMaximum - (int)first.GreenMinimum) * ((int)first.BlueMaximum - (int)first.BlueMinimum);
			second.Size = ((int)second.AlphaMaximum - (int)second.AlphaMinimum) * ((int)second.RedMaximum - (int)second.RedMinimum) * ((int)second.GreenMaximum - (int)second.GreenMinimum) * ((int)second.BlueMaximum - (int)second.BlueMinimum);
			return true;
		}

		private static float CalculateVariance(ColorData data, Box cube)
		{
			float num1 = (float)QuantizerBase.Volume(cube, data.MomentsAlpha);
			float num2 = (float)QuantizerBase.Volume(cube, data.MomentsRed);
			float num3 = (float)QuantizerBase.Volume(cube, data.MomentsGreen);
			float num4 = (float)QuantizerBase.Volume(cube, data.MomentsBlue);
			float num5 = QuantizerBase.VolumeFloat(cube, data.Moments);
			float num6 = (float)QuantizerBase.Volume(cube, data.Weights);
			float num7 = (float)((double)num1 * (double)num1 + (double)num2 * (double)num2 + (double)num3 * (double)num3 + (double)num4 * (double)num4);
			float num8 = num5 - num7 / num6;
			if (!double.IsNaN((double)num8))
				return num8;
			return 0.0f;
		}

		private static long Volume(Box cube, long[,,,] moment)
		{
			return moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] - moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] + moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - (moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] + moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] - moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMinimum]);
		}

		private static float VolumeFloat(Box cube, float[,,,] moment)
		{
			return (float)((double)moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] - (double)moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - (double)moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] + (double)moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - (double)moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] + (double)moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] + (double)moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMaximum] - (double)moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMaximum] - ((double)moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] - (double)moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] - (double)moment[(int)cube.AlphaMaximum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] + (double)moment[(int)cube.AlphaMinimum, (int)cube.RedMaximum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] - (double)moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] + (double)moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMaximum, (int)cube.BlueMinimum] + (double)moment[(int)cube.AlphaMaximum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMinimum] - (double)moment[(int)cube.AlphaMinimum, (int)cube.RedMinimum, (int)cube.GreenMinimum, (int)cube.BlueMinimum]));
		}

		private IEnumerable<Box> SplitData(ref int colorCount, ColorData data)
		{
			--colorCount;
			int index1 = 0;
			float[] numArray = new float[mMaxColor];
			Box[] boxArray = new Box[mMaxColor];
			boxArray[0].AlphaMaximum = (byte)32;
			boxArray[0].RedMaximum = (byte)32;
			boxArray[0].GreenMaximum = (byte)32;
			boxArray[0].BlueMaximum = (byte)32;
			for (int index2 = 1; index2 < colorCount; ++index2)
			{
				if (this.Cut(data, ref boxArray[index1], ref boxArray[index2]))
				{
					numArray[index1] = boxArray[index1].Size > 1 ? QuantizerBase.CalculateVariance(data, boxArray[index1]) : 0.0f;
					numArray[index2] = boxArray[index2].Size > 1 ? QuantizerBase.CalculateVariance(data, boxArray[index2]) : 0.0f;
				}
				else
				{
					numArray[index1] = 0.0f;
					--index2;
				}
				index1 = 0;
				float num = numArray[0];
				for (int index3 = 1; index3 <= index2; ++index3)
				{
					if ((double)numArray[index3] > (double)num)
					{
						num = numArray[index3];
						index1 = index3;
					}
				}
				if ((double)num <= 0.0)
				{
					colorCount = index2 + 1;
					break;
				}
			}
			return (IEnumerable<Box>)((IEnumerable<Box>)boxArray).Take<Box>(colorCount).ToList<Box>();
		}

		protected LookupData BuildLookups(IEnumerable<Box> cubes, ColorData data)
		{
			LookupData lookupData = new LookupData(33);
			int count = lookupData.Lookups.Count;
			foreach (Box cube in cubes)
			{
				for (byte index1 = (byte)((uint)cube.AlphaMinimum + 1U); (int)index1 <= (int)cube.AlphaMaximum; ++index1)
				{
					for (byte index2 = (byte)((uint)cube.RedMinimum + 1U); (int)index2 <= (int)cube.RedMaximum; ++index2)
					{
						for (byte index3 = (byte)((uint)cube.GreenMinimum + 1U); (int)index3 <= (int)cube.GreenMaximum; ++index3)
						{
							for (byte index4 = (byte)((uint)cube.BlueMinimum + 1U); (int)index4 <= (int)cube.BlueMaximum; ++index4)
								lookupData.Tags[(int)index1, (int)index2, (int)index3, (int)index4] = count;
						}
					}
				}
				long num = QuantizerBase.Volume(cube, data.Weights);
				if (num > 0L)
				{
					Lookup lookup = new Lookup()
					{
						Alpha = (int)(QuantizerBase.Volume(cube, data.MomentsAlpha) / num),
						Red = (int)(QuantizerBase.Volume(cube, data.MomentsRed) / num),
						Green = (int)(QuantizerBase.Volume(cube, data.MomentsGreen) / num),
						Blue = (int)(QuantizerBase.Volume(cube, data.MomentsBlue) / num)
					};
					lookupData.Lookups.Add(lookup);
				}
			}
			return lookupData;
		}

		protected abstract QuantizedPalette GetQuantizedPalette(
		  int colorCount,
		  ColorData data,
		  IEnumerable<Box> cubes,
		  int alphaThreshold);
	}
}
