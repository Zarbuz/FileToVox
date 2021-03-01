using FileToVox.Generator.Terrain.Data;
using System.Drawing;
using FileToVox.Generator.Terrain.Chunk;
using FileToVox.Schematics.Tools;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Generator.Terrain
{
	public partial class TerrainEnvironment
	{
		private struct VegetationRequest
		{
			public VoxelChunk Chunk;
			public int VoxelIndex;
			public Color vd;
			public Vector3 ChunkOriginalPosition;
		}

		private VegetationRequest[] mVegetationRequests;

		private int mVegetationRequestLast, mVegetationRequestFirst;
		private const int VEGETATION_CREATION_BUFFER_SIZE = 20000;

		private void InitVegetation()
		{
			if (mVegetationRequests == null || mVegetationRequests.Length != VEGETATION_CREATION_BUFFER_SIZE)
			{
				mVegetationRequests = new VegetationRequest[VEGETATION_CREATION_BUFFER_SIZE];
			}
			mVegetationRequestLast = -1;
			mVegetationRequestFirst = -1;
		}

		public void RequestVegetationCreation(VoxelChunk chunk, int voxelIndex, Color vd)
		{
			if (chunk == null)
			{
				return;
			}
			mVegetationRequestLast++;
			if (mVegetationRequestLast >= mVegetationRequests.Length)
			{
				mVegetationRequestLast = 0;
			}
			if (mVegetationRequestLast != mVegetationRequestFirst)
			{
				mVegetationRequests[mVegetationRequestLast].Chunk = chunk;
				mVegetationRequests[mVegetationRequestLast].ChunkOriginalPosition = chunk.Position;
				mVegetationRequests[mVegetationRequestLast].VoxelIndex = voxelIndex;
				mVegetationRequests[mVegetationRequestLast].vd = vd;
			}
		}

		void CheckVegetationRequests()
		{
			for (int k = 0; k < 10000; k++)
			{
				if (mVegetationRequestFirst == mVegetationRequestLast)
					return;
				mVegetationRequestFirst++;
				if (mVegetationRequestFirst >= mVegetationRequests.Length)
				{
					mVegetationRequestFirst = 0;
				}
				VoxelChunk chunk = mVegetationRequests[mVegetationRequestFirst].Chunk;
				if (chunk != null && chunk.Position == mVegetationRequests[mVegetationRequestFirst].ChunkOriginalPosition)
				{
					CreateVegetation(chunk, mVegetationRequests[mVegetationRequestFirst].VoxelIndex, mVegetationRequests[mVegetationRequestFirst].vd);
				}
			}
		}

		public Color GetVegetation(BiomeSettings biome, float random)
		{
			float acumProb = 0;
			int index = 0;
			for (int t = 0; t < biome.Vegetation.Length; t++)
			{
				acumProb += biome.Vegetation[t].Probability;
				if (random < acumProb)
				{
					index = t;
					break;
				}
			}
			return biome.Vegetation[index].Color;
		}

		private void CreateVegetation(VoxelChunk chunk, int voxelIndex, Color vd)
		{
			if (chunk != null)
			{
				// Updates current chunk
				if (chunk.Voxels[voxelIndex].Color == 0)
				{
					chunk.Voxels[voxelIndex].Color = vd.ColorToUInt();
					//ChunkRequestRefresh(chunk, false, true);
				}
			}
		}
	}
}
