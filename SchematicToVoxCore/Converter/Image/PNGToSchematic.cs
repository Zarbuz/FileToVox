using FileToVox.Generator.Heightmap.Data;
using FileToVox.Schematics;
using FileToVox.Utils;
using System;
using System.Drawing;
using System.IO;

namespace FileToVox.Converter.Image
{
	public class PNGToSchematic : ImageToSchematic
    {
        public PNGToSchematic(string path, string colorPath, int height, bool excavate, bool color, int colorLimit)
            : base(path, colorPath, height, excavate, color, colorLimit)
        {
        }

        protected override Schematic WriteSchematicMain()
        {
	        if (!File.Exists(Path))
	        {
		        Console.WriteLine("[ERROR] The file path is invalid for path : " + Path);
		        return null;
	        }

	        if (!string.IsNullOrEmpty(ColorPath) && !File.Exists(ColorPath))
	        {
		        Console.WriteLine("[ERROR] The color path is invalid");
		        return null;
	        }

			Bitmap bitmap = new Bitmap(new FileInfo(Path).FullName);
			Bitmap bitmapColor = null;
            if (!string.IsNullOrEmpty(ColorPath))
			{
				bitmapColor = new Bitmap(new FileInfo(ColorPath).FullName);
			}

            HeightmapStep heightmapStep = new HeightmapStep()
            {
	            TexturePath = Path,
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

        public override Schematic WriteSchematic()
        {
	        return WriteSchematicMain();
            //return WriteSchematicFromImage(Path, ColorPath, ColorLimit, Color);
        }
    }
}
