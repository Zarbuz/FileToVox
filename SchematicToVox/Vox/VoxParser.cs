using SchematicToVox.Tools;
using SchematicToVox.Vox;
using SchematicToVox.Vox.Chunks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Vox
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

        private int _childCount = 0;
        private int _chunkCount = 0;
        private readonly string _logOutputFile;

        public VoxParser()
        {
            _logOutputFile = DateTime.Now.ToString("y-MM-d_HH.m.s") + ".txt";
        }

        #region Read
        public bool LoadModel(string absolutePath, VoxModel output)
        {
            var name = Path.GetFileNameWithoutExtension(absolutePath);
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
                _childCount = 0;
                _chunkCount = 0;
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                    ReadChunk(reader, output);
            }
            if (output.palette == null)
                output.palette = LoadDefaultPalette();
            return true;
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
                        var voxelCount = chunkReader.ReadInt32();
                        var frame = output.voxelFrames[_childCount - 1];
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
            string path = "../../logs/" + _logOutputFile;
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
            frameAttributes.ToList().ForEach(t => writer.WriteLine("--> FRAME ATTRIBUTE: " + t._r + " " + t._t.ToString()));
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

        /// <summary>
        /// Clear model data
        /// </summary>
        /// <param name="model"></param>
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
        #endregion

        #region Default Palette
        private static uint[] default_palette = new uint[] {
            0x00000000, 0xffffffff, 0xffccffff, 0xff99ffff, 0xff66ffff, 0xff33ffff, 0xff00ffff, 0xffffccff, 0xffccccff, 0xff99ccff, 0xff66ccff, 0xff33ccff, 0xff00ccff, 0xffff99ff, 0xffcc99ff, 0xff9999ff,
            0xff6699ff, 0xff3399ff, 0xff0099ff, 0xffff66ff, 0xffcc66ff, 0xff9966ff, 0xff6666ff, 0xff3366ff, 0xff0066ff, 0xffff33ff, 0xffcc33ff, 0xff9933ff, 0xff6633ff, 0xff3333ff, 0xff0033ff, 0xffff00ff,
            0xffcc00ff, 0xff9900ff, 0xff6600ff, 0xff3300ff, 0xff0000ff, 0xffffffcc, 0xffccffcc, 0xff99ffcc, 0xff66ffcc, 0xff33ffcc, 0xff00ffcc, 0xffffcccc, 0xffcccccc, 0xff99cccc, 0xff66cccc, 0xff33cccc,
            0xff00cccc, 0xffff99cc, 0xffcc99cc, 0xff9999cc, 0xff6699cc, 0xff3399cc, 0xff0099cc, 0xffff66cc, 0xffcc66cc, 0xff9966cc, 0xff6666cc, 0xff3366cc, 0xff0066cc, 0xffff33cc, 0xffcc33cc, 0xff9933cc,
            0xff6633cc, 0xff3333cc, 0xff0033cc, 0xffff00cc, 0xffcc00cc, 0xff9900cc, 0xff6600cc, 0xff3300cc, 0xff0000cc, 0xffffff99, 0xffccff99, 0xff99ff99, 0xff66ff99, 0xff33ff99, 0xff00ff99, 0xffffcc99,
            0xffcccc99, 0xff99cc99, 0xff66cc99, 0xff33cc99, 0xff00cc99, 0xffff9999, 0xffcc9999, 0xff999999, 0xff669999, 0xff339999, 0xff009999, 0xffff6699, 0xffcc6699, 0xff996699, 0xff666699, 0xff336699,
            0xff006699, 0xffff3399, 0xffcc3399, 0xff993399, 0xff663399, 0xff333399, 0xff003399, 0xffff0099, 0xffcc0099, 0xff990099, 0xff660099, 0xff330099, 0xff000099, 0xffffff66, 0xffccff66, 0xff99ff66,
            0xff66ff66, 0xff33ff66, 0xff00ff66, 0xffffcc66, 0xffcccc66, 0xff99cc66, 0xff66cc66, 0xff33cc66, 0xff00cc66, 0xffff9966, 0xffcc9966, 0xff999966, 0xff669966, 0xff339966, 0xff009966, 0xffff6666,
            0xffcc6666, 0xff996666, 0xff666666, 0xff336666, 0xff006666, 0xffff3366, 0xffcc3366, 0xff993366, 0xff663366, 0xff333366, 0xff003366, 0xffff0066, 0xffcc0066, 0xff990066, 0xff660066, 0xff330066,
            0xff000066, 0xffffff33, 0xffccff33, 0xff99ff33, 0xff66ff33, 0xff33ff33, 0xff00ff33, 0xffffcc33, 0xffcccc33, 0xff99cc33, 0xff66cc33, 0xff33cc33, 0xff00cc33, 0xffff9933, 0xffcc9933, 0xff999933,
            0xff669933, 0xff339933, 0xff009933, 0xffff6633, 0xffcc6633, 0xff996633, 0xff666633, 0xff336633, 0xff006633, 0xffff3333, 0xffcc3333, 0xff993333, 0xff663333, 0xff333333, 0xff003333, 0xffff0033,
            0xffcc0033, 0xff990033, 0xff660033, 0xff330033, 0xff000033, 0xffffff00, 0xffccff00, 0xff99ff00, 0xff66ff00, 0xff33ff00, 0xff00ff00, 0xffffcc00, 0xffcccc00, 0xff99cc00, 0xff66cc00, 0xff33cc00,
            0xff00cc00, 0xffff9900, 0xffcc9900, 0xff999900, 0xff669900, 0xff339900, 0xff009900, 0xffff6600, 0xffcc6600, 0xff996600, 0xff666600, 0xff336600, 0xff006600, 0xffff3300, 0xffcc3300, 0xff993300,
            0xff663300, 0xff333300, 0xff003300, 0xffff0000, 0xffcc0000, 0xff990000, 0xff660000, 0xff330000, 0xff0000ee, 0xff0000dd, 0xff0000bb, 0xff0000aa, 0xff000088, 0xff000077, 0xff000055, 0xff000044,
            0xff000022, 0xff000011, 0xff00ee00, 0xff00dd00, 0xff00bb00, 0xff00aa00, 0xff008800, 0xff007700, 0xff005500, 0xff004400, 0xff002200, 0xff001100, 0xffee0000, 0xffdd0000, 0xffbb0000, 0xffaa0000,
            0xff880000, 0xff770000, 0xff550000, 0xff440000, 0xff220000, 0xff110000, 0xffeeeeee, 0xffdddddd, 0xffbbbbbb, 0xffaaaaaa, 0xff888888, 0xff777777, 0xff555555, 0xff444444, 0xff222222, 0xff111111
        };
        #endregion
    }


}
