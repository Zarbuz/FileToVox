using System;
using System.Drawing;
using Newtonsoft.Json;

namespace FileToVoxCommon.Generator.Terrain.Data
{
	[Serializable]
	public struct BiomeZone
	{
		public float AltitudeMin { get; set; }
		public float AltitudeMax { get; set; }
		public float MoistureMin { get; set; }
		public float MoistureMax { get; set; }

		[JsonIgnore]
		public BiomeSettings Biome;
	}

	[Serializable]
	public struct BiomeTree
	{
		public ModelSettings Bits { get; set; }
		public float Probability { get; set; }
	}

	public struct BiomeVegetation
	{
		public Color Color { get; set; }
		public float Probability { get; set; }
	}

	[Serializable]
	public class BiomeSettings
	{
		public BiomeZone[] Zones { get; set; }
		public Color VoxelTop { get; set; }
		public Color VoxelDirt { get; set; }

		public float TreeDensity { get; set; }
		public BiomeTree[] Trees { get; set; }

		public float VegetationDensity { get; set; }
		public BiomeVegetation[] Vegetation { get; set; }

		[JsonIgnore] public int BiomeGeneration;
		public void ValidateSettings(WorldTerrainData world)
		{
			if (Trees == null)
			{
				Trees = new BiomeTree[0];
			}

			if (Vegetation == null)
			{
				Vegetation = new BiomeVegetation[0];
			}
			
			if (Zones != null)
			{
				for (int i = 0; i < Zones.Length; i++)
				{
					BiomeZone zone = Zones[i];
					zone.Biome = this;

					if (zone.MoistureMin == 0 && zone.MoistureMax == 0)
					{
						zone.MoistureMax = 1;
					}

					Zones[i] = zone;
				}
			}

		}
	}
}
