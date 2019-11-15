using FileToVox.Schematics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using FileToVox.Schematics.Tools;
using FileToVox.Utils;
using MoreLinq;
using nQuant;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Converter
{
    public class PLYToSchematic : BaseToSchematic
    {
        private readonly List<Block> _blocks = new List<Block>();

        #region Internal data structure

        enum DataProperty
        {
            Invalid,
            R8, G8, B8, A8,
            R16, G16, B16, A16,
            SingleX, SingleY, SingleZ,
            DoubleX, DoubleY, DoubleZ,
            Data8, Data16, Data32, Data64
        }

        static int GetPropertySize(DataProperty p)
        {
            switch (p)
            {
                case DataProperty.R8: return 1;
                case DataProperty.G8: return 1;
                case DataProperty.B8: return 1;
                case DataProperty.A8: return 1;
                case DataProperty.R16: return 2;
                case DataProperty.G16: return 2;
                case DataProperty.B16: return 2;
                case DataProperty.A16: return 2;
                case DataProperty.SingleX: return 4;
                case DataProperty.SingleY: return 4;
                case DataProperty.SingleZ: return 4;
                case DataProperty.DoubleX: return 8;
                case DataProperty.DoubleY: return 8;
                case DataProperty.DoubleZ: return 8;
                case DataProperty.Data8: return 1;
                case DataProperty.Data16: return 2;
                case DataProperty.Data32: return 4;
                case DataProperty.Data64: return 8;
            }
            return 0;
        }

        class DataHeader
        {
            public List<DataProperty> properties = new List<DataProperty>();
            public int vertexCount = -1;
        }

        class DataBody
        {
            public List<Vector3> vertices;
            public List<Color> colors;

            public DataBody(int vertexCount)
            {
                vertices = new List<Vector3>(vertexCount);
                colors = new List<Color>(vertexCount);
            }

            public void AddPoint(
                float x, float y, float z,
                byte r, byte g, byte b
            )
            {
                vertices.Add(new Vector3(x, y, z));
                colors.Add(Color.FromArgb(r, g, b));
            }
        }

        #endregion

        #region Static Methods
        private static DataHeader ReadDataHeader(StreamReader reader)
        {
            DataHeader data = new DataHeader();
            int readCount = 0;

            // Magic number line ("ply")
            string line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "ply")
                throw new ArgumentException("Magic number ('ply') mismatch.");

            // Data format: check if it's binary/little endian.
            line = reader.ReadLine();
            readCount += line.Length + 1;
            if (line != "format binary_little_endian 1.0")
                throw new ArgumentException(
                    "Invalid data format ('" + line + "'). " +
                    "Should be binary/little endian.");

            // Read header contents.
            for (bool skip = false; ;)
            {
                // Read a line and split it with white space.
                line = reader.ReadLine();
                readCount += line.Length + 1;
                if (line == "end_header") break;
                string[] col = line.Split();

                // Element declaration (unskippable)
                if (col[0] == "element")
                {
                    if (col[1] == "vertex")
                    {
                        data.vertexCount = Convert.ToInt32(col[2]);
                        skip = false;
                    }
                    else
                    {
                        // Don't read elements other than vertices.
                        skip = true;
                    }
                }

                if (skip) continue;

                // Property declaration line
                if (col[0] == "property")
                {
                    DataProperty prop = DataProperty.Invalid;

                    // Parse the property name entry.
                    switch (col[2])
                    {
                        case "red": prop = DataProperty.R8; break;
                        case "green": prop = DataProperty.G8; break;
                        case "blue": prop = DataProperty.B8; break;
                        case "alpha": prop = DataProperty.A8; break;
                        case "x": prop = DataProperty.SingleX; break;
                        case "y": prop = DataProperty.SingleY; break;
                        case "z": prop = DataProperty.SingleZ; break;
                    }

                    switch (col[1])
                    {
                        // Check the property type.
                        case "char":
                        case "uchar":
                        case "int8":
                        case "uint8":
                            {
                                if (prop == DataProperty.Invalid)
                                    prop = DataProperty.Data8;
                                else if (GetPropertySize(prop) != 1)
                                    throw new ArgumentException("Invalid property type ('" + line + "').");
                                break;
                            }
                        case "short":
                        case "ushort":
                        case "int16":
                        case "uint16":
                            {
                                switch (prop)
                                {
                                    case DataProperty.Invalid: prop = DataProperty.Data16; break;
                                    case DataProperty.R8: prop = DataProperty.R16; break;
                                    case DataProperty.G8: prop = DataProperty.G16; break;
                                    case DataProperty.B8: prop = DataProperty.B16; break;
                                    case DataProperty.A8: prop = DataProperty.A16; break;
                                }
                                if (GetPropertySize(prop) != 2)
                                    throw new ArgumentException("Invalid property type ('" + line + "').");
                                break;
                            }
                        case "int":
                        case "uint":
                        case "float":
                        case "int32":
                        case "uint32":
                        case "float32":
                            {
                                if (prop == DataProperty.Invalid)
                                    prop = DataProperty.Data32;
                                else if (GetPropertySize(prop) != 4)
                                    throw new ArgumentException("Invalid property type ('" + line + "').");
                                break;
                            }
                        case "int64":
                        case "uint64":
                        case "double":
                        case "float64":
                            {
                                switch (prop)
                                {
                                    case DataProperty.Invalid: prop = DataProperty.Data64; break;
                                    case DataProperty.SingleX: prop = DataProperty.DoubleX; break;
                                    case DataProperty.SingleY: prop = DataProperty.DoubleY; break;
                                    case DataProperty.SingleZ: prop = DataProperty.DoubleZ; break;
                                }
                                if (GetPropertySize(prop) != 8)
                                    throw new ArgumentException("Invalid property type ('" + line + "').");
                                break;
                            }
                        default:
                            throw new ArgumentException("Unsupported property type ('" + line + "').");
                    }

                    data.properties.Add(prop);
                }
            }

            // Rewind the stream back to the exact position of the reader.
            reader.BaseStream.Position = readCount;

            return data;
        }

        private static DataBody ReadDataBody(DataHeader header, BinaryReader reader)
        {
            DataBody data = new DataBody(header.vertexCount);

            float x = 0, y = 0, z = 0;
            byte r = 255, g = 255, b = 255, a = 255;

            for (int i = 0; i < header.vertexCount; i++)
            {
                foreach (DataProperty prop in header.properties)
                {
                    switch (prop)
                    {
                        case DataProperty.R8: r = reader.ReadByte(); break;
                        case DataProperty.G8: g = reader.ReadByte(); break;
                        case DataProperty.B8: b = reader.ReadByte(); break;
                        case DataProperty.A8: a = reader.ReadByte(); break;

                        case DataProperty.R16: r = (byte)(reader.ReadUInt16() >> 8); break;
                        case DataProperty.G16: g = (byte)(reader.ReadUInt16() >> 8); break;
                        case DataProperty.B16: b = (byte)(reader.ReadUInt16() >> 8); break;
                        case DataProperty.A16: a = (byte)(reader.ReadUInt16() >> 8); break;

                        case DataProperty.SingleX: x = reader.ReadSingle(); break;
                        case DataProperty.SingleY: y = reader.ReadSingle(); break;
                        case DataProperty.SingleZ: z = reader.ReadSingle(); break;

                        case DataProperty.DoubleX: x = (float)reader.ReadDouble(); break;
                        case DataProperty.DoubleY: y = (float)reader.ReadDouble(); break;
                        case DataProperty.DoubleZ: z = (float)reader.ReadDouble(); break;

                        case DataProperty.Data8: reader.ReadByte(); break;
                        case DataProperty.Data16: reader.BaseStream.Position += 2; break;
                        case DataProperty.Data32: reader.BaseStream.Position += 4; break;
                        case DataProperty.Data64: reader.BaseStream.Position += 8; break;
                    }
                }

                data.AddPoint(x, y, z, r, g, b);
            }

            return data;
        }
        #endregion

        public PLYToSchematic(string path, int _scale) : base(path)
        {
            FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            DataHeader header = ReadDataHeader(new StreamReader(stream));
            DataBody body = ReadDataBody(header, new BinaryReader(stream));

            List<Vector3> bodyVertices = body.vertices;
            List<Color> bodyColors = body.colors;

            Vector3 minX = bodyVertices.MinBy(t => t.X);
            Vector3 minY = bodyVertices.MinBy(t => t.Y);
            Vector3 minZ = bodyVertices.MinBy(t => t.Z);

            float min = Math.Abs(Math.Min(minX.X, Math.Min(minY.Y, minZ.Z)));
            for (int i = 0; i < bodyVertices.Count; i++)
            {
                bodyVertices[i] += new Vector3(min, min, min);
                bodyVertices[i] = new Vector3((float)Math.Truncate(bodyVertices[i].X * _scale), (float)Math.Truncate(bodyVertices[i].Y * _scale), (float)Math.Truncate(bodyVertices[i].Z * _scale));
            }

            HashSet<Vector3> set = new HashSet<Vector3>();
            List<Vector3> vertices = new List<Vector3>();
            List<Color> colors = new List<Color>();

            using (ProgressBar progressbar = new ProgressBar())
            {
                for (int i = 0; i < bodyVertices.Count; i++)
                {
                    if (!set.Contains(bodyVertices[i]))
                    {
                        set.Add(bodyVertices[i]);
                        vertices.Add(bodyVertices[i]);
                        colors.Add(bodyColors[i]);
                    }
                    progressbar.Report(i / (float)bodyVertices.Count);
                }
            }

            minX = vertices.MinBy(t => t.X);
            minY = vertices.MinBy(t => t.Y);
            minZ = vertices.MinBy(t => t.Z);

            min = Math.Min(minX.X, Math.Min(minY.Y, minZ.Z));
            for (int i = 0; i < vertices.Count; i++)
            {
                float max = Math.Max(vertices[i].X, Math.Max(vertices[i].Y, vertices[i].Z));
                if (max - min < 2016 && max - min >= 0)
                {
                    vertices[i] -= new Vector3(min, min, min);
                    _blocks.Add(new Block((short)vertices[i].X, (short)vertices[i].Y, (short)vertices[i].Z, colors[i].ColorToUInt()));
                }
            }

        }

        public override Schematic WriteSchematic()
        {
            Schematic schematic = new Schematic()
            {
                Length = _blocks.MaxBy(t => t.Z).Z,
                Width = _blocks.MaxBy(t => t.X).X,
                Heigth = _blocks.MaxBy(t => t.Y).Y,
                Blocks = new HashSet<Block>()
            };

            LoadedSchematic.LengthSchematic = schematic.Length;
            LoadedSchematic.WidthSchematic = schematic.Width;
            LoadedSchematic.HeightSchematic = schematic.Heigth;
            ApplyQuantization();

            foreach (Block t in _blocks)
            {
                schematic.Blocks.Add(t);
            }

            return schematic;
        }

        private Bitmap CreateBitmapFromColors()
        {
            int width = _blocks.Count;

            Bitmap bitmap = new Bitmap(width, 1);

            for (int i = 0; i < _blocks.Count; i++)
            {
                Block block = _blocks[i];
                Color color = block.Color.UIntToColor();
                int x = i % width;
                int y = i / width;
                bitmap.SetPixel(x, y, color);
            }

            return bitmap;
        }

        private Bitmap Quantization(Bitmap bitmap)
        {
            Dictionary<Color, int> histo = new Dictionary<Color, int>();
            for (int x = 0; x < bitmap.Size.Width; x++)
            {
                for (int y = 0; y < bitmap.Size.Height; y++)
                {
                    Color c = bitmap.GetPixel(x, y);
                    if (histo.ContainsKey(c))
                        histo[c] = histo[c] + 1;
                    else
                        histo.Add(c, 1);
                }
            }

            IOrderedEnumerable<KeyValuePair<Color, int>> result1 = histo.OrderByDescending(a => a.Value);
            int number = 255;
            List<Color> mostUsedColor = result1.Select(x => x.Key).Take(number).ToList();

            double temp;
            Dictionary<Color, Double> dist = new Dictionary<Color, double>();
            Dictionary<Color, Color> mapping = new Dictionary<Color, Color>();
            foreach (var p in result1)
            {
                dist.Clear();
                foreach (Color pp in mostUsedColor)
                {
                    temp = Math.Abs(p.Key.R - pp.R) +
                           Math.Abs(p.Key.R - pp.R) +
                           Math.Abs(p.Key.R - pp.R);
                    dist.Add(pp, temp);
                }
                KeyValuePair<Color, double> min = dist.OrderBy(k => k.Value).FirstOrDefault();
                mapping.Add(p.Key, min.Key);
            }
            Bitmap copy = new Bitmap(bitmap);

            for (int x = 0; x < copy.Size.Width; x++)
            {
                for (int y = 0; y < copy.Size.Height; y++)
                {
                    Color c = copy.GetPixel(x, y); // **2**
                    copy.SetPixel(x, y, mapping[c]);
                }
            }

            return copy;
        }

        private void ApplyQuantization()
        {
            var quantizer = new WuQuantizer();
            using (Bitmap bitmap = CreateBitmapFromColors())
            {
                using (Image quantized = quantizer.QuantizeImage(bitmap))
                {
                    Bitmap reducedBitmap = new Bitmap(quantized);
                    int width = reducedBitmap.Size.Width;
                    for (int i = 0; i < _blocks.Count; i++)
                    {
                        int x = i % width;
                        int y = i / width;
                        _blocks[i] = new Block(_blocks[i].X, _blocks[i].Y, _blocks[i].Z, reducedBitmap.GetPixel(x, y).ColorToUInt());
                    }
                }
            }
            //Bitmap bitmap = CreateBitmapFromColors();
            //Bitmap reducedBitmap = Quantization(bitmap);

        }
    }

}
