using System.Runtime.CompilerServices;
using FileToVox.Generator.Terrain.Chunk;
using FileToVox.Generator.Terrain.Entities;
using FileToVox.Schematics;
using FileToVox.Schematics.Tools;

namespace FileToVox.Generator.Terrain
{
	public partial class TerrainEnvironment
	{
		private readonly object mLockLastChunkFetch = new object();
		private int mLastChunkFetchX, mLastChunkFetchY, mLastChunkFetchZ;
		private VoxelChunk mLastChunkFetch;

		private VoxelChunk CreateChunk(int hash, int chunkX, int chunkY, int chunkZ)
		{
			Vector3 position = new Vector3(chunkX * CHUNK_SIZE + CHUNK_HALF_SIZE, chunkY * CHUNK_SIZE + CHUNK_HALF_SIZE, chunkZ * CHUNK_SIZE + CHUNK_HALF_SIZE);

			if (!mCachedChunks.TryGetValue(hash, out CachedChunk cachedChunk))
			{
				cachedChunk = new CachedChunk();
				mCachedChunks[hash] = cachedChunk;
			}

			VoxelChunk chunk = new VoxelChunk();
			chunk.Voxels = new Voxel[CHUNK_SIZE * CHUNK_SIZE * CHUNK_SIZE];
			chunk.Position = position;

			WorldTerrainData.TerrainGeneratorSettings.PaintChunk(chunk);
			cachedChunk.Chunk = chunk;
			return chunk;
		}

		[MethodImpl(256)] // equals to MethodImplOptions.AggressiveInlining
		private int GetChunkHash(int chunkX, int chunkY, int chunkZ)
		{
			int x00 = Schematic.MAX_WORLD_LENGTH * Schematic.MAX_WORLD_HEIGHT * (chunkX + Schematic.MAX_WORLD_WIDTH);
			int y00 = Schematic.MAX_WORLD_LENGTH * (chunkY + Schematic.MAX_WORLD_HEIGHT);
			return x00 + y00 + chunkZ;
		}

		private bool GetChunkFast(int chunkX, int chunkY, int chunkZ, out VoxelChunk chunk, bool createIfNotAvailable = false)
		{
			lock (mLockLastChunkFetch)
			{
				if (mLastChunkFetchX == chunkX && mLastChunkFetchY == chunkY && mLastChunkFetchZ == chunkZ && mLastChunkFetch != null)
				{
					chunk = mLastChunkFetch;
					return true;
				}
			}
			int hash = GetChunkHash(chunkX, chunkY, chunkZ);
			bool exists = mCachedChunks.TryGetValue(hash, out CachedChunk cachedChunk);
			chunk = exists ? cachedChunk.Chunk : null;

			if (createIfNotAvailable)
			{
				if (!exists)
				{
					chunk = CreateChunk(hash, chunkX, chunkY, chunkZ);
					exists = true;
				}
				if (chunk == null)
				{ 
					chunk = CreateChunk(hash, chunkX, chunkY, chunkZ);
				}
			}
			if (exists)
			{
				lock (mLockLastChunkFetch)
				{
					mLastChunkFetchX = chunkX;
					mLastChunkFetchY = chunkY;
					mLastChunkFetchZ = chunkZ;
					mLastChunkFetch = chunk;
				}
				return chunk != null;
			}
			chunk = null;
			return false;
		}
	}
}
