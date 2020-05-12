using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace FileToVox.Utils
{
	public static class ColorComparison
	{
		// closed match for hues only:
		public static int CompareColorHue(List<Color> colors, Color target)
		{
			var hue1 = target.GetHue();
			var diffs = colors.Select(n => GetHueDistance(n.GetHue(), hue1));
			var diffMin = diffs.Min(n => n);
			return diffs.ToList().FindIndex(n => n == diffMin);
		}

		// closed match in RGB space
		public static int CompareColorRGB(List<Color> colors, Color target)
		{
			var colorDiffs = colors.Select(n => ColorDiff(n, target)).Min(n => n);
			return colors.FindIndex(n => ColorDiff(n, target) == colorDiffs);
		}

		// weighed distance using hue, saturation and brightness
		public static int CompareColorAll(List<Color> colors, Color target)
		{
			float hue1 = target.GetHue();
			float num1 = ColorNum(target);
			IEnumerable<float> diffs = colors.Select(n => Math.Abs(ColorNum(n) - num1) +
			                                              GetHueDistance(n.GetHue(), hue1));
			float diffMin = diffs.Min(x => x);
			return diffs.ToList().FindIndex(n => n == diffMin);
		}


		// color brightness as perceived:
		private static float GetBrightness(Color c)
		{ return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f; }

		// distance between two hues:
		private static float GetHueDistance(float hue1, float hue2)
		{
			float d = Math.Abs(hue1 - hue2); return d > 180 ? 360 - d : d;
		}

		//  weighed only by saturation and brightness (from my trackbars)
		private static float ColorNum(Color c)
		{
			return c.GetSaturation() * 1 +
			       GetBrightness(c) * 1;
		}

		// distance in RGB space
		private static int ColorDiff(Color c1, Color c2)
		{
			return (int)Math.Sqrt((c1.R - c2.R) * (c1.R - c2.R)
			                      + (c1.G - c2.G) * (c1.G - c2.G)
			                      + (c1.B - c2.B) * (c1.B - c2.B));
		}
	}
}
