using System;
using System.Collections.Generic;
using FileToVox.Generator.Terrain.Data;
using FileToVox.Generator.Terrain.Entities;
using FileToVox.Generator.Terrain.Utility;
using FileToVox.Schematics.Tools;
using Microsoft.VisualBasic.CompilerServices;

namespace FileToVox.Generator.Terrain
{
	public class Octree
	{
		public Vector3 Center;
		public int Extents;
		public Octree Parent;
		public Octree[] Children;
		public int ExploredChildren;
		public bool Explored;

		public Octree(Octree parent, Vector3 center, int extents)
		{
			Parent = parent;
			Center = center;
			Extents = extents;
		}

		public void Explode()
		{
			Children = new Octree[8];
			int half = Extents / 2;
			Children[0] = new Octree(this, new Vector3(Center.X - half, Center.Y + half, Center.Z + half), half);
			Children[1] = new Octree(this, new Vector3(Center.X + half, Center.Y + half, Center.Z + half), half);
			Children[2] = new Octree(this, new Vector3(Center.X - half, Center.Y + half, Center.Z - half), half);
			Children[3] = new Octree(this, new Vector3(Center.X + half, Center.Y + half, Center.Z - half), half);
			Children[4] = new Octree(this, new Vector3(Center.X - half, Center.Y - half, Center.Z + half), half);
			Children[5] = new Octree(this, new Vector3(Center.X + half, Center.Y - half, Center.Z + half), half);
			Children[6] = new Octree(this, new Vector3(Center.X - half, Center.Y - half, Center.Z - half), half);
			Children[7] = new Octree(this, new Vector3(Center.X + half, Center.Y - half, Center.Z - half), half);
		}
	}

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
		private Dictionary<int, CachedChunk> mCachedChunks;
		private Octree[] mChunkRequests;
		private int mChunkRequestLast;
		private const int CHUNKS_CREATION_BUFFER_SIZE = 15000;


		private Dictionary<Vector3, Octree> mOctreeRoots;
		private int mOctreeSize;


		private HeightMapCache mHeightMapCache;
		private BiomeLookUp[] mBiomeLookUps;

		public void InitChunkManager()
		{
			mCachedChunks = new Dictionary<int, CachedChunk>();
			mChunkRequests = new Octree[CHUNKS_CREATION_BUFFER_SIZE];
			mChunkRequestLast = -1;

			InitOctrees();
			InitHeightMap();
			InitBiomes();

			if (mWorldTerrainData != null)
			{
				if (mWorldTerrainData.TerrainGeneratorSetttings == null)
				{
					mWorldTerrainData.TerrainGeneratorSetttings = new TerrainGeneratorSettings(); //TODO
				}

				mWorldTerrainData.TerrainGeneratorSetttings.Initialize();
			}
		}

		#region PrivateMethods

		private void InitOctrees()
		{
			mOctreeRoots = new Dictionary<Vector3, Octree>();
			mOctreeSize = 60 * CHUNK_SIZE * 2; //60: max value -> 60 * 16 *2 < 2000
			float l2 = MathF.Log(mOctreeSize, 2);
			mOctreeSize = (int) MathF.Pow(2, MathF.Ceiling(l2));
		}

		private void InitHeightMap()
		{
			int poolSize = (30 + 10) * 2 * CHUNK_SIZE / 128 + 1;
			poolSize *= poolSize;
			mHeightMapCache = new HeightMapCache(poolSize);
		}

		private void InitBiomes()
		{
			mBiomeLookUps = new BiomeLookUp[441];
			if (mWorldTerrainData == null)
				return;

			if (mWorldTerrainData.Biomes == null || mWorldTerrainData.Biomes.Length == 0)
			{
				mWorldTerrainData.DefaultBiome = new BiomeSettings(); //TODO
			}

			BiomeZone defaultBiome = new BiomeZone();
			if (mWorldTerrainData.DefaultBiome != null)
			{
				mWorldTerrainData.DefaultBiome.ValidateSettings(mWorldTerrainData);
				if (mWorldTerrainData.DefaultBiome.Zones != null && mWorldTerrainData.DefaultBiome.Zones.Length > 0)
				{
					defaultBiome = mWorldTerrainData.DefaultBiome.Zones[0];
				}
			}

			for (int i = 0; i < mBiomeLookUps.Length; i++)
			{
				mBiomeLookUps[i] = new BiomeLookUp(defaultBiome, true);
			}

			if (mWorldTerrainData.Biomes == null)
				return;

			float maxHeight = mWorldTerrainData.TerrainGeneratorSetttings?.MaxHeight ?? 1000;
			for (int i = 0; i < mWorldTerrainData.Biomes.Length; i++)
			{
				BiomeSettings biome = mWorldTerrainData.Biomes[i];
				if (biome == null)
					continue;
				biome.ValidateSettings(mWorldTerrainData);
				if (biome.Zones == null)
					continue;

				for (int j = 0; j <= biome.Zones.Length; j++)
				{
					BiomeZone zone = biome.Zones[j];
					for (int elevation = 0; elevation <= 20; elevation++)
					{
						float e0 = maxHeight * elevation / 20f;
						float e1 = maxHeight * (elevation + 1) / 20f;
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

		#endregion
	}
}
