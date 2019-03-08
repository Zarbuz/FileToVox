using System;
using System.IO;
using System.Linq;
using System.Text;
using SchematicToVoxCore.Schematics.Tools;
using SchematicToVoxCore.Vox.Chunks;

namespace SchematicToVoxCore.Vox
{
    public class VoxReader : VoxParser
    {
        private int _voxelCountLastXYZIChunk = 0;
        protected string _logOutputFile;

        public VoxModel LoadModel(string absolutePath)
        {
            VoxModel output = new VoxModel();
            var name = Path.GetFileNameWithoutExtension(absolutePath);
            _logOutputFile = name + "-" + DateTime.Now.ToString("y-MM-d_HH.m.s") + ".txt";
            using (var reader = new BinaryReader(new MemoryStream(File.ReadAllBytes(absolutePath))))
            {
                var head = new string(reader.ReadChars(4));
                if (!head.Equals(HEADER))
                {
                    Console.WriteLine("Not a Magicavoxel file! " + output);
                    return null;
                }
                int version = reader.ReadInt32();
                if (version != VERSION)
                {
                    Console.WriteLine("Version number: " + version + " Was designed for version: " + VERSION);
                }
                ResetModel(output);
                _childCount = 0;
                _chunkCount = 0;
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                    ReadChunk(reader, output);
            }
            if (output.palette == null)
                output.palette = LoadDefaultPalette();
            return output;
        }

        private Color[] LoadDefaultPalette()
        {
            var colorCount = default_palette.Length;
            var result = new Color[256];
            byte r, g, b, a;
            for (int i = 0; i < colorCount; i++)
            {
                var source = default_palette[i];
                r = (byte)(source & 0xff);
                g = (byte)((source >> 8) & 0xff);
                b = (byte)((source >> 16) & 0xff);
                a = (byte)((source >> 26) & 0xff);
                result[i] = new Color32(r, g, b, a);
            }
            return result;
        }

        /// <summary>
        /// Load the palette color. Plattes are offset by 1
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Color[] LoadPalette(BinaryReader reader)
        {
            var result = new Color[256];
            for (int i = 1; i < 256; i++)
            {
                result[i] = new Color32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
            }
            return result;
        }

        private void ReadChunk(BinaryReader reader, VoxModel output)
        {
            var chunkName = new string(reader.ReadChars(4));
            var chunkSize = reader.ReadInt32();
            var childChunkSize = reader.ReadInt32();
            var chunk = reader.ReadBytes(chunkSize);
            var children = reader.ReadBytes(childChunkSize);
            _chunkCount++;

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
                        if (_childCount >= output.voxelFrames.Count)
                            output.voxelFrames.Add(new VoxelData());
                        output.voxelFrames[_childCount].Resize(w, d, h);
                        _childCount++;
                        break;
                    case XYZI:
                        _voxelCountLastXYZIChunk = chunkReader.ReadInt32();
                        var frame = output.voxelFrames[_childCount - 1];
                        byte x, y, z, color;
                        for (int i = 0; i < _voxelCountLastXYZIChunk; i++)
                        {
                            x = chunkReader.ReadByte();
                            y = chunkReader.ReadByte();
                            z = chunkReader.ReadByte();
                            color = chunkReader.ReadByte();
                            frame.Set(x, y, z, color);
                        }
                        break;
                    case RGBA:
                        output.palette = LoadPalette(chunkReader);
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
            WriteLogs(chunkName, chunkSize, childChunkSize, output);

            //read child chunks
            using (var childReader = new BinaryReader(new MemoryStream(children)))
            {
                while (childReader.BaseStream.Position != childReader.BaseStream.Length)
                {
                    ReadChunk(childReader, output);
                }
            }

        }

        private void WriteLogs(string chunkName, int chunkSize, int childChunkSize, VoxModel output)
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");

