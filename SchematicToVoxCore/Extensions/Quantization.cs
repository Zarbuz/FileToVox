using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using FileToVox.Schematics;
using nQuant;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Extensions
{
    public static class Quantization
    {
        public static List<Block> ApplyQuantization(List<Block> blocks)
        {
            WuQuantizer quantizer = new WuQuantizer();
            try
            {
                using (Bitmap bitmap = CreateBitmapFromColors(blocks))
                {
                    using (Image quantized = quantizer.QuantizeImage(bitmap))
                    {
                        Bitmap reducedBitmap = (Bitmap) quantized;
                        //Console.WriteLine(quantized.PixelFormat);
                        //Bitmap reducedBitmap = new Bitmap(quantized);
                        int width = reducedBitmap.Size.Width;
                        for (int i = 0; i < blocks.Count; i++)
                        {
                            int x = i % width;
                            int y = i / width;
                            blocks[i] = new Block(blocks[i].X, blocks[i].Y, blocks[i].Z,
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

        private static Bitmap CreateBitmapFromColors(List<Block> blocks)
        {
            int width = blocks.Count;

            Bitmap bitmap = new Bitmap(width, 1);

            for (int i = 0; i < blocks.Count; i++)
            {
                Block block = blocks[i];
                Color color = block.Color.UIntToColor();
                int x = i % width;
                int y = i / width;
                bitmap.SetPixel(x, y, color);
            }

            return bitmap;
        }
    }
}
