namespace FileToVox.Generator.Terrain.Utility
{
	public class HeightMapCache
	{
		private class HeightMapInfoPoolEntry
		{
			public int Uses;
			public int Key;
			public HeightMapInfo[] Heights;
		}

		private HeightMapInfoPoolEntry[] mSectorsPool;
		private FastHashSet<int> mSectors;
		private int mLastKey;
		private int mLastSector;


		public HeightMapCache(int poolSize)
		{
			mSectors = new FastHashSet<int>(16);
			mSectorsPool = new HeightMapInfoPoolEntry[poolSize];
			for (int i = 0; i < mSectorsPool.Length; i++)
			{
				mSectorsPool[i] = new HeightMapInfoPoolEntry();
			}

			mLastSector = -1;
		}

		public bool TryGetValue(int x, int z, out HeightMapInfo[] Heights, out int heightIndex)
		{
			int fx = x >> 7;
			int fz = z >> 7;
			heightIndex = ((z - (fz << 7)) << 7) + (x - (fx << 7));
			int Key = ((fz + 1024) << 16) + (fx + 1024);
			if (Key != mLastKey || mLastSector < 0)
			{
				if (!mSectors.TryGetValue(Key, out int poolIndex) || Key != mSectorsPool[poolIndex].Key)
				{
					int leastUsed = int.MaxValue;
					for (int k = 0; k < mSectorsPool.Length; k++)
					{
						if (mSectorsPool[k].Uses < leastUsed)
						{
							leastUsed = mSectorsPool[k].Uses;
							poolIndex = k;
						}
					}

					HeightMapInfoPoolEntry sector = mSectorsPool[poolIndex];
					if (sector.Key > 0)
					{
						mSectors.Remove(sector.Key);
					}

					sector.Key = Key;
					sector.Uses = 0;
					mSectors[Key] = poolIndex;

					if (sector.Heights == null)
					{
						sector.Heights = new HeightMapInfo[16384];
					}
					else
					{
						for (int k = 0; k < sector.Heights.Length; k++)
						{
							sector.Heights[k].Biome = null;
							sector.Heights[k].Moisture = 0;
							sector.Heights[k].GroundLevel = 0;
						}
					}
				}
				mLastKey = Key;
				mLastSector = poolIndex;
			}

			HeightMapInfoPoolEntry theSector = mSectorsPool[mLastSector];
			theSector.Uses++;
			Heights = theSector.Heights;
			return Heights[heightIndex].GroundLevel != 0;
		}
	}
}
