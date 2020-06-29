using BitMiracle.LibTiff.Classic;
using FileToVox.Extensions;
using FileToVox.Schematics;
using FileToVox.Utils;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

namespace FileToVox.Converter.Image
{
	public class TIFtoSchematic : ImageToSchematic
    {
        public TIFtoSchematic(string path, string colorPath, int height, bool excavate, bool color, bool top, int colorLimit) : base(path, colorPath, height, excavate, color, top, colorLimit)
        {
        }

        public override Schematic WriteSchematic()
        {
            return WriteSchematicFromImage();
        }

        private Schematic WriteSchematicFromImage()
        {
            Bitmap bitmap = ConvertTifToBitmap(_path);
            Bitmap clone = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (Graphics gr = Graphics.FromImage(clone))
            {
	            gr.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
            }
            Bitmap bitmapColor = new Bitmap(bitmap.Width, bitmap.Height); //default initialization
            Quantizer.Quantizer quantizer = new Quantizer.Quantizer();

            if (_colorPath != null)
            {
                bitmapColor = ConvertTifToBitmap(_colorPath);
                if (bitmap.Height != bitmapColor.Height || bitmap.Width != bitmapColor.Width)
                {
                    throw new ArgumentException("[ERROR] Image color is not the same size of the original image");
                }

                clone = new Bitmap(bitmapColor.Width, bitmapColor.Height, PixelFormat.Format32bppArgb);
                using (Graphics gr = Graphics.FromImage(clone))
                {
	                gr.DrawImage(bitmapColor, new Rectangle(0, 0, clone.Width, clone.Height));
                }

                if (_colorLimit != 256 || bitmapColor.CountColor() > 256)
                {
	                System.Drawing.Image image = quantizer.QuantizeImage(clone, 10, 70, _colorLimit);
	                bitmapColor = new Bitmap(image);
                }
            }
            else if (_color)
            {
	            if (_colorLimit != 256 || clone.CountColor() > 256)
	            {
		            System.Drawing.Image image = quantizer.QuantizeImage(bitmap, 10, 70, _colorLimit);
		            bitmap = new Bitmap(image);
	            }
            }

            Schematic schematic = WriteSchematicIntern(bitmap, bitmapColor);
            return schematic;
        }

        private Bitmap ConvertTifToBitmap(string path)
        {
            using (Tiff tiff = Tiff.Open(path, "r"))
            {
                FieldValue[] value = tiff.GetField(TiffTag.IMAGEWIDTH);
                int width = value[0].ToInt();

                value = tiff.GetField(TiffTag.IMAGELENGTH);
                int height = value[0].ToInt();

                int[] raster = new int[height * width];
                if (!tiff.ReadRGBAImage(width, height, raster))
                {
                    throw new Exception("Could not read image");
                }

                Bitmap bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
                BitmapData bmpData = bmp.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
                byte[] bytes = new byte[bmpData.Stride * bmpData.Height];

                for (int y = 0; y < bmp.Height; y++)
                {
                    int rasterOffset = y * bmp.Width;
                    int bitsOffset = (bmp.Height - y - 1) * bmpData.Stride;

                    for (int x = 0; x < bmp.Width; x++)
                    {
                        int rgba = raster[rasterOffset++];
                        bytes[bitsOffset++] = (byte)((rgba >> 16) & 0xff);
                        bytes[bitsOffset++] = (byte)((rgba >> 8) & 0xff);
                        bytes[bitsOffset++] = (byte)(rgba & 0xff);
                        bytes[bitsOffset++] = (byte)((rgba >> 24) & 0xff);
                    }
                }

                System.Runtime.InteropServices.Marshal.Copy(bytes, 0, bmpData.Scan0, bytes.Length);
                bmp.UnlockBits(bmpData);

                return bmp;
            }
        }
    }
}
