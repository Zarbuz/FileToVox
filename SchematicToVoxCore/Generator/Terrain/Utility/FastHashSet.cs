using System;
using System.Collections.Generic;
using System.Text;

namespace FileToVox.Generator.Terrain.Utility
{
	public class FastHashSet<V>
	{
		private int[] mHashes;
		public DictionaryEntry[] Entries;
		private readonly int mInitialsize = 89;
		private const float Loadfactor = 1f;

		private static readonly int[] PrimeSizes = new int[] { 89, 179, 359, 719, 1439, 2879, 5779, 11579, 23159, 46327,
			92657, 185323, 370661, 741337, 1482707, 2965421, 5930887, 11861791,
			23723599, 47447201, 94894427, 189788857, 379577741, 759155483
		};

		public struct DictionaryEntry
		{
			public int key;
			public int next;
			public V value;
		}


		public FastHashSet()
		{
			Initialize();
		}

		public FastHashSet(int capacity)
		{
			mInitialsize = FindNewSize(capacity);
			Initialize();
		}

		public V GetAtPosition(int pos)
		{
			return Entries[pos].value;
		}

		public void StoreAtPosition(int pos, V value)
		{
			Entries[pos].value = value;
		}

		public int Add(object keyObj, V value, bool overwrite = true)
		{
			return Add(keyObj.GetHashCode(), value, overwrite);
		}

		public int Add(int key, V value, bool overwrite = true)
		{
			if (Count >= Entries.Length)
			{
				Resize();
			}


			key &= 0x7FFFFFFF;

			int hashPos = key % mHashes.Length;

			int entryLocation = mHashes[hashPos];

			int storePos = Count;


			if (entryLocation != -1)
			{ // already there
				int currEntryPos = entryLocation;

				do
				{
					DictionaryEntry entry = Entries[currEntryPos];

					// same key is in the dictionary
					if (key == entry.key)
					{
						if (overwrite)
						{
							Entries[currEntryPos].value = value;
						}
						return currEntryPos;
					}

					currEntryPos = entry.next;
				} while (currEntryPos > -1);
			}

			// new value
			mHashes[hashPos] = storePos;

			Entries[storePos].next = entryLocation;
			Entries[storePos].key = key;
			Entries[storePos].value = value;

			Count++;

			return storePos;
		}

		private void Resize()
		{
			int newSize = FindNewSize(mHashes.Length * 2 + 1);
			int[] newHashes = new int[newSize];
			DictionaryEntry[] newEntries = new DictionaryEntry[newSize];

			Array.Copy(Entries, newEntries, Count);

			for (int i = 0; i < newSize; i++)
			{
				newHashes[i] = -1;
			}
			for (int i = Count; i < newSize; i++)
			{
				newEntries[i].key = -1;
			}

			for (int i = 0; i < Count; i++)
			{
				int key = newEntries[i].key;
				if (key >= 0)
				{
					int hashPos = key % newSize;
					int curPos = newHashes[hashPos];
					newHashes[hashPos] = i;
					newEntries[i].next = curPos;
				}
			}

			mHashes = newHashes;
			Entries = newEntries;
		}

		private int FindNewSize(int desiredCapacity)
		{
			for (int i = 0; i < PrimeSizes.Length; i++)
			{
				if (PrimeSizes[i] >= desiredCapacity)
					return PrimeSizes[i];
			}

			throw new NotImplementedException("Too large array");
		}

		public V Get(int key)
		{
			int pos = GetPosition(key);

			if (pos == -1)
				throw new Exception("Key does not exist");

			return Entries[pos].value;
		}

		public int GetPosition(int key)
		{
			key &= 0x7FFFFFFF;

			int pos = key % mHashes.Length;

			int entryLocation = mHashes[pos];

			if (entryLocation == -1)
				return -1;

			int nextpos = entryLocation;

			do
			{
				DictionaryEntry entry = Entries[nextpos];

				if (key == entry.key)
					return nextpos;

				nextpos = entry.next;

			} while (nextpos != -1);

			return -1;
		}

		public bool ContainsKey(int key)
		{
			return GetPosition(key) != -1;
		}

		public bool TryGetValue(int key, out V value)
		{
			int pos = GetPosition(key);

			if (pos == -1)
			{
				value = default(V);
				return false;
			}

			value = Entries[pos].value;

			return true;
		}

		public V this[int key]
		{
			get => Get(key);
			set => Add(key, value, true);
		}

		public void Add(KeyValuePair<int, V> item)
		{
			Add(item.Key, item.Value, false);
		}

		public void Clear()
		{
			Count = 0;
			for (int i = 0; i < mHashes.Length; i++)
			{
				mHashes[i] = -1;
			}
		}

		private void Initialize()
		{
			this.mHashes = new int[mInitialsize];
			this.Entries = new DictionaryEntry[mInitialsize];
			Count = 0;

			for (int i = 0; i < Entries.Length; i++)
			{
				mHashes[i] = -1;
				Entries[i].key = -1;
			}
		}

		public int Count { get; private set; }

		public bool IsReadOnly
		{
			get { return false; }
		}


		public void Remove(object keyObj)
		{
			uint key = (uint)keyObj.GetHashCode();
			Remove((int)key);
		}


		public void Remove(int key)
		{
			key &= 0x7FFFFFFF;
			int hashPos = key % mHashes.Length;

			int entryLocation = mHashes[hashPos];

			if (entryLocation == -1)
				return;


			int currEntryPos = entryLocation;
			int prevEntryPos = entryLocation;

			do
			{
				DictionaryEntry entry = Entries[currEntryPos];

				// key is in the dictionary
				if (key == entry.key)
				{
					Entries[currEntryPos].key = -1;
					Entries[prevEntryPos].next = Entries[currEntryPos].next;
					if (entryLocation == currEntryPos)
					{
						mHashes[hashPos] = entry.next;
						if (entryLocation + 1 == Count)
						{
							Count--;
						}
					}
					return;
				}

				prevEntryPos = currEntryPos;
				currEntryPos = entry.next;

			} while (currEntryPos > -1);
		}




	}
}
