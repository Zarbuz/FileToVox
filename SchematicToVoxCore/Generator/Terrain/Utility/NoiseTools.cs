using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace FileToVox.Generator.Terrain.Utility
{
	public static class NoiseTools
	{
		public static float[] LoadNoiseTexture(string path, out int textureSize)
		{
			if (!File.Exists(path))
			{
				textureSize = 0;
				return null;
			}

			Bitmap bitmap = new Bitmap(new FileInfo(path).FullName);
			textureSize = bitmap.Width;

			float[] values = new float[bitmap.Width * bitmap.Height];
			int index = 0;
			for (int x = 0; x < bitmap.Width; x++)
			{
				for (int y = 0; y < bitmap.Height; y++)
				{
					values[index] = bitmap.GetPixel(x, y).R;
					index++;
				}
			}

			return values;
		}
	}
}
