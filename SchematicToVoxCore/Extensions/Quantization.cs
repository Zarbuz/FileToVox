using FileToVox.Schematics;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace FileToVox.Extensions
{
    public static class Quantization
    {
        public static List<Voxel> ApplyQuantization(List<Voxel> blocks, int colorLimit)
        {
            Quantizer.Quantizer quantizer = new Quantizer.Quantizer();
            try
            {
	            if (blocks.Count == 0)
	            {
					Console.WriteLine("[WARNING] No voxels to quantize, skipping this part...");
					return blocks;
	            }

                using (Bitmap bitmap = CreateBitmapFromColors(blocks))
                {
                    using (Image quantized = quantizer.QuantizeImage(bitmap, 10, 70, colorLimit))
                    {
                        Bitmap reducedBitmap = (Bitmap) quantized;
                        //Console.WriteLine(quantized.PixelFormat);
                        //Bitmap reducedBitmap = new Bitmap(quantized);
                        int width = reducedBitmap.Size.Width;
                        for (int i = 0; i < blocks.Count; i++)
                        {
                            int x = i % width;
                            int y = i / width;
                            blocks[i] = new Voxel(blocks[i].X, blocks[i].Y, blocks[i].Z,
                                reducedBitmap.GetPixel(x, y).ColorToUInt());
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return blocks;
        }

        private static Bitmap CreateBitmapFromColors(List<Voxel> blocks)
        {
            int width = blocks.Count;

            Bitmap bitmap = new Bitmap(width, 1);

            for (int i = 0; i < blocks.Count; i++)
            {
                Voxel voxel = blocks[i];
                Color color = voxel.Color.UIntToColor();
                int x = i % width;
                int y = i / width;
                bitmap.SetPixel(x, y, color);
            }

            return bitmap;
        }
    }
}
