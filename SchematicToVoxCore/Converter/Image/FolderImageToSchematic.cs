using FileToVox.Schematics;
using FileToVox.Utils;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using FileToVox.Extensions;

namespace FileToVox.Converter.Image
{
	public class FolderImageToSchematic : AbstractToSchematic
    {
        private readonly bool mExcavate;
        private readonly string mInputColorFile;
        private readonly int mColorLimit;
        public FolderImageToSchematic(string path, bool excavate, string inputColorFile, int colorLimit) : base(path)
        {
            mExcavate = excavate;
            mInputColorFile = inputColorFile;
            mColorLimit = colorLimit;
        }

        public override Schematic WriteSchematic()
        {
            int height = Directory.GetFiles(_path, "*.*", SearchOption.AllDirectories).Count(s => s.EndsWith(".png"));
            Console.WriteLine("[INFO] Count files in the folder : " + height);

            List<Voxel> blocks = new List<Voxel>();
            Bitmap bitmapColor = null;
            if (mInputColorFile != null)
            {
	            bitmapColor = new Bitmap(mInputColorFile);
	            if (bitmapColor.Width > 256 || bitmapColor.Height > 1)
	            {
		            throw new ArgumentException("[ERROR] The input color file must have a dimension of 256x1 px");
	            }
            }

            int maxWidth = 0;
            int maxLength = 0;
            using (ProgressBar progressbar = new ProgressBar())
            {
                string[] files = Directory.GetFiles(_path);
                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    Bitmap bitmap = new Bitmap(file);
                    DirectBitmap directBitmap = new DirectBitmap(bitmap);
                    for (int x = 0; x < directBitmap.Width; x++)
                    {
                        for (int y = 0; y < directBitmap.Height; y++)
                        {
                            Color color = directBitmap.GetPixel(x, y);
                            if (color != Color.Empty && color != Color.Transparent && color != Color.Black && (color.R != 0 && color.G != 0 && color.B != 0))
                            {
	                            if (mInputColorFile != null)
	                            {
		                            double distance = Math.Sqrt(Math.Pow((height / 2) - x, 2) + Math.Pow((height / 2) - y, 2));
		                            float range = (float) Math.Abs(distance / (height / 2)); //
		                            range = range > 1 ? 1 : range;
		                            color = bitmapColor.GetPixel((int)(range * (bitmapColor.Width - 1)), 0);
	                            }

	                            maxWidth = maxWidth < directBitmap.Width ? directBitmap.Width : maxWidth;
                                maxLength = maxLength < directBitmap.Height ? directBitmap.Height : maxLength;
                                if (mExcavate)
                                {
                                    CheckNeighbor(ref blocks, directBitmap, color, i, x, y);
                                }
                                else
                                {
                                    blocks.Add(new Voxel((ushort) x, (ushort) i, (ushort) y, color.ColorToUInt()));
                                }
                            }
                        }
                    }
                    directBitmap.Dispose();
                    progressbar.Report(i / (float)files.Length);
                }
            }

            Schematic schematic = new Schematic();
            schematic.Height = (ushort)height;
            schematic.Width = (ushort)maxWidth;
            schematic.Length = (ushort)maxLength;

            List<Voxel> list = Quantization.ApplyQuantization(blocks, mColorLimit);

            schematic.Blocks = list.ToHashSet();

            Console.WriteLine("[LOG] Done.");
            return schematic;
        }

        private void CheckNeighbor(ref List<Voxel> blocks, DirectBitmap bitmap, Color color, int i, int x, int y)
        {
            if (x - 1 >= 0 && x + 1 < bitmap.Width && y - 1 >= 0 && y + 1 < bitmap.Height)
            {
                Color left = bitmap.GetPixel(x - 1, y);
                Color top = bitmap.GetPixel(x, y - 1);
                Color right = bitmap.GetPixel(x + 1, y);
                Color bottom = bitmap.GetPixel(x, y + 1);

                bool leftColor = left != Color.Empty && left != Color.Transparent && left != Color.Black && (left.R != 0 && left.G != 0 && left.B != 0);
                bool topColor = top != Color.Empty && top != Color.Transparent && top != Color.Black && (top.R != 0 && top.G != 0 && top.B != 0);
                bool rightColor = right != Color.Empty && right != Color.Transparent && right != Color.Black && (right.R != 0 && right.G != 0 && right.B != 0);
                bool bottomColor = bottom != Color.Empty && bottom != Color.Transparent && bottom != Color.Black && (bottom.R != 0 && bottom.G != 0 && bottom.B != 0);

                if (!leftColor || !topColor || !rightColor || !bottomColor)
                {
	                blocks.Add(new Voxel((ushort) x, (ushort) i, (ushort) y, color.ColorToUInt()));
                }
            }
        }
    }
}
