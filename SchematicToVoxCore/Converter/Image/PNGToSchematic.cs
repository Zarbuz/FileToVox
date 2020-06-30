using FileToVox.Extensions;
using FileToVox.Schematics;
using FileToVox.Utils;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace FileToVox.Converter.Image
{
	public class PNGToSchematic : ImageToSchematic
    {
        public PNGToSchematic(string path, string colorPath, int height, bool excavate, bool color, bool top, int colorLimit)
            : base(path, colorPath, height, excavate, color, top, colorLimit)
        {
        }

        public override Schematic WriteSchematic()
        {
            return WriteSchematicFromImage();
        }

        private Schematic WriteSchematicFromImage()
        {
            Bitmap bitmap = new Bitmap(new FileInfo(_path).FullName);
            Bitmap clone = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format32bppArgb);
            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(bitmap, new Rectangle(0, 0, clone.Width, clone.Height));
            }
            Bitmap bitmapColor = new Bitmap(bitmap.Width, bitmap.Height); //default initialization
            Quantizer.Quantizer quantizer = new Quantizer.Quantizer();

            if (_colorPath != null)
            {
                bitmapColor = new Bitmap(new FileInfo(_colorPath).FullName);
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
		            System.Drawing.Image image = quantizer.QuantizeImage(clone, 10, 70, _colorLimit);
		            bitmap = new Bitmap(image);
                }
            }

            Schematic schematic = WriteSchematicIntern(bitmap, bitmapColor);
            return schematic;
        }
    }
}
