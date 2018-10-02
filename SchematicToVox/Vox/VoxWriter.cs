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

        private int _childrenChunkSize = 0;

        public bool WriteModel(string absolutePath, Schematic schematic)
        {
            _width = _length = _height = 0;
            schematic = PostTreatement(schematic);
            using (var writer = new BinaryWriter(File.Open(absolutePath, FileMode.Create)))
            {
                writer.Write(Encoding.UTF8.GetBytes(HEADER));
                writer.Write(VERSION);
                writer.Write(Encoding.UTF8.GetBytes(MAIN));
                writer.Write(0); //MAIN CHUNK has a size of 0
                writer.Write(CountChildrenSize(schematic));
                WriteChunk(writer, schematic);
            }
            return true;
        }

        private Schematic PostTreatement(Schematic schematic)
        {
            var result = schematic;
            using (var writer = new StreamWriter(File.Open("../../logs/schematic.txt", FileMode.Create)))
            {
                foreach (var block in schematic.Blocks)
                {
                    writer.WriteLine(block.ToString());
                }
            }
            
            return result;
        }

        private int CountChildrenSize(Schematic schematic)
        {
            _width = (int)Math.Ceiling(((decimal)schematic.Width / 126));
            _length = (int)Math.Ceiling(((decimal)schematic.Length / 126));
            _height = (int)Math.Ceiling(((decimal)schematic.Heigth / 126));

            int countSize = _width * _length * _height;
            int chunkSize = 24 * countSize; //24 = 12 bytes for header and 12 bytes of content
            int chunkXYZI = (16 * countSize) + schematic.Blocks.Count * 4; //16 = 12 bytes for header and 4 for the voxel count + (number of voxels) * 4
            _childrenChunkSize = chunkSize; //SIZE CHUNK
            _childrenChunkSize += chunkXYZI; //XYZI CHUNK
            return _childrenChunkSize;
        }

        private List<Block> GetBlocksInRegion(Vector3 min, Vector3 max, Schematic schematic)
        {
            return schematic.Blocks.Where(t => t.X >= min.x && t.Y >= min.y && t.Z >= min.z
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

        private void WriteChunk(BinaryWriter writer, Schematic schematic)
        {
            int countSize = _width * _length * _height;
            for (int i = 0; i < countSize; i++)
            {
                writer.Write(Encoding.UTF8.GetBytes(SIZE));
                writer.Write(12); //Chunk Size (constant)
                writer.Write(0); //Child Chunk Size (constant)

                writer.Write(126); //Width
                writer.Write(126); //Height
                writer.Write(126); //Depth


                writer.Write(Encoding.UTF8.GetBytes(XYZI));
                Block firstBlock = schematic.Blocks[0];
                var blocks = GetBlocksInRegion(new Vector3(firstBlock.X, firstBlock.Y, firstBlock.Z), new Vector3(firstBlock.X + 126, firstBlock.Y + 126, firstBlock.Z + 126), schematic);
                RecenterBlocks(ref blocks, i);
                writer.Write((blocks.Count * 4) + 4);
                writer.Write(0); //Child chunk size (constant)

                writer.Write(blocks.Count);

                foreach (Block block in blocks)
                {
                    writer.Write((byte)block.X);
                    writer.Write((byte)block.Y);
                    writer.Write((byte)block.Z);
                    writer.Write((byte)79); //TODO: Apply color of the block
                    schematic.Blocks.Remove(block);
                }
            }

        }
    }
}
