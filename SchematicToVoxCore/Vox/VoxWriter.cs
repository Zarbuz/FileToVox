using FileToVox.Schematics;
using FileToVox.Schematics.Tools;
using FileToVox.Utils;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileToVox.Vox
{
	public class VoxWriter : VoxParser
    {
        private int _width;
        private int _length;
        private int _height;
        private int _countSize;
        private int _countRegionNonEmpty;
        private int _totalBlockCount;

        private int _countBlocks;
        private int _childrenChunkSize;
        private Schematic _schematic;
        private Rotation _rotation = Rotation._PZ_PX_P;
        private List<BlockGlobal> _firstBlockInEachRegion;
        private List<Color> _usedColors;
        private uint[,,] _blocks;

        public bool WriteModel(string absolutePath, Schematic schematic)
        {
            _width = _length = _height = _countSize = _totalBlockCount = _countRegionNonEmpty = 0;
            _schematic = schematic;
            _blocks = _schematic.Blocks.To3DArray(schematic);
            using (var writer = new BinaryWriter(File.Open(absolutePath, FileMode.Create)))
            {
                writer.Write(Encoding.UTF8.GetBytes(HEADER));
                writer.Write(VERSION);
                writer.Write(Encoding.UTF8.GetBytes(MAIN));
                writer.Write(0); //MAIN CHUNK has a size of 0
                writer.Write(CountChildrenSize());
                WriteChunks(writer);
            }
            return true;
        }

        /// <summary>
        /// Count the total bytes of all children chunks
        /// </summary>
        /// <returns></returns>
        private int CountChildrenSize()
        {
            _width = (int)Math.Ceiling(((decimal)_schematic.Width / 126)) + 1;
            _length = (int)Math.Ceiling(((decimal)_schematic.Length / 126)) + 1;
            _height = (int)Math.Ceiling(((decimal)_schematic.Heigth / 126)) + 1;

            _countSize = _width * _length * _height;
            _firstBlockInEachRegion = GetFirstBlockForEachRegion();
			_countRegionNonEmpty = _firstBlockInEachRegion.Count;
            _totalBlockCount = _schematic.Blocks.Count;

            Console.WriteLine("[INFO] Total blocks: " + _totalBlockCount);

            int chunkSize = 24 * _countRegionNonEmpty; //24 = 12 bytes for header and 12 bytes of content
            int chunkXYZI = (16 * _countRegionNonEmpty) + _totalBlockCount * 4; //16 = 12 bytes for header and 4 for the voxel count + (number of voxels) * 4
            int chunknTRNMain = 40; //40 = 
            int chunknGRP = 24 + _countRegionNonEmpty * 4;
            int chunknTRN = 60 * _countRegionNonEmpty;
            int chunknSHP = 32 * _countRegionNonEmpty;
            int chunkRGBA = 1024 + 12;

            for (int i = 0; i < _countRegionNonEmpty; i++)
            {
                string pos = GetWorldPosString(i);
                chunknTRN += Encoding.UTF8.GetByteCount(pos);
                chunknTRN += Encoding.UTF8.GetByteCount(Convert.ToString((byte)_rotation));
            }
            _childrenChunkSize = chunkSize; //SIZE CHUNK
            _childrenChunkSize += chunkXYZI; //XYZI CHUNK
            _childrenChunkSize += chunknTRNMain; //First nTRN CHUNK (constant)
            _childrenChunkSize += chunknGRP; //nGRP CHUNK
            _childrenChunkSize += chunknTRN; //nTRN CHUNK
            _childrenChunkSize += chunknSHP;
            _childrenChunkSize += chunkRGBA;

            return _childrenChunkSize;
        }

        /// <summary>
        /// Get all blocks in the specified coordinates
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        private HashSet<Block> GetBlocksInRegion(Vector3 min, Vector3 max)
        {
            ConcurrentBag<Block> concurrent = new ConcurrentBag<Block>();

            Parallel.ForEach(_schematic.Blocks, block =>
            {
                if (block.X >= min.X && block.Y >= min.Y && block.Z >= min.Z && block.X < max.X && block.Y < max.Y && block.Z < max.Z)
                {
                    concurrent.Add(block);
                }
            });

            return concurrent.ToHashSet();
        }

        private bool HasBlockInRegion(Vector3 min, Vector3 max)
        {
	        for (int y = (int) min.Y; y < max.Y; y++)
	        {
		        for (int z = (int) min.Z; z < max.Z; z++)
		        {
			        for (int x = (int) min.X; x < max.X; x++)
			        {
				        if (y < _schematic.Heigth && x < _schematic.Width && z < _schematic.Length && _blocks[x, y, z] != 0)
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
			for (int i = 0; i < _countSize; i++)
            {
                int x = i % _width;
                int y = (i / _width) % _height;
                int z = i / (_width * _height);
                if (HasBlockInRegion(new Vector3(x * 126, y * 126, z * 126), new Vector3(x * 126 + 126, y * 126 + 126, z * 126 + 126)))
                {
	                list.Add(new BlockGlobal(x * 126, y * 126, z * 126));
                }
			}

			return list;
        }

        /// <summary>
        /// Convert the coordinates of the first block in each region into string
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private string GetWorldPosString(int index)
        {
            int worldPosX = _firstBlockInEachRegion[index].X - (_length / 2) * 126;
            int worldPosZ = _firstBlockInEachRegion[index].Z - (_width / 2) * 126;
            int worldPosY = _firstBlockInEachRegion[index].Y + 126;

            string pos = worldPosZ + " " + worldPosX + " " + worldPosY;
            return pos;
        }

        /// <summary>
        /// Main loop for write all chunks
        /// </summary>
        /// <param name="writer"></param>
        private void WriteChunks(BinaryWriter writer)
        {
            WritePaletteChunk(writer);
            using (var progressbar = new ProgressBar())
            {
                Console.WriteLine("[LOG] Started to write chunks ...");
                for (int i = 0; i < _countRegionNonEmpty; i++)
                {
                    WriteSizeChunk(writer);
                    WriteXyziChunk(writer, i);
                    float progress = ((float)i / _countSize);
                    progressbar.Report(progress);
                }
                Console.WriteLine("[LOG] Done.");
            }

			foreach (Block block in _schematic.Blocks)
			{
				Console.WriteLine(block);
			}

			WriteMainTranformNode(writer);
            WriteGroupChunk(writer);
            for (int i = 0; i < _countRegionNonEmpty; i++)
            {
                WriteTransformChunk(writer, i);
                WriteShapeChunk(writer, i);
            }
            Console.WriteLine("[LOG] Check total blocks after conversion: " + _countBlocks);
            if (_totalBlockCount != _countBlocks)
            {
                Console.WriteLine("[ERROR] There is a difference between total blocks before and after conversion.");
            }
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

            writer.Write(126); //Width
            writer.Write(126); //Height
            writer.Write(126); //Depth
        }

        /// <summary>
        /// Write XYZI chunk
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="index"></param>
        private void WriteXyziChunk(BinaryWriter writer, int index)
        {
            writer.Write(Encoding.UTF8.GetBytes(XYZI));
            HashSet<Block> blocks = new HashSet<Block>();

            if (_schematic.Blocks.Count > 0)
            {
                BlockGlobal firstBlock = _firstBlockInEachRegion[index];
                blocks = GetBlocksInRegion(new Vector3(firstBlock.X, firstBlock.Y, firstBlock.Z), new Vector3(firstBlock.X + 126, firstBlock.Y + 126, firstBlock.Z + 126));
            }
            writer.Write((blocks.Count() * 4) + 4); //XYZI chunk size
            writer.Write(0); //Child chunk size (constant)
            writer.Write(blocks.Count()); //Blocks count
            _countBlocks += blocks.Count;

            foreach (Block block in blocks)
            {
                writer.Write((byte)(block.X % 126));
                writer.Write((byte)(block.Y % 126));
                writer.Write((byte)(block.Z % 126));
                int i = _usedColors.IndexOf(block.Color.UIntToColor()) + 1;
                writer.Write((i != 0) ? (byte)i : (byte)1);
                _schematic.Blocks.Remove(block);
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
                            + Encoding.UTF8.GetByteCount(Convert.ToString((byte)_rotation))); //nTRN chunk size
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
            writer.Write(Encoding.UTF8.GetByteCount(Convert.ToString((byte)_rotation)));
            writer.Write(Encoding.UTF8.GetBytes(Convert.ToString((byte)_rotation)));


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
            writer.Write(16 + (4 * (_countRegionNonEmpty - 1))); //nGRP chunk size
            writer.Write(0); //Child nGRP chunk size
            writer.Write(1); //ID of nGRP
            writer.Write(0); //Read DICT size for attributes (none)
            writer.Write(_countRegionNonEmpty);
            for (int i = 0; i < _countRegionNonEmpty; i++)
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
            _usedColors = new List<Color>(256);
            foreach (Block block in _schematic.Blocks)
            {
                Color color = block.Color.UIntToColor();
                if (_usedColors.Count < 256 && !_usedColors.Contains(color))
                {
                    _usedColors.Add(color);
                    writer.Write(color.R);
                    writer.Write(color.G);
                    writer.Write(color.B);
                    writer.Write(color.A);
                }
            }

            for (int i = (256 - _usedColors.Count); i >= 1; i--)
            {
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);
                writer.Write((byte)0);
            }
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
