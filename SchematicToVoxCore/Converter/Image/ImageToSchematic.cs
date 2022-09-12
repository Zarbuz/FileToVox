using FileToVox.Utils;
using FileToVoxCore.Schematics;
using ImageMagick;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FileToVox.Converter.Image
{
	public class ImageToSchematic : AbstractToSchematic
    {
        protected readonly bool Excavate;
        protected readonly int MaxHeight;
        protected readonly bool Color;
        protected readonly string ColorPath;
        protected readonly int ColorLimit;

        [StructLayout(LayoutKind.Explicit)]
        public struct RGB
        {
	        // Structure of pixel for a 24 bpp bitmap
	        [FieldOffset(0)] public byte blue;
	        [FieldOffset(1)] public byte green;
	        [FieldOffset(2)] public byte red;
	        [FieldOffset(3)] public byte alpha;
	        [FieldOffset(0)] public int argb;
        }

        public ImageToSchematic(string path, string colorPath, int height, bool excavate, bool color, int colorLimit) : base(path)
        {
            ColorPath = colorPath;
            MaxHeight = height;
            Excavate = excavate;
            Color = color;
            ColorLimit = colorLimit;
        }

        public override Schematic WriteSchematic()
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

	        MagickImage image = new MagickImage(PathFile);
	        MagickImage colorImage = null;

			if (!string.IsNullOrEmpty(ColorPath))
			{
				colorImage = new MagickImage(ColorPath);
			}

			LoadImageParam loadImageParam = new LoadImageParam()
			{
				TexturePath = PathFile,
				ColorLimit = ColorLimit,
				ColorTexturePath = ColorPath,
				EnableColor = Color,
				Excavate = Excavate,
				Height = MaxHeight,
			};
			return ImageUtils.WriteSchematicFromImage(image, colorImage, loadImageParam);

		}
	}
}
