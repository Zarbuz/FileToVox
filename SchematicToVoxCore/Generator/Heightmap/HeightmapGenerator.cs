using System;
using System.Drawing;
using System.IO;
using FileToVox.Generator.Heightmap.Data;
using FileToVox.Schematics;
using FileToVox.Utils;

namespace FileToVox.Generator.Heightmap
{
	public class HeightmapGenerator : IGenerator
	{
		private HeightmapData mHeightmapData;
		public HeightmapGenerator(HeightmapData heightmapData)
		{
			mHeightmapData = heightmapData;
		}

		public Schematic WriteSchematic()
		{
			Schematic finalSchematic = new Schematic();
			for (int index = 0; index < mHeightmapData.Steps.Length; index++)
			{
				HeightmapStep step = mHeightmapData.Steps[index];
				step.ValidateSettings();

				Bitmap bitmap = new Bitmap(new FileInfo(step.TexturePath).FullName);
				Bitmap bitmapColor = null;
				if (!string.IsNullOrEmpty(step.ColorTexturePath))
				{
					bitmapColor = new Bitmap(new FileInfo(step.ColorTexturePath).FullName);
				}

				Schematic schematicStep = ImageUtils.WriteSchematicFromImage(bitmap, bitmapColor, step);
				finalSchematic = SchematicMerger.Merge(finalSchematic, schematicStep, step.PlacementMode);
			}

			return finalSchematic;
		}
	}
}
