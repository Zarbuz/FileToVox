using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox
{
    public class VoxParser
    {
        private const string HEADER = "VOX ";
        private const string MAIN = "MAIN";
        private const string SIZE = "SIZE";
        private const string XYZI = "XYZI";
        private const string RGBA = "RGBA";
        private const string MATT = "MATT";
        private const string PACK = "PACK";

        private const string nTRN = "nTRN";
        private const string nGRP = "nGRP";
        private const string nSHP = "nSHP";
        private const string LAYR = "LAYR";
        private const string MATL = "MATL";
        private const string rOBJ = "rOBJ";

        private const int VERSION = 150;
        private int childCount = 0;

        public bool LoadModel(string absolutePath, VoxModel output)
        {
            var name = Path.GetFileNameWithoutExtension(absolutePath);
            Console.WriteLine("load: " + name);
            using (var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(absolutePath))))
            {
                var head = new string(reader.ReadChars(4));
                if (!head.Equals(HEADER))
                {
                    Console.WriteLine("Not a Magicavoxel file! " + output);
                    return false;
                }
                int version = reader.ReadInt32();
                if (version != VERSION)
                {
                    Console.WriteLine("Version number: " + version + " Was designed for version: " + VERSION);
                }
                ResetModel(output);
                childCount = 0;
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                    ReadChunk(reader, output);
            }
            return true;
        }

        private void ReadChunk(BinaryReader reader, VoxModel output)
        {
            var chunkName = new string(reader.ReadChars(4));
            var chunkSize = reader.ReadInt32();
            var childChunkSize = reader.ReadInt32();
            var chunk = reader.ReadBytes(chunkSize);
            var children = reader.ReadBytes(childChunkSize);
            using (var chunkReader = new BinaryReader(new MemoryStream(chunk)))
            {
                switch (chunkName)
                {
                    case MAIN:
                        break;
                    case SIZE:
                        int w = chunkReader.ReadInt32();
                        int h = chunkReader.ReadInt32();
                        int d = chunkReader.ReadInt32();
                        if (childCount >= output.voxelFrames.Count)
                            output.voxelFrames.Add(new VoxelData());
                        output.voxelFrames[childCount].Resize(w, d, h);
                        childCount++;
                        break;
                    case XYZI:
                        var voxelCount = chunkReader.ReadInt32();
                        var frame = output.voxelFrames[childCount - 1];
                        byte x, y, z;
                        for (int i = 0; i < voxelCount; i++)
                        {
                            x = chunkReader.ReadByte();
                            y = chunkReader.ReadByte();
                            z = chunkReader.ReadByte();
                            frame.Set(x, y, z, chunkReader.ReadByte());
                        }
                        break;
                    case RGBA:
                        break;
                    case MATT:
                        break;
                    case PACK:
                        int frameCount = chunkReader.ReadInt32();
                        for (int i = 0; i < frameCount; i++)
                        {
                            output.voxelFrames.Add(new VoxelData());
                        }
                        break;
                    case nTRN:
                        output.transformNodeChunks.Add(ReadTransformNodeChunk(chunkReader));
                        break;
                    case nGRP:
                        output.groupNodeChunks.Add(ReadGroupNodeChunk(chunkReader));
                        break;
                    case nSHP:
                        output.shapeNodeChunks.Add(ReadShapeNodeChunk(chunkReader));
                        break;
                    case LAYR:
                        output.layerChunks.Add(ReadLayerChunk(chunkReader));
                        break;
                    case MATL:
                        output.materialChunks.Add(ReadMaterialChunk(chunkReader));
                        break;
                    case rOBJ:
                        output.rendererSettingChunks.Add(ReaddRObjectChunk(chunkReader));
                        break;
                    default:
                        Console.WriteLine($"Unknown chunk: \"{chunkName}\"");
                        break;
                }
            }

            //read child chunks
            using (var childReader = new BinaryReader(new MemoryStream(children)))
            {
                while (childReader.BaseStream.Position != childReader.BaseStream.Length)
                {
                    ReadChunk(childReader, output);
                }
            }
        }

        private static string ReadSTRING(BinaryReader reader)
        {
            var size = reader.ReadInt32();
            var bytes = reader.ReadBytes(size);
            return System.Text.Encoding.UTF8.GetString(bytes);
        }

        private delegate T ItemReader<T>(BinaryReader reader);

        private static T[] ReadArray<T>(BinaryReader reader, ItemReader<T> itemReader)
        {
            return Enumerable.Range(0, reader.ReadInt32())
                .Select(i => itemReader(reader)).ToArray();
        }

        private static KeyValue[] ReadDICT(BinaryReader reader)
        {
            return Enumerable.Range(0, reader.ReadInt32())
                .Select(i => new KeyValue
                {
                    Key = ReadSTRING(reader),
                    Value = ReadSTRING(reader),
                }).ToArray();
        }

        private RendererSettingChunk ReaddRObjectChunk(BinaryReader chunkReader)
        {
            return new RendererSettingChunk
            {
                attributes = ReadDICT(chunkReader)
            };
        }

        private MaterialChunk ReadMaterialChunk(BinaryReader chunkReader)
        {
            return new MaterialChunk
            {
                id = chunkReader.ReadInt32(),
                properties = ReadDICT(chunkReader)
            };
        }

        private LayerChunk ReadLayerChunk(BinaryReader chunkReader)
        {
            return new LayerChunk
            {
                id = chunkReader.ReadInt32(),
                attributes = ReadDICT(chunkReader),
                unknown = chunkReader.ReadInt32()
            };
        }

        private ShapeNodeChunk ReadShapeNodeChunk(BinaryReader chunkReader)
        {
            return new ShapeNodeChunk
            {
                id = chunkReader.ReadInt32(),
                attributes = ReadDICT(chunkReader),
                models = ReadArray(chunkReader, r => new ShapeModel
                {
                    modelId = r.ReadInt32(),
                    attributes = ReadDICT(r)
                })
            };
        }

        private GroupNodeChunk ReadGroupNodeChunk(BinaryReader chunkReader)
        {
            return new GroupNodeChunk
            {
                id = chunkReader.ReadInt32(),
                attributes = ReadDICT(chunkReader),
                childIds = ReadArray(chunkReader, r => r.ReadInt32())
            };
        }

        private TransformNodeChunk ReadTransformNodeChunk(BinaryReader chunkReader)
        {
            return new TransformNodeChunk()
            {
                id = chunkReader.ReadInt32(),
                attributes = ReadDICT(chunkReader),
                childId = chunkReader.ReadInt32(),
                reservedId = chunkReader.ReadInt32(),
                layerId = chunkReader.ReadInt32(),
                frameAttributes = ReadArray(chunkReader, r => new DICT(ReadDICT(r)))
            };
        }

        private void ResetModel(VoxModel model)
        {
            if (model.voxelFrames != null)
                model.voxelFrames.Clear();
            else
                model.voxelFrames = new List<VoxelData>();
            model.materialChunks.Clear();
            model.transformNodeChunks.Clear();
            model.groupNodeChunks.Clear();
            model.shapeNodeChunks.Clear();
            model.layerChunks.Clear();
            model.rendererSettingChunks.Clear();
        }
    }
}
