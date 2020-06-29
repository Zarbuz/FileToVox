using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace FileToVox.Converter.Image
{
	public class DirectBitmap : IDisposable
	{
		public int[] Bits { get; }
		public bool Disposed { get; private set; }
		public int Height { get; }
		public int Width { get; }


		public DirectBitmap(Bitmap bitmap)
		{
			Width = bitmap.Width;
			Height = bitmap.Height;
			Bits = new int[Width * Height];
			BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly,
				PixelFormat.Format32bppRgb);

			unsafe
			{
				ImageToSchematic.RGB* p = (ImageToSchematic.RGB*) data.Scan0;
				int last = p->argb;
				int h = bitmap.Height;
				int w = bitmap.Width;
				for (int y = 0; y < h; ++y)
				{
					for (int x = 0; x < w; ++x)
					{
						int c = p->argb;
						if (c == last)
						{
							SetPixel(x, y, c);
						}
						else
						{
							SetPixel(x, y, c);
							last = c;
						}

						++p;
					}
				}
			}
		}

		public void SetPixel(int x, int y, Color color)
		{
			int index = x + (y * Width);
			int col = color.ToArgb();

			Bits[index] = col;
		}

		public void SetPixel(int x, int y, int color)
		{
			int index = x + (y * Width);
			Bits[index] = color;
		}

		public Color GetPixel(int x, int y)
		{
			int index = x + (y * Width);
			int col = Bits[index];
			Color result = Color.FromArgb(col);

			return result;
		}

		public void Dispose()
		{
			if (Disposed) return;
			Disposed = true;
		}
	}
}