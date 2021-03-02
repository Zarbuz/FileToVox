using FileToVox.Generator.Terrain.Chunk;
using FileToVox.Generator.Terrain.Data;
using FileToVox.Generator.Terrain.Utility;
using FileToVox.Schematics.Tools;
using FileToVox.Utils;
using System;
using System.Drawing;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Generator.Terrain
{
	public partial class TerrainEnvironment : SimpleSingleton<TerrainEnvironment>
	{
		public const int CHUNK_SIZE = 16;
		public const int CHUNK_HALF_SIZE = CHUNK_SIZE / 2;
		public const int ONE_Y_ROW = CHUNK_SIZE * CHUNK_SIZE;
		public const int ONE_Z_ROW = CHUNK_SIZE;

		public WorldTerrainData WorldTerrainData { get; private set; }

		public void MainInitialize(WorldTerrainData terrainData)
		{
			WorldTerrainData = terrainData;
			WorldRandom.Randomize(WorldTerrainData.Seed);
			InitTrees();
			InitVegetation();
			InitChunkManager();
		}

		public void StartGeneration()
		{
			CheckChunksInRange();
			CheckTreeRequests();
			CheckVegetationRequests();
		}

		public bool GetChunk(Vector3 position, out VoxelChunk chunk, bool forceCreation = false)
		{
			FastMath.FloorToInt(position.X / CHUNK_SIZE, position.Y / CHUNK_SIZE, position.Z / CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);
			return GetChunkFast(chunkX, chunkY, chunkZ, out chunk, forceCreation);
		}

		public bool GetVoxelIndex(Vector3 position, out VoxelChunk chunk, out int voxelIndex, bool createChunkIfNotExists = true)
		{
			FastMath.FloorToInt(position.X / CHUNK_SIZE, position.Y / CHUNK_SIZE, position.Z / CHUNK_SIZE, out int chunkX, out int chunkY, out int chunkZ);

			if (GetChunkFast(chunkX, chunkY, chunkZ, out chunk, createChunkIfNotExists))
			{
				int py = (int)(position.Y - chunkY * CHUNK_SIZE);
				int pz = (int)(position.Z - chunkZ * CHUNK_SIZE);
				int px = (int)(position.X - chunkX * CHUNK_SIZE);
				voxelIndex = py * ONE_Y_ROW + pz * ONE_Z_ROW + px;
				return true;
			}

			voxelIndex = 0;
			return false;
		}

		public Vector3 GetChunkPosition(Vector3 position)
		{
			FastMath.FloorToInt(position.X / CHUNK_SIZE, position.Y / CHUNK_SIZE, position.Z / CHUNK_SIZE, out int x, out int y, out int z);

			x = x * CHUNK_SIZE + CHUNK_HALF_SIZE;
			y = y * CHUNK_SIZE + CHUNK_HALF_SIZE;
			z = z * CHUNK_SIZE + CHUNK_HALF_SIZE;
			return new Vector3(x, y, z);
		}

		public uint[,,] GetVoxels(Vector3 boxMin, Vector3 boxMax)
		{
			Vector3 position = new Vector3();
			Vector3 chunkMinPos = GetChunkPosition(boxMin);
			Vector3 chunkMaxPos = GetChunkPosition(boxMax);

			int minX = (int)boxMin.X;
			int minY = (int)boxMin.Y;
			int minZ = (int)boxMin.Z;
			int maxX = (int)boxMax.X;
			int maxY = (int)boxMax.Y;
			int maxZ = (int)boxMax.Z;

			int sizeY = maxY - minY;
			int sizeZ = maxZ - minZ;
			int sizeX = maxX - minX;

			uint[,,] voxels = new uint[sizeY + 1, sizeZ + 1, sizeX + 1];

			for (float y = chunkMinPos.Y; y <= chunkMaxPos.Y; y += CHUNK_SIZE)
			{
				position.Y = y;
				for (float z = chunkMinPos.Z; z <= chunkMaxPos.Z; z += CHUNK_SIZE)
				{
					position.Z = z;
					for (float x = chunkMinPos.X; x <= chunkMaxPos.X; x += CHUNK_SIZE)
					{
						position.X = x;
						if (GetChunk(position, out VoxelChunk chunk, false))
						{
							FastMath.FloorToInt(chunk.Position.X, chunk.Position.Y, chunk.Position.Z, out int chunkMinX, out int chunkMinY, out int chunkMinZ);
							chunkMinX -= CHUNK_HALF_SIZE;
							chunkMinY -= CHUNK_HALF_SIZE;
							chunkMinZ -= CHUNK_HALF_SIZE;
							for (int vy = 0; vy < CHUNK_SIZE; vy++)
							{
								int wy = chunkMinY + vy;
								if (wy < minY || wy > maxY)
									continue;
								int my = wy - minY;
								int voxelIndexY = vy * ONE_Y_ROW;
								for (int vz = 0; vz < CHUNK_SIZE; vz++)
								{
									int wz = chunkMinZ + vz;
									if (wz < minZ || wz > maxZ)
										continue;
									int mz = wz - minZ;
									int voxelIndex = voxelIndexY + vz * ONE_Z_ROW;
									for (int vx = 0; vx < CHUNK_SIZE; vx++, voxelIndex++)
									{
										int wx = chunkMinX + vx;
										if (wx < minX || wx > maxX)
											continue;
										int mx = wx - minX;
										voxels[my, mz, mx] = chunk.Voxels[voxelIndex].Color;
									}
								}
							}
						}
						else
						{
							int chunkMinY = (int)Math.Floor(y) - CHUNK_HALF_SIZE;
							int chunkMinZ = (int)Math.Floor(z) - CHUNK_HALF_SIZE;
							int chunkMinX = (int)Math.Floor(x) - CHUNK_HALF_SIZE;
							for (int vy = 0; vy < CHUNK_SIZE; vy++)
							{
								int wy = chunkMinY + vy;
								if (wy < minY || wy > maxY)
									continue;
								int my = wy - minY;
								for (int vz = 0; vz < CHUNK_SIZE; vz++)
								{
									int wz = chunkMinZ + vz;
									if (wz < minZ || wz > maxZ)
										continue;
									int mz = wz - minZ;
									for (int vx = 0; vx < CHUNK_SIZE; vx++)
									{
										int wx = chunkMinX + vx;
										if (wx < minX || wx > maxX)
											continue;
										int mx = wx - minX;
										voxels[my, mz, mx] = 0;
									}
								}
							}
						}
					}
				}
			}

			return voxels;
		}
	}
}
