using BitMiracle.LibTiff.Classic;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using FileToVox.Utils;
using FileToVoxCommon.Generator.Heightmap.Data;
using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;

namespace FileToVox.Converter.Image
{
	public class TIFtoSchematic : ImageToSchematic
    {
        public TIFtoSchematic(string path, string colorPath, int height, bool excavate, bool color, int colorLimit) : base(path, colorPath, height, excavate, color, colorLimit)
        {
        }

        public override Schematic WriteSchematic()
        {
	        return WriteSchematicMain();
        }

        protected override Schematic WriteSchematicMain()
        {
	        if (!File.Exists(PathFile))
	        {
		        Console.WriteLine("[ERROR] The file path is invalid for path : " + PathFile);
		        return null;
	        }

	        if (!string.IsNullOrEmpty(ColorPath) && !File.Exists(ColorPath))
	        {
		        Console.WriteLine("[ERROR] The color path is invalid");
		        return null;
	        }

	        Bitmap bitmap = ConvertTifToBitmap(PathFile);
	        Bitmap bitmapColor = null;
	        if (!string.IsNullOrEmpty(ColorPath))
	        {
		        bitmapColor = ConvertTifToBitmap(ColorPath);
	        }

	        HeightmapStep heightmapStep = new HeightmapStep()
	        {
		        TexturePath = PathFile,
		        ColorLimit = ColorLimit,
		        ColorTexturePath = ColorPath,
		        EnableColor = Color,
		        Excavate = Excavate,
		        Height = MaxHeight,
		        Offset = 0,
		        PlacementMode = PlacementMode.ADDITIVE,
		        RotationMode = RotationMode.Y
	        };

            return ImageUtils.WriteSchematicFromImage(bitmap, bitmapColor, heightmapStep);
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
