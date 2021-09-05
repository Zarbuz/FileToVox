using System;
using System.Drawing;
using System.IO;
using FileToVox.Utils;
using FileToVoxCommon.Generator.Heightmap.Data;
using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;

namespace FileToVox.Generator.Heightmap
{
	public class HeightmapGenerator : IGenerator
	{
		private HeightmapData mHeightmapData;
		private Schematic mSchematic;
		public HeightmapGenerator(HeightmapData heightmapData, Schematic schematic)
		{
			mHeightmapData = heightmapData;
			mSchematic = schematic;
		}

		public Schematic WriteSchematic()
		{
			Schematic finalSchematic = new Schematic();
			if (mSchematic != null)
			{
				finalSchematic = new Schematic(mSchematic.GetAllVoxels());
			}
			
			Console.WriteLine("[INFO] Count steps: " + mHeightmapData.Steps.Length);
			for (int index = 0; index < mHeightmapData.Steps.Length; index++)
			{
				Console.WriteLine("[INFO] Start parse heightmap for step : " + index);
				HeightmapStep step = mHeightmapData.Steps[index];
				step.ValidateSettings();
				step.DisplayInfo();

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
