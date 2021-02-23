using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using FileToVox.Schematics;
using Newtonsoft.Json;

namespace FileToVox.Generator.Terrain.Data
{
	[Serializable]
	public struct BiomeData
	{
		public float AltitudeMin { get; set; }
		public float AltitudeMax { get; set; }
		public float MoistureMin { get; set; }
		public float MoistureMax { get; set; }

		[JsonIgnore]
		public BiomeDefinition Biome;
	}

	[Serializable]
	public struct BiomeTree
	{
		public ModelDefinition Bits { get; set; }
		public float Probability { get; set; }
	}

	public struct BiomeVegetation
	{
		public Color Color { get; set; }
		public float Probability { get; set; }
	}

	[Serializable]
	public class BiomeDefinition
	{
		public BiomeData[] Zones { get; set; }
		public Color VoxelTop { get; set; }
		public Color VoxelDirt { get; set; }

		public float TreeDensity { get; set; }
		public BiomeTree[] Trees { get; set; }

		public float VegetationDensity { get; set; }
		public BiomeVegetation[] Vegetation { get; set; }

	}
}
