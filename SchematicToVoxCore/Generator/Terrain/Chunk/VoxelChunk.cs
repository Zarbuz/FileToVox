using FileToVoxCore.Schematics;
using FileToVoxCore.Schematics.Tools;

namespace FileToVox.Generator.Terrain.Chunk
{
	public class VoxelChunk
	{
		public Voxel[] Voxels;
		public Vector3 Position;

		private VoxelChunk mTop;
		private VoxelChunk mBottom;
		private VoxelChunk mLeft;
		private VoxelChunk mForward;
		private VoxelChunk mRight;
		private VoxelChunk mBack;

		public VoxelChunk Top
		{
			get
			{
				if (mTop == null)
				{
					Vector3 topPosition = Position;
					topPosition.Y += TerrainEnvironment.CHUNK_SIZE;
					TerrainEnvironment.Instance.GetChunk(topPosition, out mTop);
					if (mTop != null)
						mTop.mBottom = this;
				}
				return mTop;
			}
			set => mTop = value;
		}


		public VoxelChunk Bottom
		{
			get
			{
				if (mBottom == null)
				{
					Vector3 bottomPosition = Position;
					bottomPosition.Y -= TerrainEnvironment.CHUNK_SIZE;
					TerrainEnvironment.Instance.GetChunk(bottomPosition, out mBottom, false);
					if (mBottom != null)
						mBottom.mTop = this;
				}
				return mBottom;
			}
			set => mBottom = value;
		}


		public VoxelChunk Left
		{
			get
			{
				if (mLeft == null)
				{
					Vector3 leftPosition = Position;
					leftPosition.X -= TerrainEnvironment.CHUNK_SIZE;
					TerrainEnvironment.Instance.GetChunk(leftPosition, out mLeft, false);
					if (mLeft != null)
						mLeft.mRight = this;
				}
				return mLeft;
			}
			set => mLeft = value;
		}


		public VoxelChunk Right
		{
			get
			{
				if (mRight == null)
				{
					Vector3 rightPosition = Position;
					rightPosition.X += TerrainEnvironment.CHUNK_SIZE;
					TerrainEnvironment.Instance.GetChunk(rightPosition, out mRight, false);
					if (mRight != null)
						mRight.mLeft = this;
				}
				return mRight;
			}
			set => mRight = value;
		}


		public VoxelChunk Forward
		{
			get
			{
				if (mForward == null)
				{
					Vector3 forwardPosition = Position;
					forwardPosition.Z += TerrainEnvironment.CHUNK_SIZE;
					TerrainEnvironment.Instance.GetChunk(forwardPosition, out mForward, false);
					if (mForward != null)
						mForward.mBack = this;
				}
				return mForward;
			}
			set => mForward = value;
		}


		public VoxelChunk Back
		{
			get
			{
				if (mBack == null)
				{
					Vector3 backPosition = Position;
					backPosition.Z -= TerrainEnvironment.CHUNK_SIZE;
					TerrainEnvironment.Instance.GetChunk(backPosition, out mBack, false);
					if (mBack != null)
						mBack.mForward = this;
				}
				return mBack;
			}
			set => mBack = value;
		}


	}
}
