using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FileToVox.Utils;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Schematics
{
	public class PaletteSchematicConverter
	{
		private List<Color> _colors;
		public PaletteSchematicConverter(string palettePath, int colorLimit)
		{
			_colors = new List<Color>();
			Bitmap bitmap = new Bitmap(palettePath);
			for (int x = 0; x < bitmap.Width; x++)
			{
				for (int y = 0; y < bitmap.Height; y++)
				{
					if (_colors.Count < colorLimit)
					{
						_colors.Add(bitmap.GetPixel(x, y));
					}
				}
			}
		}

		public List<Color> GetPalette()
		{
			return _colors;
		}

		public Schematic ConvertSchematic(Schematic schematic)
		{
			Console.WriteLine("[INFO] Started to convert all colors of blocks to match the palette");
			Schematic newSchematic = new Schematic();
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
					newSchematic.AddVoxel(block.X, block.Y, block.Z, _colors[paletteDictionary[block.Color]].ColorToUInt());
					progressbar.Report(i++ / (float)allVoxels.Count);
				}
			}

			Console.WriteLine("[INFO] Done.");
			return newSchematic;
		}
	}
}
