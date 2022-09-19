using FileToVox.Extensions;
using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ImageMagick;
using Color = FileToVoxCore.Drawing.Color;

namespace FileToVox.Converter.PaletteSchematic
{
	public class PaletteSchematicConverter
	{
		private readonly List<Color> _colors;

		public PaletteSchematicConverter(string palettePath)
		{
			_colors = new List<Color>();
			MagickImage bitmap = new MagickImage(palettePath);
			IPixelCollection<ushort> pixels = bitmap.GetPixels();
			for (int x = 0; x < bitmap.Width; x++)
			{
				for (int y = 0; y < bitmap.Height; y++)
				{
					if (_colors.Count < 256)
					{
						_colors.Add(pixels.GetPixel(x, y).GetPixelColor().ToFileToVoxCoreColor());
					}
				}
			}
		}

		public List<Color> GetPalette()
		{
			return _colors;
		}

		public List<uint> GetPaletteUint()
		{
			List<uint> palette = _colors.Select(color => color.ColorToUInt()).ToList();
			return palette;
		}

		public Schematic ConvertSchematic(Schematic schematic)
		{
			Console.WriteLine("[INFO] Started to convert all colors of blocks to match the palette");
			Schematic newSchematic = new Schematic(GetPaletteUint());
			List<uint> colors = schematic.UsedColors;
			Dictionary<uint, int> paletteDictionary = new Dictionary<uint, int>();
			foreach (uint color in colors)
			{
				int index = ColorComparison.CompareColorRGB(_colors, color.UIntToColor());
				paletteDictionary[color] = index;
			}

			using (ProgressBar progressbar = new ProgressBar())
			{
				int i = 0;
				List<Voxel> allVoxels = schematic.GetAllVoxels();
				foreach (Voxel block in allVoxels)
				{
					newSchematic.AddVoxel(block.X, block.Y, block.Z,
						_colors[paletteDictionary[block.Color]].ColorToUInt());
					progressbar.Report(i++ / (float) allVoxels.Count);
				}
			}

			Console.WriteLine("[INFO] Done.");
			return newSchematic;
		}
	}
}