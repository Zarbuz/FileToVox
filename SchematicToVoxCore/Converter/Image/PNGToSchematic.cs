using System;
using System.Drawing;
using System.IO;
using FileToVox.Utils;
using FileToVoxCommon.Generator.Heightmap.Data;
using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;

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

			Bitmap bitmap = new Bitmap(new FileInfo(PathFile).FullName);
			Bitmap bitmapColor = null;
            if (!string.IsNullOrEmpty(ColorPath))
			{
				bitmapColor = new Bitmap(new FileInfo(ColorPath).FullName);
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
				Reverse = false,
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
