using System;
using FileToVox.Generator.Heightmap.Data;
using FileToVox.Schematics;

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
			Schematic schematic = new Schematic();
			for (int index = 0; index < mHeightmapData.Steps.Length; index++)
			{
				HeightmapStep step = mHeightmapData.Steps[index];
				if (string.IsNullOrEmpty(step.TexturePath))
				{
					throw new ArgumentException("[ERROR] Missing mandatory texture path for step: " + (index + 1));
				}

				
			}

			return schematic;
		}
	}
}
