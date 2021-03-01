using System;
using System.Collections.Generic;
using System.Drawing;
using FileToVox.Generator.Terrain.Chunk;
using FileToVox.Generator.Terrain.Data;
using FileToVox.Schematics.Tools;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Generator.Terrain
{
	public partial class TerrainEnvironment
	{
		private struct TreeRequest
		{
			public VoxelChunk Chunk;
			public Vector3 ChunkOriginalPosition;
			public Vector3 RootPosition;
			public ModelSettings Tree;
		}

		private const int TREES_CREATION_BUFFER_SIZE = 20000;
		private TreeRequest[] mTreeRequests;
		private int mTreeRequestLast, mTreeRequestFirst;
		private List<VoxelChunk> mTreeChunkRefreshRequests;

		private void InitTrees()
		{
			if (mTreeRequests == null || mTreeRequests.Length != TREES_CREATION_BUFFER_SIZE)
			{
				mTreeRequests = new TreeRequest[TREES_CREATION_BUFFER_SIZE];
			}
			mTreeRequestLast = -1;
			mTreeRequestFirst = -1;
			if (mTreeChunkRefreshRequests == null)
			{
				mTreeChunkRefreshRequests = new List<VoxelChunk>();
			}
			else
			{
				mTreeChunkRefreshRequests.Clear();
			}
		}

		public ModelSettings GetTree(BiomeTree[] trees, float random)
		{
			float acumProb = 0;
			int index = 0;
			for (int t = 0; t < trees.Length; t++)
			{
				acumProb += trees[t].Probability;
				if (random < acumProb)
				{
					index = t;
					break;
				}
			}

			return trees[index].Bits;
		}

		public void RequestTreeCreation(VoxelChunk chunk, Vector3 position, ModelSettings treeModel)
		{
			if (treeModel == null)
				return;

			mTreeRequestLast++;
			if (mTreeRequestLast >= mTreeRequests.Length)
			{
				mTreeRequestLast = 0;
			}
			if (mTreeRequestLast != mTreeRequestFirst)
			{
				mTreeRequests[mTreeRequestLast].Chunk = chunk;
				mTreeRequests[mTreeRequestLast].ChunkOriginalPosition = chunk.Position;
				mTreeRequests[mTreeRequestLast].RootPosition = position;
				mTreeRequests[mTreeRequestLast].Tree = treeModel;
			}
		}

		private void CheckTreeRequests()
		{
			for (int k = 0; k < 10000; k++)
			{
				if (mTreeRequestFirst == mTreeRequestLast)
					return;
				mTreeRequestFirst++;
				if (mTreeRequestFirst >= mTreeRequests.Length)
				{
					mTreeRequestFirst = 0;
				}
				VoxelChunk chunk = mTreeRequests[mTreeRequestFirst].Chunk;
				if (chunk != null && chunk.Position == mTreeRequests[mTreeRequestFirst].ChunkOriginalPosition)
				{
					Random random = new Random();
					CreateTree(mTreeRequests[mTreeRequestFirst].RootPosition, mTreeRequests[mTreeRequestFirst].Tree, random.Next(0, 4));
				}
			}
		}

		private void CreateTree(Vector3 position, ModelSettings tree, int rotation)
		{
			if (tree == null)
			{
				return;
			}
			
			Vector3 pos = new Vector3();
			mTreeChunkRefreshRequests.Clear();
			VoxelChunk lastChunk = null;
			int modelOneYRow = tree.SizeZ * tree.SizeX;
			int modelOneZRow = tree.SizeX;
			int halfSizeX = tree.SizeX / 2;
			int halfSizeZ = tree.SizeZ / 2;

			for (int b = 0; b < tree.Bits.Length; b++)
			{
				int bitIndex = tree.Bits[b].VoxelIndex;
				int py = bitIndex / modelOneYRow;
				int remy = bitIndex - py * modelOneYRow;
				int pz = remy / modelOneZRow;
				int px = remy - pz * modelOneZRow;

				// Random rotation
				int tmp;
				switch (rotation)
				{
					case 0:
						tmp = px;
						px = halfSizeZ - pz;
						pz = tmp - halfSizeX;
						break;
					case 1:
						tmp = px;
						px = pz - halfSizeZ;
						pz = tmp - halfSizeX;
						break;
					case 2:
						tmp = px;
						px = pz - halfSizeZ;
						pz = halfSizeX - tmp;
						break;
					default:
						px -= halfSizeX;
						pz -= halfSizeZ;
						break;
				}

				pos.X = position.X + tree.OffsetX + px;
				pos.Y = position.Y + tree.OffsetY + py;
				pos.Z = position.Z + tree.OffsetZ + pz;

				if (GetVoxelIndex(pos, out VoxelChunk chunk, out int voxelIndex))
				{
					if (chunk.Voxels[voxelIndex].Color != 0)
					{
						chunk.Voxels[voxelIndex].Color = tree.Bits[b].Color.ColorToUInt();
						if (py == 0)
						{
							if (voxelIndex >= ONE_Y_ROW)
							{
								if (chunk.Voxels[voxelIndex - ONE_Y_ROW].Color != 0)
								{
									chunk.Voxels[voxelIndex - ONE_Y_ROW].Color = tree.Bits[b].Color.ColorToUInt();
								}
							}
							else
							{
								VoxelChunk bottom = chunk.Bottom;
								if (bottom != null)
								{
									int bottomIndex = voxelIndex + (CHUNK_SIZE - 1) * ONE_Y_ROW;
									if (bottom.Voxels[bottomIndex].Color != 0)
									{
										chunk.Voxels[bottomIndex].Color = tree.Bits[b].Color.ColorToUInt();
										
										if (!mTreeChunkRefreshRequests.Contains(bottom))
											mTreeChunkRefreshRequests.Add(bottom);
									}
								}
							}
						}
						if (chunk != lastChunk)
						{
							lastChunk = chunk;
							if (!mTreeChunkRefreshRequests.Contains(chunk))
							{
								mTreeChunkRefreshRequests.Add(chunk);
							}
						}
					}
				}
			}

			
		}
	}
}
