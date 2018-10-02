using SchematicReader;
using SchematicToVox.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Vox
{
    public class VoxWriter : VoxParser
    {
        private int _width = 0;
        private int _length = 0;
        private int _height = 0;
        private int _countSize = 0;

        private int _childrenChunkSize = 0;
        private Schematic _schematic;

        public bool WriteModel(string absolutePath, Schematic schematic)
        {
            _width = _length = _height = _countSize = 0;
            _schematic = schematic;
            using (var writer = new BinaryWriter(File.Open(absolutePath, FileMode.Create)))
            {
                writer.Write(Encoding.UTF8.GetBytes(HEADER));
                writer.Write(VERSION);
                writer.Write(Encoding.UTF8.GetBytes(MAIN));
                writer.Write(0); //MAIN CHUNK has a size of 0
                writer.Write(CountChildrenSize());
                WriteChunk(writer);
            }
            return true;
        }

        private int CountChildrenSize()
        {
            _width = (int)Math.Ceiling(((decimal)_schematic.Width / 126));
            _length = (int)Math.Ceiling(((decimal)_schematic.Length / 126));
            _height = (int)Math.Ceiling(((decimal)_schematic.Heigth / 126));
            _countSize = _width * _length * _height;

            int chunkSize = 24 * _countSize; //24 = 12 bytes for header and 12 bytes of content
            int chunkXYZI = (16 * _countSize) + _schematic.Blocks.Count * 4; //16 = 12 bytes for header and 4 for the voxel count + (number of voxels) * 4
            int chunknTRNMain = 40; //40 = 
            int chunknGRP = 24 + _countSize * 4;
            int chunknTRN = 50 * _countSize;

            for (int i = 0; i < _countSize; i++)
            {
                int worldPosX = i / _width;
                int worldPosY = (i / _width) % _height;
                int worldPosZ = i / (_width * _height);
                string pos = worldPosX + " " + worldPosY + " " + worldPosZ;
                chunknTRN += Encoding.UTF8.GetByteCount(pos);
            }
            _childrenChunkSize = chunkSize; //SIZE CHUNK
            _childrenChunkSize += chunkXYZI; //XYZI CHUNK
            _childrenChunkSize += chunknTRNMain; //First nTRN CHUNK (constant)
            _childrenChunkSize += chunknGRP; //nGRP CHUNK
            _childrenChunkSize += chunknTRN; //nTRN CHUNK
            return _childrenChunkSize;
        }

        private List<Block> GetBlocksInRegion(Vector3 min, Vector3 max)
        {
            return _schematic.Blocks.Where(t => t.X >= min.x && t.Y >= min.y && t.Z >= min.z
            && t.X < max.x && t.Y < max.y && t.Z < max.z).ToList();
        }

        private void RecenterBlocks(ref List<Block> blocks, int multiplier)
        {
            if (blocks[0].Z >= 126)
            {
                blocks.ForEach(t => t.Z -= (126 * multiplier));
            }
            if (blocks[0].X >= 126)
            {
                blocks.ForEach(t => t.X -= (126 * multiplier));
            }
            if (blocks[0].Y >= 126)
            {
                blocks.ForEach(t => t.Y -= (126 * multiplier));
            }
        }

        private void WriteChunk(BinaryWriter writer)
        {
            for (int i = 0; i < _countSize; i++)
            {
                WriteSizeChunk(writer);
                WriteXyziChunk(writer, i);
            }

            writer.Write(Encoding.UTF8.GetBytes(nTRN));
            writer.Write(28); //Main nTRN has always a 28 bytes size
            writer.Write(0); //Child nTRN chunk size
            writer.Write(0); // ID of nTRN
            writer.Write(0); //ReadDICT size for attributes (none)
            writer.Write(1); //Child ID
            writer.Write(-1); //Reserved ID
            writer.Write(-1); //Layer ID
            writer.Write(1); //Read Array Size
            writer.Write(0); //ReadDICT size


            WriteGroupChunk(writer);
            for (int i = 0; i < _countSize; i++)
            {
                WriteTransformChunk(writer, i);
            }
        }

        private void WriteSizeChunk(BinaryWriter writer)
        {
            writer.Write(Encoding.UTF8.GetBytes(SIZE));
            writer.Write(12); //Chunk Size (constant)
            writer.Write(0); //Child Chunk Size (constant)

            writer.Write(126); //Width
            writer.Write(126); //Height
            writer.Write(126); //Depth
        }

        private void WriteXyziChunk(BinaryWriter writer, int index)
        {
            writer.Write(Encoding.UTF8.GetBytes(XYZI));
            Block firstBlock = _schematic.Blocks[0];
            var blocks = GetBlocksInRegion(new Vector3(firstBlock.X, firstBlock.Y, firstBlock.Z), new Vector3(firstBlock.X + 126, firstBlock.Y + 126, firstBlock.Z + 126));
            RecenterBlocks(ref blocks, index);
            writer.Write((blocks.Count * 4) + 4); //XYZI chunk size
            writer.Write(0); //Child chunk size (constant)
            writer.Write(blocks.Count); //Blocks count

            foreach (Block block in blocks)
            {
                writer.Write((byte)block.X);
                writer.Write((byte)block.Y);
                writer.Write((byte)block.Z);
                writer.Write((byte)79); //TODO: Apply color of the block
                _schematic.Blocks.Remove(block);
            }
        }

        private void WriteTransformChunk(BinaryWriter writer, int index)
        {
            writer.Write(Encoding.UTF8.GetBytes(nTRN));
            int worldPosX = (index / _width) * 126;
            int worldPosY = ((index / _width) % _height) * 126;
            int worldPosZ = (index / (_width * _height)) *126;

            string pos = worldPosX + " " + worldPosY + " " + worldPosZ;
            writer.Write(38 + Encoding.UTF8.GetByteCount(pos)); //nTRN chunk size
            writer.Write(0); //nTRN child chunk size
            writer.Write(2 * index + 2); //ID
            writer.Write(0); //ReadDICT size for attributes (none)
            writer.Write(2 * index + 3);//Child ID
            writer.Write(-1); //Reserved ID
            writer.Write(-1); //Layer ID
            writer.Write(1); //Read Array Size
            writer.Write(1); //Read DICT Size
            writer.Write(2); //Read STRING Size
            writer.Write(Encoding.UTF8.GetBytes("_t"));
            writer.Write(Encoding.UTF8.GetByteCount(pos));
            writer.Write(Encoding.UTF8.GetBytes(pos));
        }

        private void WriteShapeChunk(BinaryWriter writer, int index)
        {

        }

        private void WriteGroupChunk(BinaryWriter writer)
        {
            writer.Write(Encoding.UTF8.GetBytes(nGRP));
            writer.Write(16 + (4 * (_countSize - 1))); //nGRP chunk size
            writer.Write(0); //Child nGRP chunk size
            writer.Write(1); //ID of nGRP
            writer.Write(0); //Read DICT size for attributes (none)
            writer.Write(_countSize);
            for (int i = 0; i < _countSize; i++)
            {
                writer.Write((2 * i) + 2); //id for childrens (start at 2, increment by 2)
            }
        }
    }
}
