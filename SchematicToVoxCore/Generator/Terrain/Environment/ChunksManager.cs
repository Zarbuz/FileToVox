using System;
using System.Collections.Generic;
using FileToVox.Generator.Terrain.Data;
using FileToVox.Generator.Terrain.Entities;
using FileToVox.Generator.Terrain.Utility;
using FileToVox.Schematics.Tools;

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
		public BiomeData Data;
		public bool UniqueZone;
		public List<BiomeData> OverlappingZones = new List<BiomeData>();

		public BiomeLookUp(BiomeData biome, bool uniqueZone)
		{
			Data = biome;
			UniqueZone = uniqueZone;
		}

		public BiomeDefinition GetBiome(float altitude, float moisture)
		{
			if (UniqueZone)
			{
				return Data.Biome;
			}

			for (int i = 0; i < OverlappingZones.Count; i++)
			{
				if (altitude >= OverlappingZones[i].AltitudeMin && altitude < OverlappingZones[i].AltitudeMax &&
				    moisture >= OverlappingZones[i].MoistureMin && moisture < OverlappingZones[i].MoistureMax)
				{
					return OverlappingZones[i].Biome;
				}
			}

			return Data.Biome;
		}
	}

	public struct HeightMapInfo
	{
		public float Moisture;
		public int GroundLevel;
		public BiomeDefinition Biome;
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
				mWorldTerrainData.DefaultBiome = new BiomeDefinition();
			}
		}

		#endregion
	}
}
