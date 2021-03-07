using FileToVox.Schematics.Tools;
using System;

namespace FileToVox.Generator.Terrain.Utility
{
	public static class WorldRandom
	{
		private const int RANDOM_TABLE_SIZE = 8192; // 2^13
		private const int RANDOM_TABLE_SIZE_MINUS_ONE = RANDOM_TABLE_SIZE - 1;
		private const uint MAGIC1 = 2166136261; // 17
		private const uint MAGIC2 = 16777619;   // 23

		private static float[] RND;
		private static uint RND_INDEX = 0;

		static WorldRandom()
		{
			Randomize(0);
		}

		public static void Randomize(int seed)
		{
			Random random = new Random(seed);
			if (RND == null || RND.Length == 0)
				RND = new float[RANDOM_TABLE_SIZE];
			for (int k = 0; k < RND.Length; k++)
			{
				do
				{
					RND[k] = (float) random.NextDouble();
				} while (RND[k] == 1f);
			}
		}

		public static float GetValue(Vector3 position)
		{
			uint hash = MAGIC1;
			hash = hash * MAGIC2 ^ (uint)position.X;
			hash = hash * MAGIC2 ^ (uint)position.Y;
			hash = hash * MAGIC2 ^ (uint)position.Z;
			RND_INDEX = hash & RANDOM_TABLE_SIZE_MINUS_ONE;
			return RND[RND_INDEX];
		}

		public static float GetValue(float x, float z)
		{
			uint hash = MAGIC1;
			hash = hash * MAGIC2 ^ (uint)x;
			hash = hash * MAGIC2 ^ (uint)z;
			RND_INDEX = hash & RANDOM_TABLE_SIZE_MINUS_ONE;
			return RND[RND_INDEX];
		}

		public static float GetValue(int someValue)
		{
			RND_INDEX = (uint)someValue & RANDOM_TABLE_SIZE_MINUS_ONE;
			return RND[RND_INDEX];
		}

		public static int Range(int min, int max, Vector3 position)
		{
			float v = GetValue(position);
			return (int)(min + (max - min) * 0.99999f * v);
		}
		public static int Range(int min, int max)
		{
			float v = GetValue();
			return (int)(min + (max - min) * 0.99999f * v);
		}

		public static float Range(float min, float max)
		{
			float v = GetValue();
			return min + (max - min) * v;
		}

		public static float Range(float min, float max, int seed)
		{
			float v = GetValue(seed);
			return min + (max - min) * v;
		}

		public static float GetValue()
		{
			RND_INDEX++;
			RND_INDEX &= RANDOM_TABLE_SIZE_MINUS_ONE;
			return RND[RND_INDEX];
		}

		public static Vector3 GetVector3(Vector3 position, float scale, float shift = 0)
		{
			float x = (GetValue(position) + shift) * scale;
			float y = (GetValue() + shift) * scale;
			float z = (GetValue() + shift) * scale;
			return new Vector3(x, y, z);
		}

		public static Vector3 GetVector3(Vector3 position, Vector3 scale, float shift = 0)
		{
			float x = (GetValue(position) + shift) * scale.X;
			float y = (GetValue() + shift) * scale.Y;
			float z = (GetValue() + shift) * scale.Z;
			return new Vector3(x, y, z);
		}
	}
}