            string path = "logs/" + _logOutputFile;
            using (var writer = new StreamWriter(path, true))
            {
                writer.WriteLine("CHUNK NAME: " + chunkName + " (" + _chunkCount + ")");
                writer.WriteLine("CHUNK SIZE: " + chunkSize + " BYTES");
                writer.WriteLine("CHILD CHUNK SIZE: " + childChunkSize);
                switch (chunkName)
                {
                    case SIZE:
                        var frame = output.voxelFrames[_childCount - 1];
                        writer.WriteLine("-> SIZE: " + frame.VoxelsWide + " " + frame.VoxelsTall + " " + frame.VoxelsDeep);
                        break;
                    case XYZI:
                        writer.WriteLine("-> XYZI: " + _voxelCountLastXYZIChunk);
                        break;
                    case nTRN:
                        var transform = output.transformNodeChunks.Last();
                        writer.WriteLine("-> TRANSFORM NODE: " + transform.id);
                        writer.WriteLine("--> CHILD ID: " + transform.childId);
                        writer.WriteLine("--> RESERVED ID: " + transform.reservedId);
                        writer.WriteLine("--> LAYER ID: " + transform.layerId);
                        DisplayAttributes(transform.attributes, writer);
                        DisplayFrameAttributes(transform.frameAttributes, writer);
                        break;
                    case nGRP:
                        var group = output.groupNodeChunks.Last();
                        writer.WriteLine("-> GROUP NODE: " + group.id);
                        group.childIds.ToList().ForEach(t => writer.WriteLine("--> CHILD ID: " + t));
                        DisplayAttributes(group.attributes, writer);
                        break;
                    case nSHP:
                        var shape = output.shapeNodeChunks.Last();
                        writer.WriteLine("-> SHAPE NODE: " + shape.id);
                        DisplayAttributes(shape.attributes, writer);
                        DisplayModelAttributes(shape.models, writer);
                        break;
                    case LAYR:
                        var layer = output.layerChunks.Last();
                        writer.WriteLine("-> LAYER NODE: " + layer.id + " " +
                            layer.Name + " " +
                            layer.Hidden + " " +
                            layer.unknown);
                        DisplayAttributes(layer.attributes, writer);
                        break;
                    case MATL:
                        var material = output.materialChunks.Last();
                        writer.WriteLine("-> MATERIAL NODE: " + material.id.ToString("F1"));
                        writer.WriteLine("--> ALPHA: " + material.Alpha.ToString("F1"));
                        writer.WriteLine("--> EMISSION: " + material.Emission.ToString("F1"));
                        writer.WriteLine("--> FLUX: " + material.Flux.ToString("F1"));
                        writer.WriteLine("--> METALLIC: " + material.Metallic.ToString("F1"));
                        writer.WriteLine("--> ROUGH: " + material.Rough.ToString("F1"));
                        writer.WriteLine("--> SMOOTHNESS: " + material.Smoothness.ToString("F1"));
                        writer.WriteLine("--> SPEC: " + material.Spec.ToString("F1"));
                        writer.WriteLine("--> WEIGHT: " + material.Weight.ToString("F1"));
                        DisplayAttributes(material.properties, writer);
                        break;
                }
                writer.WriteLine("");
                writer.Close();
            }
        }

        private void DisplayAttributes(KeyValue[] attributes, StreamWriter writer)
        {
            attributes.ToList().ForEach(t => writer.WriteLine("--> ATTRIBUTE: Key=" + t.Key + " Value=" + t.Value));
        }

        private void DisplayFrameAttributes(DICT[] frameAttributes, StreamWriter writer)
        {
            var list = frameAttributes.ToList();
            foreach (var item in list )
            {
                writer.WriteLine("--> FRAME ATTRIBUTE: " + item._r + " " + item._t.ToString());
            }
        }

        private void DisplayModelAttributes(ShapeModel[] models, StreamWriter writer)
        {
            models.ToList().ForEach(t => writer.WriteLine("--> MODEL ATTRIBUTE: " + t.modelId));
            models.ToList().ForEach(t => DisplayAttributes(t.attributes, writer));
        }

        private static string ReadSTRING(BinaryReader reader)
        {
            var size = reader.ReadInt32();
            var bytes = reader.ReadBytes(size);
            string text = Encoding.UTF8.GetString(bytes);
            return text;
        }

        private delegate T ItemReader<T>(BinaryReader reader);

        private static T[] ReadArray<T>(BinaryReader reader, ItemReader<T> itemReader)
        {
            int size = reader.ReadInt32();
            return Enumerable.Range(0, size)
                .Select(i => itemReader(reader)).ToArray();
        }

        private static KeyValue[] ReadDICT(BinaryReader reader)
        {
            int size = reader.ReadInt32();
            return Enumerable.Range(0, size)
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
            var groupNodeChunk = new GroupNodeChunk
            {
                id = chunkReader.ReadInt32(),
                attributes = ReadDICT(chunkReader),
                childIds = ReadArray(chunkReader, r => r.ReadInt32())
            };
            return groupNodeChunk;
        }

        private TransformNodeChunk ReadTransformNodeChunk(BinaryReader chunkReader)
        {
            var transformNodeChunk = new TransformNodeChunk()
            {
                id = chunkReader.ReadInt32(),
                attributes = ReadDICT(chunkReader),
                childId = chunkReader.ReadInt32(),
                reservedId = chunkReader.ReadInt32(),
                layerId = chunkReader.ReadInt32(),
                frameAttributes = ReadArray(chunkReader, r => new DICT(ReadDICT(r)))
            };
            return transformNodeChunk;
        }
    }
}
