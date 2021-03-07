using System;
using System.Collections.Generic;
using FileToVox.Generator.Terrain.Data;
using FileToVox.Generator.Terrain.Entities;
using FileToVox.Generator.Terrain.Utility;
using FileToVox.Schematics;
using FileToVox.Schematics.Tools;

namespace FileToVox.Generator.Terrain
{
	public class BiomeLookUp
	{
		public BiomeZone Zone;
		public bool UniqueZone;
		public List<BiomeZone> OverlappingZones = new List<BiomeZone>();

		public BiomeLookUp(BiomeZone biome, bool uniqueZone)
		{
			Zone = biome;
			UniqueZone = uniqueZone;
		}

		public BiomeSettings GetBiome(float altitude, float moisture)
		{
			if (UniqueZone)
			{
				return Zone.Biome;
			}

			for (int i = 0; i < OverlappingZones.Count; i++)
			{
				if (altitude >= OverlappingZones[i].AltitudeMin && altitude < OverlappingZones[i].AltitudeMax &&
					moisture >= OverlappingZones[i].MoistureMin && moisture < OverlappingZones[i].MoistureMax)
				{
					return OverlappingZones[i].Biome;
				}
			}

			return Zone.Biome;
		}
	}

	public struct HeightMapInfo
	{
		public float Moisture;
		public int GroundLevel;
		public BiomeSettings Biome;
	}

	public partial class TerrainEnvironment
	{
		#region Fields
		private Dictionary<int, CachedChunk> mCachedChunks;
		private int mVisibleXMin, mVisibleXMax, mVisibleYMin, mVisibleYMax, mVisibleZMin, mVisibleZMax;

		private HeightMapCache mHeightMapCache;
		private BiomeLookUp[] mBiomeLookUps;
		#endregion

		#region PublicMethods

		public void InitChunkManager()
		{
			mCachedChunks = new Dictionary<int, CachedChunk>();

			InitHeightMap();
			InitBiomes();

			NoiseTools.SeedOffset = WorldRandom.GetVector3(Vector3.zero, 1024);

			if (WorldTerrainData != null)
			{
				if (WorldTerrainData.TerrainGeneratorSettings == null)
				{
					WorldTerrainData.TerrainGeneratorSettings = new TerrainGeneratorSettings(); //TODO
				}

				WorldTerrainData.TerrainGeneratorSettings.Initialize();
			}
		}

		public void DisposeAll()
		{
			mLastChunkFetch = null;

			if (mCachedChunks != null)
			{
				mCachedChunks.Clear();
				mCachedChunks = null;
			}

			// Clear heightmap
			if (mHeightMapCache != null)
			{
				mHeightMapCache.Clear();
			}
		}

		public void CheckChunksInRange()
		{
			//MV has a max size of 2000x2000
			int width = Math.Min(Math.Max(WorldTerrainData.Width, WorldTerrainData.Length), Schematic.MAX_WORLD_WIDTH);
			int chunkXZDistance = (width / CHUNK_SIZE) / 2;
			int chunkYDistance = (Schematic.MAX_WORLD_HEIGHT / CHUNK_SIZE) / 2;

			mVisibleXMin = -chunkXZDistance;
			mVisibleXMax = chunkXZDistance;

			mVisibleZMin = -chunkXZDistance;
			mVisibleZMax = chunkXZDistance;

			mVisibleYMin = -chunkYDistance;
			mVisibleYMax = chunkYDistance;
			CheckNewNearChunks();
		}

		public void GetHeightMapInfoFast(float x, float z, HeightMapInfo[] heightChunkData)
		{
			int ix = FastMath.FloorToInt(x);
			int iz = FastMath.FloorToInt(z);

			TerrainGeneratorSettings tg = WorldTerrainData.TerrainGeneratorSettings;
			for (int zz = 0; zz < CHUNK_SIZE; zz++)
			{
				for (int xx = 0; xx < CHUNK_SIZE; xx++)
				{
					if (!mHeightMapCache.TryGetValue(ix + xx, iz + zz, out HeightMapInfo[] heights, out int heightsIndex))
					{
						tg.GetHeightAndMoisture(ix + xx, iz + zz, out float altitude, out float moisture);
						if (altitude > 1f)
							altitude = 1f;
						else if (altitude < 0f)
							altitude = 0f;
						if (moisture > 1f)
							moisture = 1f;
						else if (moisture < 0f)
							moisture = 0f;
						int biomeIndex = (int)(altitude * 20) * 21 + (int)(moisture * 20f);
						float groundLevel = altitude * tg.MaxHeight;
						heights[heightsIndex].GroundLevel = (int)groundLevel;
						heights[heightsIndex].Moisture = moisture;
						heights[heightsIndex].Biome = mBiomeLookUps[biomeIndex].GetBiome(groundLevel, moisture);
					}
					heightChunkData[zz * CHUNK_SIZE + xx] = heights[heightsIndex];
				}
			}
		}

