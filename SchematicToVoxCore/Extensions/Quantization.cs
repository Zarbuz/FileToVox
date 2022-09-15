using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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

		public static List<Voxel> ApplyQuantization(List<Voxel> voxels, int colorLimit)
		{
			if (voxels.Count == 0)
			{
				Console.WriteLine("[WARNING] No voxels to quantize, skipping this part...");
				return voxels;
			}

			if (Program.DisableQuantization())
			{
				Console.WriteLine("[WARNING] By disabling quantization, only the first 255 unique colors will be taken into account");
				return voxels;
			}

			colorLimit = Math.Min(colorLimit, 256);
			Console.WriteLine("[INFO] Started quantization of all colors ...");
			using (ProgressBar progressBar = new ProgressBar())
			{

				Dictionary<Color, int> histo = new Dictionary<Color, int>();
				foreach (Color color in voxels.Select(voxel => voxel.Color.UIntToColor()))
				{
					if (histo.ContainsKey(color))
					{
						histo[color]++;
					}
					else
					{
						histo[color] = 1;
					}
				}

				IOrderedEnumerable<KeyValuePair<Color, int>> result1 = histo.OrderByDescending(a => a.Value);
				List<Color> mostUsedColor = result1.Select(x => x.Key).Take(colorLimit).ToList();
				double temp;
				Dictionary<Color, Double> dist = new Dictionary<Color, double>();
				Dictionary<Color, Color> mapping = new Dictionary<Color, Color>();
				foreach (KeyValuePair<Color, int> p in result1)
				{
					dist.Clear();
					foreach (Color pp in mostUsedColor)
					{
						temp = Math.Abs(p.Key.R - pp.R) +
							   Math.Abs(p.Key.G - pp.G) +
							   Math.Abs(p.Key.B - pp.B);
						dist.Add(pp, temp);
					}
					var min = dist.OrderBy(k => k.Value).FirstOrDefault();
					mapping.Add(p.Key, min.Key);
				}

				//Console.WriteLine(quantized.PixelFormat);
				//Bitmap reducedBitmap = new Bitmap(quantized);
				for (int i = 0; i < voxels.Count; i++)
				{
					Color c = voxels[i].Color.UIntToColor();
					Color replaceColor = mapping[c];

					voxels[i] = new Voxel(voxels[i].X, voxels[i].Y, voxels[i].Z, replaceColor.ColorToUInt());
					progressBar.Report(i / (float)voxels.Count);
				}

			}

			Console.WriteLine("[INFO] Done.");
			return voxels;
		}


	}
}
