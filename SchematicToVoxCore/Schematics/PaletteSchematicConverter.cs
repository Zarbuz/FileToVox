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
					if (!_colors.Contains(bitmap.GetPixel(x, y)) && _colors.Count < colorLimit)
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
			Console.WriteLine("[LOG] Started to convert all colors of blocks to match the palette");
			Schematic newSchematic = new Schematic(schematic.Width, schematic.Height, schematic.Length, new HashSet<Block>());
			List<uint> colors = schematic.Colors;
			Dictionary<uint, int> paletteDictionary = new Dictionary<uint, int>();
			foreach (uint color in colors)
			{
				int index = ColorComparison.CompareColorAll(_colors, color.UIntToColor());
				paletteDictionary[color] = index;
			}

			using (ProgressBar progressbar = new ProgressBar())
			{
				int i = 0;
				foreach (Block block in schematic.Blocks)
				{
					newSchematic.Blocks.Add(new Block(block.X, block.Y, block.Z, _colors[paletteDictionary[block.Color]].ColorToUInt(), paletteDictionary[block.Color]));
					progressbar.Report(i++ / (float)schematic.Blocks.Count);
				}
			}

			Console.WriteLine("[LOG] Done.");
			return newSchematic;
		}
	}
}
