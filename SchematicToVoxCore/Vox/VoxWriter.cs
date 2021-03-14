using FileToVox.Schematics;
using FileToVox.Schematics.Tools;
using FileToVox.Utils;
using FileToVox.Vox.Chunks;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace FileToVox.Vox
{
	public class VoxWriter : VoxParser
	{
		private int mWidth;
		private int mLength;
		private int mHeight;
		private int mCountSize;
		private int mCountRegionNonEmpty;
		private int mTotalBlockCount;

		private int mCountBlocks;
		private int mChildrenChunkSize;
		private Schematic mSchematic;
		private readonly Rotation mRotation = Rotation._PZ_PX_P;
		private List<BlockGlobal> mFirstBlockInEachRegion;
		private List<Color> mUsedColors;
		private List<Color> mPalette;
		private uint[,,] mBlocks;
		private int mChunkSize;

		public bool WriteModel(int chunkSize, string absolutePath, List<Color> palette, Schematic schematic)
		{
			mChunkSize = chunkSize;
			mWidth = mLength = mHeight = mCountSize = mTotalBlockCount = mCountRegionNonEmpty = 0;
			mSchematic = schematic;
			mPalette = palette;
			mBlocks = mSchematic.Blocks.To3DArray(schematic);
			using (BinaryWriter writer = new BinaryWriter(File.Open(absolutePath, FileMode.Create)))
			{
				writer.Write(Encoding.UTF8.GetBytes(HEADER));
				writer.Write(VERSION);
				writer.Write(Encoding.UTF8.GetBytes(MAIN));
				writer.Write(0); //MAIN CHUNK has a size of 0
				writer.Write(CountChildrenSize());
				return WriteChunks(writer);
			}
		}

		/// <summary>
		/// Count the total bytes of all children chunks
		/// </summary>
		/// <returns></returns>
		private int CountChildrenSize()
		{
			mWidth = (int)Math.Ceiling(((decimal)mSchematic.Width / mChunkSize)) + 1;
			mLength = (int)Math.Ceiling(((decimal)mSchematic.Length / mChunkSize)) + 1;
			mHeight = (int)Math.Ceiling(((decimal)mSchematic.Height / mChunkSize)) + 1;

			mCountSize = mWidth * mLength * mHeight;
			mFirstBlockInEachRegion = GetFirstBlockForEachRegion();
			mCountRegionNonEmpty = mFirstBlockInEachRegion.Count;
			mTotalBlockCount = mSchematic.Blocks.Count;

			Console.WriteLine("[INFO] Total blocks: " + mTotalBlockCount);

			int chunkSize = 24 * mCountRegionNonEmpty; //24 = 12 bytes for header and 12 bytes of content
			int chunkXYZI = (16 * mCountRegionNonEmpty) + mTotalBlockCount * 4; //16 = 12 bytes for header and 4 for the voxel count + (number of voxels) * 4
			int chunknTRNMain = 40; //40 = 
			int chunknGRP = 24 + mCountRegionNonEmpty * 4;
			int chunknTRN = 60 * mCountRegionNonEmpty;
			int chunknSHP = 32 * mCountRegionNonEmpty;
			int chunkRGBA = 1024 + 12;
			int chunkMATL = 256 * 206;

			for (int i = 0; i < mCountRegionNonEmpty; i++)
			{
				string pos = GetWorldPosString(i);
				chunknTRN += Encoding.UTF8.GetByteCount(pos);
				chunknTRN += Encoding.UTF8.GetByteCount(Convert.ToString((byte)mRotation));
			}
			mChildrenChunkSize = chunkSize; //SIZE CHUNK
			mChildrenChunkSize += chunkXYZI; //XYZI CHUNK
			mChildrenChunkSize += chunknTRNMain; //First nTRN CHUNK (constant)
			mChildrenChunkSize += chunknGRP; //nGRP CHUNK
			mChildrenChunkSize += chunknTRN; //nTRN CHUNK
			mChildrenChunkSize += chunknSHP;
			mChildrenChunkSize += chunkRGBA;
			mChildrenChunkSize += chunkMATL;

			return mChildrenChunkSize;
		}

		/// <summary>
		/// Get all blocks in the specified coordinates
		/// </summary>
		/// <param name="min"></param>
		/// <param name="max"></param>
		/// <returns></returns>
		private List<Voxel> GetBlocksInRegion(Vector3 min, Vector3 max)
		{
			List<Voxel> list = new List<Voxel>();

			for (int y = (int)min.Y; y < max.Y; y++)
			{
				for (int z = (int)min.Z; z < max.Z; z++)
				{
					for (int x = (int)min.X; x < max.X; x++)
					{
						if (y < mSchematic.Height && x < mSchematic.Width && z < mSchematic.Length && mBlocks[x, y, z] != 0)
						{
							Voxel voxel = new Voxel((ushort)x, (ushort)y, (ushort)z, mBlocks[x, y, z]);
							list.Add(voxel);
						}
					}
				}
			}

			return list;
		}

		private bool HasBlockInRegion(Vector3 min, Vector3 max)
		{
			for (int y = (int)min.Y; y < max.Y; y++)
			{
				for (int z = (int)min.Z; z < max.Z; z++)
				{
					for (int x = (int)min.X; x < max.X; x++)
					{
						if (y < mSchematic.Height && x < mSchematic.Width && z < mSchematic.Length && (mBlocks[x, y, z] != 0))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Get world coordinates of the first block in each region
		/// </summary>
		private List<BlockGlobal> GetFirstBlockForEachRegion()
		{
			List<BlockGlobal> list = new List<BlockGlobal>();

			//x = Index % XSIZE;
			//y = (Index / XSIZE) % YSIZE;
			//z = Index / (XSIZE * YSIZE);
			Console.WriteLine("[LOG] Started to compute the first block for each region");
			using (ProgressBar progressBar = new ProgressBar())
			{
				for (int i = 0; i < mCountSize; i++)
				{
					int x = i % mWidth;
					int y = (i / mWidth) % mHeight;
					int z = i / (mWidth * mHeight);
					if (HasBlockInRegion(new Vector3(x * mChunkSize, y * mChunkSize, z * mChunkSize), new Vector3(x * mChunkSize + mChunkSize, y * mChunkSize + mChunkSize, z * mChunkSize + mChunkSize)))
					{
						list.Add(new BlockGlobal(x * mChunkSize, y * mChunkSize, z * mChunkSize));
					}

					progressBar.Report(i / (float)mCountSize);
				}
			}

			Console.WriteLine("[LOG] Done.");
			return list;
		}

		/// <summary>
		/// Convert the coordinates of the first block in each region into string
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		private string GetWorldPosString(int index)
		{
			int worldPosX = mFirstBlockInEachRegion[index].X - 938;
			int worldPosZ = mFirstBlockInEachRegion[index].Z - 938;
			int worldPosY = mFirstBlockInEachRegion[index].Y + mChunkSize;

			string pos = worldPosZ + " " + worldPosX + " " + worldPosY;
			return pos;
		}

		/// <summary>
		/// Main loop for write all chunks
		/// </summary>
		/// <param name="writer"></param>
		private bool WriteChunks(BinaryWriter writer)
		{
			WritePaletteChunk(writer);
			for (int i = 0; i < 256; i++)
			{
				WriteMaterialChunk(writer, i + 1);
			}

			using (ProgressBar progressbar = new ProgressBar())
			{
				Console.WriteLine("[LOG] Started to write chunks ...");
				for (int i = 0; i < mCountRegionNonEmpty; i++)
				{
					WriteSizeChunk(writer);
					WriteXyziChunk(writer, i);
					float progress = ((float)i / mCountRegionNonEmpty);
					progressbar.Report(progress);
				}
				Console.WriteLine("[LOG] Done.");
			}

			WriteMainTranformNode(writer);
			WriteGroupChunk(writer);
			for (int i = 0; i < mCountRegionNonEmpty; i++)
			{
				WriteTransformChunk(writer, i);
				WriteShapeChunk(writer, i);
			}
			Console.WriteLine("[LOG] Check total blocks after conversion: " + mCountBlocks);
			if (mTotalBlockCount != mCountBlocks)
			{
				Console.WriteLine("[ERROR] There is a difference between total blocks before and after conversion.");
				if (Program.DEBUG)
				{
					foreach (Voxel voxel in mSchematic.Blocks)
					{
						Console.WriteLine("Missed writing of the voxel: " + voxel);
					}
				}
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write the main trande node chunk
		/// </summary>
		/// <param name="writer"></param>
		private void WriteMainTranformNode(BinaryWriter writer)
		{
			writer.Write(Encoding.UTF8.GetBytes(nTRN));
			writer.Write(28); //Main nTRN has always a 28 bytes size
			writer.Write(0); //Child nTRN chunk size
			writer.Write(0); // ID of nTRN
			writer.Write(0); //ReadDICT size for attributes (none)
			writer.Write(1); //Child ID
			writer.Write(-1); //Reserved ID
			writer.Write(0); //Layer ID
			writer.Write(1); //Read Array Size
			writer.Write(0); //ReadDICT size
		}

		/// <summary>
		/// Write SIZE chunk
		/// </summary>
		/// <param name="writer"></param>
		private void WriteSizeChunk(BinaryWriter writer)
		{
			writer.Write(Encoding.UTF8.GetBytes(SIZE));
			writer.Write(12); //Chunk Size (constant)
			writer.Write(0); //Child Chunk Size (constant)

			writer.Write(mChunkSize); //Width
			writer.Write(mChunkSize); //Height
			writer.Write(mChunkSize); //Depth
		}

		/// <summary>
		/// Write XYZI chunk
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="index"></param>
		private void WriteXyziChunk(BinaryWriter writer, int index)
		{
			writer.Write(Encoding.UTF8.GetBytes(XYZI));
			IEnumerable<Voxel> blocks = null;

			if (mSchematic.Blocks.Count > 0)
			{
				BlockGlobal firstBlock = mFirstBlockInEachRegion[index];

				//blocks = _schematic.Blocks.Where(block => block.X >= firstBlock.X && block.Y >= firstBlock.Y && block.Z >= firstBlock.Z && block.X < firstBlock.X + CHUNK_SIZE && block.Y < firstBlock.Y + CHUNK_SIZE && block.Z < firstBlock.Z + CHUNK_SIZE);
				blocks = GetBlocksInRegion(new Vector3(firstBlock.X, firstBlock.Y, firstBlock.Z), new Vector3(firstBlock.X + mChunkSize, firstBlock.Y + mChunkSize, firstBlock.Z + mChunkSize));
			}
			writer.Write((blocks.Count() * 4) + 4); //XYZI chunk size
			writer.Write(0); //Child chunk size (constant)
			writer.Write(blocks.Count()); //Blocks count
			mCountBlocks += blocks.Count();

			foreach (Voxel block in blocks)
			{
				writer.Write((byte)(block.X % mChunkSize));
				writer.Write((byte)(block.Y % mChunkSize));
				writer.Write((byte)(block.Z % mChunkSize));
				if (block.PalettePosition != -1)
				{
					writer.Write((byte)(block.PalettePosition + 1));
				}
				else
				{
					int i = mUsedColors.IndexOf(block.Color.UIntToColor()) + 1;
					writer.Write((i != 0) ? (byte)i : (byte)1);
				}

				if (Program.DEBUG)
				{
					mSchematic.Blocks.Remove(block);
				}

			}
		}

		/// <summary>
		/// Write nTRN chunk
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="index"></param>
		private void WriteTransformChunk(BinaryWriter writer, int index)
		{
			writer.Write(Encoding.UTF8.GetBytes(nTRN));
			string pos = GetWorldPosString(index);
			writer.Write(48 + Encoding.UTF8.GetByteCount(pos)
							+ Encoding.UTF8.GetByteCount(Convert.ToString((byte)mRotation))); //nTRN chunk size
			writer.Write(0); //nTRN child chunk size
			writer.Write(2 * index + 2); //ID
			writer.Write(0); //ReadDICT size for attributes (none)
			writer.Write(2 * index + 3);//Child ID
			writer.Write(-1); //Reserved ID
			writer.Write(-1); //Layer ID
			writer.Write(1); //Read Array Size
			writer.Write(2); //Read DICT Size (previously 1)

			writer.Write(2); //Read STRING size
			writer.Write(Encoding.UTF8.GetBytes("_r"));
			writer.Write(Encoding.UTF8.GetByteCount(Convert.ToString((byte)mRotation)));
			writer.Write(Encoding.UTF8.GetBytes(Convert.ToString((byte)mRotation)));


			writer.Write(2); //Read STRING Size
			writer.Write(Encoding.UTF8.GetBytes("_t"));
			writer.Write(Encoding.UTF8.GetByteCount(pos));
			writer.Write(Encoding.UTF8.GetBytes(pos));
		}

		/// <summary>
		/// Write nSHP chunk
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="index"></param>
		private void WriteShapeChunk(BinaryWriter writer, int index)
		{
			writer.Write(Encoding.UTF8.GetBytes(nSHP));
			writer.Write(20); //nSHP chunk size
			writer.Write(0); //nSHP child chunk size
			writer.Write(2 * index + 3); //ID
			writer.Write(0);
			writer.Write(1);
			writer.Write(index);
			writer.Write(0);
		}

		/// <summary>
		/// Write nGRP chunk
		/// </summary>
		/// <param name="writer"></param>
		private void WriteGroupChunk(BinaryWriter writer)
		{
			writer.Write(Encoding.UTF8.GetBytes(nGRP));
			writer.Write(16 + (4 * (mCountRegionNonEmpty - 1))); //nGRP chunk size
			writer.Write(0); //Child nGRP chunk size
			writer.Write(1); //ID of nGRP
			writer.Write(0); //Read DICT size for attributes (none)
			writer.Write(mCountRegionNonEmpty);
			for (int i = 0; i < mCountRegionNonEmpty; i++)
			{
				writer.Write((2 * i) + 2); //id for childrens (start at 2, increment by 2)
			}
		}

		/// <summary>
		/// Write RGBA chunk
		/// </summary>
		/// <param name="writer"></param>
		private void WritePaletteChunk(BinaryWriter writer)
		{
			writer.Write(Encoding.UTF8.GetBytes(RGBA));
			writer.Write(1024);
			writer.Write(0);
			mUsedColors = new List<Color>(256);
			if (mPalette != null)
			{
				mUsedColors = mPalette;
				foreach (Color color in mUsedColors)
				{
					writer.Write(color.R);
					writer.Write(color.G);
					writer.Write(color.B);
					writer.Write(color.A);
				}
			}
			else
			{
				foreach (Voxel block in mSchematic.Blocks)
				{
					Color color = block.Color.UIntToColor();
					if (mUsedColors.Count < 256 && !mUsedColors.Contains(color))
					{
						mUsedColors.Add(color);
						writer.Write(color.R);
						writer.Write(color.G);
						writer.Write(color.B);
						writer.Write(color.A);
					}
				}
			}

			for (int i = (256 - mUsedColors.Count); i >= 1; i--)
			{
				writer.Write((byte)0);
				writer.Write((byte)0);
				writer.Write((byte)0);
				writer.Write((byte)0);
			}
		}

		/// <summary>
		/// Write the MATL chunk
		/// </summary>
		/// <param name="writer"></param>
		private int WriteMaterialChunk(BinaryWriter writer, int index)
		{
			int byteWritten = 0;
			writer.Write(Encoding.UTF8.GetBytes(MATL));
			KeyValue[] materialProperties = new KeyValue[12];
			materialProperties[0].Key = "_type";
			materialProperties[0].Value = "_diffuse";

			materialProperties[1].Key = "_weight";
			materialProperties[1].Value = "1";

			materialProperties[2].Key = "_rough";
			materialProperties[2].Value = "0.1";

			materialProperties[3].Key = "_spec";
			materialProperties[3].Value = "0.5";

			materialProperties[4].Key = "_spec_p";
			materialProperties[4].Value = "0.5";

			materialProperties[5].Key = "_ior";
			materialProperties[5].Value = "0.3";

			materialProperties[6].Key = "_att";
			materialProperties[6].Value = "0";

			materialProperties[7].Key = "_g0";
			materialProperties[7].Value = "-0.5";

			materialProperties[8].Key = "_g1";
			materialProperties[8].Value = "0.8";

			materialProperties[9].Key = "_gw";
			materialProperties[9].Value = "0.7";

			materialProperties[10].Key = "_flux";
			materialProperties[10].Value = "0";

			materialProperties[11].Key = "_ldr";
			materialProperties[11].Value = "0";

			writer.Write(GetMaterialPropertiesSize(materialProperties) + 8);
			writer.Write(0); //Child Chunk Size (constant)

			writer.Write(index); //Id
			writer.Write(materialProperties.Length); //ReadDICT size

			byteWritten += Encoding.UTF8.GetByteCount(MATL) + 16;

			foreach (KeyValue keyValue in materialProperties)
			{
				writer.Write(Encoding.UTF8.GetByteCount(keyValue.Key));
				writer.Write(Encoding.UTF8.GetBytes(keyValue.Key));
				writer.Write(Encoding.UTF8.GetByteCount(keyValue.Value));
				writer.Write(Encoding.UTF8.GetBytes(keyValue.Value));

				byteWritten += 8 + Encoding.UTF8.GetByteCount(keyValue.Key) + Encoding.UTF8.GetByteCount(keyValue.Value);
			}

			return byteWritten;
		}

		private int GetMaterialPropertiesSize(KeyValue[] properties)
		{
			return properties.Sum(keyValue => 8 + Encoding.UTF8.GetByteCount(keyValue.Key) + Encoding.UTF8.GetByteCount(keyValue.Value));
		}
	}

	struct BlockGlobal
	{
		public readonly int X;
		public readonly int Y;
		public readonly int Z;

		public BlockGlobal(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public override string ToString()
		{
			return $"{X} {Y} {Z}";
		}
	}
}
