using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using FileToVoxCore.Extensions;
using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;
using FileToVoxCore.Vox.Chunks;
using Region = FileToVoxCore.Schematics.Region;

namespace FileToVoxCore.Vox
{
	public class VoxWriter : VoxParser
	{
		private int mCountRegionNonEmpty;
		private int mTotalBlockCount;

		private int mCountBlocks;
		private int mChildrenChunkSize;
		private Schematic mSchematic;
		private readonly Rotation mRotation = Rotation._PZ_PX_P;
		private List<Region> mFirstBlockInEachRegion;
		private List<Color> mPalette;
		private int mChunkSize;

		public bool WriteModel(int chunkSize, string absolutePath, List<Color> palette, Schematic schematic)
		{
			mChunkSize = chunkSize;
			mTotalBlockCount = mCountRegionNonEmpty = mCountBlocks = 0;
			mSchematic = schematic;
			mPalette = palette;
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
			mFirstBlockInEachRegion = mSchematic.GetAllRegions();
			mCountRegionNonEmpty = mFirstBlockInEachRegion.Count;
			mTotalBlockCount = mSchematic.GetAllVoxels().Count;

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
				Console.WriteLine("[INFO] Started to write chunks ...");
				for (int i = 0; i < mCountRegionNonEmpty; i++)
				{
					WriteSizeChunk(writer);
					WriteXyziChunk(writer, i);
					float progress = ((float)i / mCountRegionNonEmpty);
					progressbar.Report(progress);
				}
				Console.WriteLine("[INFO] Done.");
			}

			WriteMainTranformNode(writer);
			WriteGroupChunk(writer);
			for (int i = 0; i < mCountRegionNonEmpty; i++)
			{
				WriteTransformChunk(writer, i);
				WriteShapeChunk(writer, i);
			}
			Console.WriteLine("[INFO] Check total blocks after conversion: " + mCountBlocks);
			if (mTotalBlockCount != mCountBlocks)
			{
				Console.WriteLine("[ERROR] There is a difference between total blocks before and after conversion: " + mTotalBlockCount);
				if (Schematic.DEBUG)
				{
					foreach (Voxel voxel in mSchematic.GetAllVoxels())
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

			Region firstBlock = mFirstBlockInEachRegion[index];
			IEnumerable<Voxel> blocks = firstBlock.BlockDict.Values;
			//blocks = _schematic.Blocks.Where(block => block.X >= firstBlock.X && block.Y >= firstBlock.Y && block.Z >= firstBlock.Z && block.X < firstBlock.X + CHUNK_SIZE && block.Y < firstBlock.Y + CHUNK_SIZE && block.Z < firstBlock.Z + CHUNK_SIZE);
			//blocks = GetBlocksInRegion(new Vector3(firstBlock.X, firstBlock.Y, firstBlock.Z), new Vector3(firstBlock.X + mChunkSize, firstBlock.Y + mChunkSize, firstBlock.Z + mChunkSize));
			writer.Write((blocks.Count() * 4) + 4); //XYZI chunk size
			writer.Write(0); //Child chunk size (constant)
			writer.Write(blocks.Count()); //Blocks count
			mCountBlocks += blocks.Count();

			foreach (Voxel block in blocks)
			{
				writer.Write((byte)(block.X % mChunkSize));
				writer.Write((byte)(block.Y % mChunkSize));
				writer.Write((byte)(block.Z % mChunkSize));
				int paletteIndex = mPalette?.IndexOf(block.Color.UIntToColor()) - 1 ?? mSchematic.GetPaletteIndex(block.Color);
				if (paletteIndex != -1)
				{
					writer.Write((byte)(paletteIndex + 1));
				}
				else
				{
					writer.Write((byte)1);
				}

				//mSchematic.RemoveVoxel(block.X, block.Y, block.Z);
				//firstBlock.BlockDict.Remove(block.GetIndex());
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
			List<Color> usedColors = new List<Color>(256);
			if (mPalette != null)
			{
				usedColors = mPalette;
				foreach (Color color in usedColors)
				{
					writer.Write(color.R);
					writer.Write(color.G);
					writer.Write(color.B);
					writer.Write(color.A);
				}
			}
			else
			{
				foreach (uint voxelColor in mSchematic.UsedColors)
				{
					Color color = voxelColor.UIntToColor();
					usedColors.Add(color);
					writer.Write(color.R);
					writer.Write(color.G);
					writer.Write(color.B);
					writer.Write(color.A);
				}
			}

			for (int i = (256 - usedColors.Count); i >= 1; i--)
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

	
}
