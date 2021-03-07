using System;
using System.Collections.Generic;
using System.Text;

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
		private HashSet<int> mSectors;
		private int mLastKey;
		private int mLastSector;


		public HeightMapCache(int poolSize)
		{
			mSectors = new HashSet<int>(16);
			mSectorsPool = new HeightMapInfoPoolEntry[poolSize];
			for (int i = 0; i < mSectorsPool.Length; i++)
			{
				mSectorsPool[i] = new HeightMapInfoPoolEntry();
			}

			mLastSector = -1;
		}
	}
}
