using FileToVox.Converter.Image;
using FileToVox.Schematics;
using FileToVox.Schematics.Tools;
using FileToVox.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace SchematicToVoxCore.Extensions
{
    public static class FctExtensions
    {
        public static int CountColor(this Bitmap bitmap)
        {
            Console.WriteLine("[INFO] Check total different colors...");
            //Make a clone of the bitmap to avoid lock bitmaps in the rest of the code
            Bitmap clone = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (Graphics gr = Graphics.FromImage(clone))
            {
	            gr.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
            }

            BitmapData data = clone.LockBits(new Rectangle(0, 0, clone.Width, clone.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppRgb);

            Dictionary<int, int> counts = new Dictionary<int, int>();
            unsafe
            {
	            ImageToSchematic.RGB* p = (ImageToSchematic.RGB*)data.Scan0;
	            int last = p->argb;
	            counts.Add(last, 1);
	            int h = clone.Height;
                int w = clone.Width;
                int index = 0;
	            using (ProgressBar progressBar = new ProgressBar())
	            {
		            for (int y = 0; y < h; ++y)
		            {
			            for (int x = 0; x < w; ++x)
			            {
				            int c = p->argb;
				            if (c == last) counts[last] += 1;
				            else
				            {
					            if (!counts.ContainsKey(c))
						            counts.Add(c, 1);
					            else
						            counts[c]++;
					            last = c;
				            }
				            progressBar.Report(index++ / (float)(w * h));
                            ++p;
			            }
		            }
	            }
            }
            
            Console.WriteLine("[INFO] Done. (" + counts.Count + ")");
            return counts.Count;
        }

        public static uint ColorToUInt(this Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) |
                          (color.G << 8) | (color.B << 0));
        }

        public static uint ByteArrayToUInt(byte r, byte g, byte b, byte a)
        {
            return (uint)((a << 24) | (r << 16) | (g << 8) | (b << 0));
        }

        public static Color UIntToColor(this uint color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }

        public static List<Voxel> ApplyOffset(this List<Voxel> list, Vector3 vector)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = new Voxel((ushort)(list[i].X - vector.X), (ushort)(list[i].Y - vector.Y), (ushort)(list[i].Z - vector.Z), list[i].Color);
            }

            return list;
        }

		public static Dictionary<ulong, Voxel> ToVoxelDictionary(this List<Voxel> voxels)
		{
			Dictionary<ulong, Voxel> dictionary = new Dictionary<ulong, Voxel>();
			foreach (Voxel voxel in voxels)
			{
				ulong index = Schematic.GetVoxelIndex(voxel.X, voxel.Y, voxel.Z);
				dictionary[index] = voxel;
			}

			return dictionary;
		}
      
    }

	
}
