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

        private int CountChildrenSize(Schematic schematic)
        {
            _width = (int)Math.Ceiling(((decimal)schematic.Width / 126));
            _length = (int)Math.Ceiling(((decimal)schematic.Length / 126));
            _height = (int)Math.Ceiling(((decimal)schematic.Heigth / 126));

            int countSize = _width * _length * _height;
            int chunkSize = 24 * countSize;
            int chunkXYZI = (16 * countSize) + schematic.Blocks.Count * 4;
            _childrenChunkSize = chunkSize; //SIZE CHUNK
            _childrenChunkSize += chunkXYZI; //XYZI CHUNK
            return _childrenChunkSize;
        }

        private List<Block> GetBlocksInRegion(Vector3 min, Vector3 max, Schematic schematic)
        {
            return schematic.Blocks.Where(t => t.X >= min.x && t.Y >= min.y && t.Z >= min.z
            && t.X < max.x && t.Y < max.y && t.Z < max.z).ToList();
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
                var blocks = GetBlocksInRegion(new Vector3(i * 126, i * 126, i * 126), new Vector3((i * 126) + 126, (i * 126) + 126, (i * 126) + 126), schematic);
                writer.Write((blocks.Count * 4) + 4);
                writer.Write(0); //Child chunk size (constant)

                writer.Write(blocks.Count);

                foreach (Block block in blocks)
                {
                    writer.Write((byte)block.X);
                    writer.Write((byte)block.Y);
                    writer.Write((byte)block.Z);
                    writer.Write((byte)79); //TODO: Apply color of the block
                }
            }

        }
    }
}
