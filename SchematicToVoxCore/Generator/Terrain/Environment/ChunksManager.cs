using System;
using System.Collections.Generic;
using FileToVox.Generator.Terrain.Entities;
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

	public partial class TerrainEnvironment
	{
		private Dictionary<int, CachedChunk> mCachedChunks;
		private Octree[] mChunkRequests;
		private int mChunkRequestLast;
		private const int CHUNKS_CREATION_BUFFER_SIZE = 15000;


		private Dictionary<Vector3, Octree> mOctreeRoots;
		private int mOctreeSize;

		public void InitChunkManager()
		{
			mCachedChunks = new Dictionary<int, CachedChunk>();
			mChunkRequests = new Octree[CHUNKS_CREATION_BUFFER_SIZE];
			mChunkRequestLast = -1;

			InitOctrees();
			InitHeightMap();
		}

		#region PrivateMethods

		private void InitOctrees()
		{
			mOctreeRoots = new Dictionary<Vector3, Octree>();
			mOctreeSize = 60 * CHUNK_SIZE * 2; //60: max value -> 60 * 16 *2 < 2000
			float l2 = MathF.Log(mOctreeSize, 2);
			mOctreeSize = (int) MathF.Pow(2, MathF.Ceiling(l2));
		}

		#endregion
	}
}