		#endregion

		#region PrivateMethods

		private void InitHeightMap()
		{
			int poolSize = (30 + 10) * 2 * CHUNK_SIZE / 128 + 1;
			poolSize *= poolSize;
			mHeightMapCache = new HeightMapCache(poolSize);
		}

		private void InitBiomes()
		{
			mBiomeLookUps = new BiomeLookUp[441];
			if (WorldTerrainData == null)
				return;

			if (WorldTerrainData.Biomes == null || WorldTerrainData.Biomes.Length == 0)
			{
				WorldTerrainData.DefaultBiome = new BiomeSettings(); //TODO
			}

			BiomeZone defaultBiome = new BiomeZone();
			if (WorldTerrainData.DefaultBiome != null)
			{
				WorldTerrainData.DefaultBiome.ValidateSettings(WorldTerrainData);
				if (WorldTerrainData.DefaultBiome.Zones != null && WorldTerrainData.DefaultBiome.Zones.Length > 0)
				{
					defaultBiome = WorldTerrainData.DefaultBiome.Zones[0];
				}
			}

			for (int i = 0; i < mBiomeLookUps.Length; i++)
			{
				mBiomeLookUps[i] = new BiomeLookUp(defaultBiome, true);
			}

			if (WorldTerrainData.Biomes == null)
				return;

			for (int i = 0; i < WorldTerrainData.Biomes.Length; i++)
			{
				BiomeSettings biome = WorldTerrainData.Biomes[i];
				if (biome == null)
					continue;
				biome.ValidateSettings(WorldTerrainData);
				if (biome.Zones == null)
					continue;

				for (int j = 0; j < biome.Zones.Length; j++)
				{
					BiomeZone zone = biome.Zones[j];
					for (int elevation = 0; elevation <= 20; elevation++)
					{
						for (int moisture = 0; moisture <= 20; moisture++)
						{
							float m0 = moisture / 20f;
							float m1 = (moisture + 1) / 20f;
							if (m0 <= zone.MoistureMax && m1 >= zone.MoistureMin)
							{
								int lookupIndex = elevation * 21 + moisture;
								if (!mBiomeLookUps[lookupIndex].OverlappingZones.Contains(zone))
								{
									mBiomeLookUps[lookupIndex].OverlappingZones.Add(zone);
									if (mBiomeLookUps[lookupIndex].OverlappingZones.Count > 1)
									{
										mBiomeLookUps[lookupIndex].UniqueZone = false;
									}
								}

								mBiomeLookUps[lookupIndex].Zone = zone;
								if (zone.MoistureMin > m0 || zone.MoistureMax < 1)
								{
									mBiomeLookUps[lookupIndex].UniqueZone = false;
								}
							}
						}
					}
				}
			}

			for (int i = 0; i < mBiomeLookUps.Length; i++)
			{
				if (!mBiomeLookUps[i].UniqueZone && !mBiomeLookUps[i].OverlappingZones.Contains(defaultBiome))
				{
					mBiomeLookUps[i].OverlappingZones.Add(defaultBiome);
				}
			}

		}

		private void CheckNewNearChunks()
		{
			for (int x = mVisibleXMin; x <= mVisibleXMax; x++)
			{
				int x00 = Schematic.MAX_WORLD_LENGTH * Schematic.MAX_WORLD_HEIGHT * (x + Schematic.MAX_WORLD_WIDTH);
				for (int y = mVisibleYMin; y <= mVisibleYMax; y++)
				{
					int y00 = Schematic.MAX_WORLD_LENGTH * (y + Schematic.MAX_WORLD_HEIGHT);
					int h00 = x00 + y00;
					for (int z = mVisibleZMin; z <= mVisibleZMax; z++)
					{
						int hash = h00 + z;
						CreateChunk(hash, x, y, z);
					}
				}
			}
		}
		#endregion


	}
}
