using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using FileToVoxCore.Vox.Chunks;

namespace FileToVoxCore.Vox
{
    public class VoxReader : VoxParser
    {
        private int mVoxelCountLastXyziChunk = 0;
        protected string LogOutputFile;
        private bool mWriteLog;
        public VoxModel LoadModel(string absolutePath, bool writeLog = false, bool debug = false)
        {
            VoxModel output = new VoxModel();
            var name = Path.GetFileNameWithoutExtension(absolutePath);
            mVoxelCountLastXyziChunk = 0;
            LogOutputFile = name + "-" + DateTime.Now.ToString("y-MM-d_HH.m.s") + ".txt";
            mWriteLog = writeLog;
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
                while (reader.BaseStream.Position != reader.BaseStream.Length)
                    ReadChunk(reader, output);
            }

            if (debug)
            {
                CheckDuplicateIds(output);
                CheckDuplicateChildGroupIds(output);
                CheckTransformIdNotInGroup(output);
                Console.ReadKey();
            }


            if (output.Palette == null)
                output.Palette = LoadDefaultPalette();
            return output;
        }

        private void CheckDuplicateIds(VoxModel output)
        {
            List<int> allIds = output.GroupNodeChunks.Select(t => t.Id).ToList();
            allIds.AddRange(output.TransformNodeChunks.Select(t => t.Id));
            allIds.AddRange(output.ShapeNodeChunks.Select(t => t.Id));

            List<int> duplicates = allIds.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();

            foreach (int Id in duplicates)
            {
                Console.WriteLine("[ERROR] Duplicate Id: " + Id);
            }
        }

        private void CheckDuplicateChildGroupIds(VoxModel output)
        {
            List<int> ChildIds = output.GroupNodeChunks.SelectMany(t => t.ChildIds).ToList();
            List<int> duplicates = ChildIds.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(y => y.Key)
                .ToList();

            foreach (int Id in duplicates)
            {
                Console.WriteLine("[ERROR] Duplicate child group Id: " + Id);
            }
        }

