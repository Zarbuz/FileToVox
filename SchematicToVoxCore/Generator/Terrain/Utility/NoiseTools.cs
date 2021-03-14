using System.Drawing;
using System.IO;
using FileToVox.Schematics.Tools;

namespace FileToVox.Generator.Terrain.Utility
{
	public static class NoiseTools
	{
		public static Vector3 SeedOffset;

		public static float[] LoadNoiseTexture(string path, out int textureSize)
		{
			string fullPath = Path.Combine(TerrainEnvironment.Instance.WorldTerrainData.DirectoryPath, path);

			if (!File.Exists(fullPath))
			{
				textureSize = 0;
				return null;
			}

			Bitmap bitmap = new Bitmap(new FileInfo(fullPath).FullName);
			textureSize = bitmap.Width;

			float[] values = new float[bitmap.Width * bitmap.Height];
			int index = 0;
			for (int x = 0; x < bitmap.Width; x++)
			{
				for (int y = 0; y < bitmap.Height; y++)
				{
					values[index] = bitmap.GetPixel(x, y).R / (float)255;
					index++;
				}
			}

			return values;
		}

		public static float GetNoiseValueBilinear(float[] noiseArray, int textureSize, float x, float z, bool ridgeNoise = false)
		{
			if (textureSize == 0)
				return 0;

			double zz = (double)z + textureSize * 10000f + SeedOffset.Z;
			double xx = (double)x + textureSize * 10000f + SeedOffset.Z;
			int posZInt = (int)zz;
			int posXInt = (int)xx;
			float fy = (float)(zz - posZInt);
			float fx = (float)(xx - posXInt);

			int ty0 = posZInt % textureSize;
			int tx0 = posXInt % textureSize;

			int ty = (ty0 == textureSize - 1) ? 0 : ty0 + 1;
			float noiseUL = noiseArray[ty * textureSize + tx0];
			int tx = (tx0 == textureSize - 1) ? 0 : tx0 + 1;
			float noiseUR = noiseArray[ty * textureSize + tx];
			float noiseBL = noiseArray[ty0 * textureSize + tx0];
			float noiseBR = noiseArray[ty0 * textureSize + tx];

			float value = (1f - fx) * (fy * noiseUL + (1f - fy) * noiseBL) + fx * (fy * noiseUR + (1f - fy) * noiseBR);

			if (ridgeNoise)
			{
				value = 0.5f - value;
				if (value < 0)
				{
					value = 2f * (0.5f + value);
				}
				else
				{
					value = 2f * (0.5f - value);
				}
			}
			return value;
		}
	}
}
