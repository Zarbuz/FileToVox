﻿using System;
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
			Console.WriteLine("[INFO] Count steps: " + mHeightmapData.Steps.Length);
			for (int index = 0; index < mHeightmapData.Steps.Length; index++)
			{
				Console.WriteLine("[INFO] Start parse heightmap for step : " + index);
				HeightmapStep step = mHeightmapData.Steps[index];
				step.ValidateSettings();
				step.DisplayInfo();

				if (step.PlacementMode == PlacementMode.TOP_ONLY && index == 0)
				{
					Console.WriteLine("[ERROR] PlacementMode TOP_ONLY shouldn't be at the first step");
				}

				Bitmap bitmap = new Bitmap(new FileInfo(step.TexturePath).FullName);
				Bitmap bitmapColor = null;
				if (!string.IsNullOrEmpty(step.ColorTexturePath))
				{
					bitmapColor = new Bitmap(new FileInfo(step.ColorTexturePath).FullName);
				}


				Schematic schematicStep = ImageUtils.WriteSchematicFromImage(bitmap, bitmapColor, step);
				finalSchematic = SchematicMerger.Merge(finalSchematic, schematicStep, step);
			}

			return finalSchematic;
		}
	}
}