        private void CheckTransformIdNotInGroup(VoxModel output)
        {
            List<int> Ids = output.TransformNodeChunks.Select(t => t.Id).ToList();
            List<int> ChildIds = output.GroupNodeChunks.SelectMany(t => t.ChildIds).ToList();

            List<int> empty = new List<int>();
            foreach (int Id in Ids)
            {
                if (ChildIds.IndexOf(Id) == -1 && Id != 0)
                {
                    empty.Add(Id);
                }
            }

            foreach (int Id in empty)
            {
                Console.WriteLine("[ERROR] Transform Id never called in any group: " + Id);
            }
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
                result[i] = Color.FromArgb(a, r, g, b);
            }
            return result;
        }

        /// <summary>
        /// Load the Palette color. Plattes are offset by 1
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Color[] LoadPalette(BinaryReader reader)
        {
            var result = new Color[256];
            for (int i = 1; i < 256; i++)
            {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                byte a = reader.ReadByte();
                result[i] = Color.FromArgb(a, r, g, b);
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
            ChunkCount++;

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
                        if (ChildCount >= output.VoxelFrames.Count)
                            output.VoxelFrames.Add(new VoxelData());
                        output.VoxelFrames[ChildCount].Resize(w, h, d);
                        ChildCount++;
                        break;
                    case XYZI:
                        mVoxelCountLastXyziChunk = chunkReader.ReadInt32();
                        var frame = output.VoxelFrames[ChildCount - 1];
                        byte x, y, z, color;
                        for (int i = 0; i < mVoxelCountLastXyziChunk; i++)
                        {
                            x = chunkReader.ReadByte();
                            y = chunkReader.ReadByte();
                            z = chunkReader.ReadByte();
                            color = chunkReader.ReadByte();
                            frame.Set(x, y, z, color);
                            output.ColorUsed.Add(color);
                        }
                        break;
                    case RGBA:
                        output.Palette = LoadPalette(chunkReader);
                        break;
                    case MATT:
                        break;
                    case PACK:
                        int frameCount = chunkReader.ReadInt32();
                        for (int i = 0; i < frameCount; i++)
                        {
                            output.VoxelFrames.Add(new VoxelData());
                        }
                        break;
                    case nTRN:
                        output.TransformNodeChunks.Add(ReadTransformNodeChunk(chunkReader));
                        break;
                    case nGRP:
                        output.GroupNodeChunks.Add(ReadGroupNodeChunk(chunkReader));
                        break;
                    case nSHP:
                        output.ShapeNodeChunks.Add(ReadShapeNodeChunk(chunkReader));
                        break;
                    case LAYR:
                        output.LayerChunks.Add(ReadLayerChunk(chunkReader));
                        break;
                    case MATL:
                        output.MaterialChunks.Add(ReadMaterialChunk(chunkReader));
                        break;
                    case rOBJ:
                        output.RendererSettingChunks.Add(ReaddRObjectChunk(chunkReader));
                        break;
					case IMAP:
						output.PaletteColorIndex = new int[256];
						for (int i = 0; i < 256; i++)
						{
							int index = chunkReader.ReadByte();
							output.PaletteColorIndex[i] = index;
						}
						break;
					default:
                        Console.WriteLine($"Unknown chunk: \"{chunkName}\"");
                        break;
                }
            }

            if (mWriteLog)
            {
                WriteLogs(chunkName, chunkSize, childChunkSize, output);
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

        private void WriteLogs(string chunkName, int chunkSize, int childChunkSize, VoxModel output)
        {
            if (!Directory.Exists("logs"))
                Directory.CreateDirectory("logs");

            string path = "logs/" + LogOutputFile;
            using (var writer = new StreamWriter(path, true))
            {
                writer.WriteLine("CHUNK NAME: " + chunkName + " (" + ChunkCount + ")");
                writer.WriteLine("CHUNK SIZE: " + chunkSize + " BYTES");
                writer.WriteLine("CHILD CHUNK SIZE: " + childChunkSize);
                switch (chunkName)
                {
                    case SIZE:
                        var frame = output.VoxelFrames[ChildCount - 1];
                        writer.WriteLine("-> SIZE: " + frame.VoxelsWide + " " + frame.VoxelsTall + " " + frame.VoxelsDeep);
                        break;
                    case XYZI:
                        writer.WriteLine("-> XYZI: " + mVoxelCountLastXyziChunk);
                        break;
                    case nTRN:
                        var transform = output.TransformNodeChunks.Last();
                        writer.WriteLine("-> TRANSFORM NODE: " + transform.Id);
                        writer.WriteLine("--> CHILD Id: " + transform.ChildId);
                        writer.WriteLine("--> RESERVED Id: " + transform.ReservedId);
                        writer.WriteLine("--> LAYER Id: " + transform.LayerId);
                        DisplayAttributes(transform.Attributes, writer);
                        DisplayFrameAttributes(transform.FrameAttributes, writer);
                        break;
                    case nGRP:
                        var group = output.GroupNodeChunks.Last();
                        writer.WriteLine("-> GROUP NODE: " + group.Id);
                        group.ChildIds.ToList().ForEach(t => writer.WriteLine("--> CHILD Id: " + t));
                        DisplayAttributes(group.Attributes, writer);
                        break;
                    case nSHP:
                        var shape = output.ShapeNodeChunks.Last();
                        writer.WriteLine("-> SHAPE NODE: " + shape.Id);
                        DisplayAttributes(shape.Attributes, writer);
                        DisplayModelAttributes(shape.Models, writer);
                        break;
                    case LAYR:
                        var layer = output.LayerChunks.Last();
                        writer.WriteLine("-> LAYER NODE: " + layer.Id + " " +
                            layer.Name + " " +
                            layer.Hidden + " " +
                            layer.Unknown);
                        DisplayAttributes(layer.Attributes, writer);
                        break;
                    case MATL:
                        var material = output.MaterialChunks.Last();
                        writer.WriteLine("-> MATERIAL NODE: " + material.Id.ToString("F1"));
                        writer.WriteLine("--> ALPHA: " + material.Alpha.ToString("F1"));
                        writer.WriteLine("--> EMISSION: " + material.Emission.ToString("F1"));
                        writer.WriteLine("--> FLUX: " + material.Flux.ToString("F1"));
                        writer.WriteLine("--> METALLIC: " + material.Metallic.ToString("F1"));
                        writer.WriteLine("--> ROUGH: " + material.Rough.ToString("F1"));
                        writer.WriteLine("--> SMOOTHNESS: " + material.Smoothness.ToString("F1"));
                        writer.WriteLine("--> SPEC: " + material.Spec.ToString("F1"));
                        writer.WriteLine("--> WEIGHT: " + material.Weight.ToString("F1"));
                        DisplayAttributes(material.Properties, writer);
						break;
                    case IMAP:
	                    if (output.PaletteColorIndex != null)
	                    {
		                    foreach (int colorIndex in output.PaletteColorIndex)
		                    {
			                    writer.WriteLine("--> " + colorIndex);
		                    }
                        }
                        
                        break;
                }
                writer.WriteLine("");
                writer.Close();
            }
        }

        private void DisplayAttributes(KeyValue[] Attributes, StreamWriter writer)
        {
            Attributes.ToList().ForEach(t => writer.WriteLine("--> ATTRIBUTE: Key=" + t.Key + " Value=" + t.Value));
        }

        private void DisplayFrameAttributes(DICT[] FrameAttributes, StreamWriter writer)
        {
            var list = FrameAttributes.ToList();
            foreach (var item in list)
            {
                writer.WriteLine("--> FRAME ATTRIBUTE: " + item._r + " " + item._t.ToString());
            }
        }

        private void DisplayModelAttributes(ShapeModel[] Models, StreamWriter writer)
        {
            Models.ToList().ForEach(t => writer.WriteLine("--> MODEL ATTRIBUTE: " + t.ModelId));
            Models.ToList().ForEach(t => DisplayAttributes(t.Attributes, writer));
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
                Attributes = ReadDICT(chunkReader)
            };
        }

        private MaterialChunk ReadMaterialChunk(BinaryReader chunkReader)
        {
            return new MaterialChunk
            {
                Id = chunkReader.ReadInt32(),
                Properties = ReadDICT(chunkReader)
            };
        }

        private LayerChunk ReadLayerChunk(BinaryReader chunkReader)
        {
            return new LayerChunk
            {
                Id = chunkReader.ReadInt32(),
                Attributes = ReadDICT(chunkReader),
                Unknown = chunkReader.ReadInt32()
            };
        }

        private ShapeNodeChunk ReadShapeNodeChunk(BinaryReader chunkReader)
        {
            return new ShapeNodeChunk
            {
                Id = chunkReader.ReadInt32(),
                Attributes = ReadDICT(chunkReader),
                Models = ReadArray(chunkReader, r => new ShapeModel
                {
                    ModelId = r.ReadInt32(),
                    Attributes = ReadDICT(r)
                })
            };
        }

        private GroupNodeChunk ReadGroupNodeChunk(BinaryReader chunkReader)
        {
            var groupNodeChunk = new GroupNodeChunk
            {
                Id = chunkReader.ReadInt32(),
                Attributes = ReadDICT(chunkReader),
                ChildIds = ReadArray(chunkReader, r => r.ReadInt32())
            };
            return groupNodeChunk;
        }

        private TransformNodeChunk ReadTransformNodeChunk(BinaryReader chunkReader)
        {
            var transformNodeChunk = new TransformNodeChunk()
            {
                Id = chunkReader.ReadInt32(),
                Attributes = ReadDICT(chunkReader),
                ChildId = chunkReader.ReadInt32(),
                ReservedId = chunkReader.ReadInt32(),
                LayerId = chunkReader.ReadInt32(),
                FrameAttributes = ReadArray(chunkReader, r => new DICT(ReadDICT(r)))
            };
            return transformNodeChunk;
        }
    }
}
