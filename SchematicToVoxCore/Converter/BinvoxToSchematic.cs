using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using FileToVox.Schematics;
using FileToVox.Schematics.Tools;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Converter
{
    public static class BinvoxToSchematic
    {
        private static bool _headerRead;
        private static bool _voxelsRead;
        private static Vector3 _dimensions;
        private static Vector3 _translate;
        private static double _scale;
        private static byte[] _voxels;

        public static Schematic WriteSchematic(string inputFile)
        {
            _voxels = null;
            _headerRead = false;
            _voxelsRead = false;
            return WriteSchematicFromBinvox(inputFile);
        }

        private static Schematic WriteSchematicFromBinvox(string path)
        {
            using (LineReader lineReader = new LineReader(File.Open(path, FileMode.Open), Encoding.UTF8))
            {
                ReadHeader(lineReader);
                ReadVoxels(lineReader);
                Schematic schematic = new Schematic()
                {
                    Width = (short)_dimensions.Z,
                    Heigth = (short)_dimensions.Y,
                    Length = (short)_dimensions.X,
                    Blocks = new HashSet<Block>()
                };

                SchematicReader.HeightSchematic = schematic.Heigth;
                SchematicReader.LengthSchematic = schematic.Length;
                SchematicReader.WidthSchematic = schematic.Width;

                int xmult = (int)(_dimensions.Z * _dimensions.Y);
                int zmult = (int) (_dimensions.Z);


                for (int Y = 0; Y < schematic.Heigth; Y++)
                {
                    for (int Z = 0; Z < schematic.Length; Z++)
                    {
                        for (int X = 0; X < schematic.Width; X++)
                        {
                            int index = X * xmult + Z * zmult + Y;
                            if (_voxels[index] == 1)
                            {
                                schematic.Blocks.Add(new Block((short) X, (short) Y, (short) Z,
                                    Color.Wheat.ColorToUInt()));
                            }
                        }
                    }
                }

                return schematic;
            }
        }



        private static void ReadHeader(LineReader lineReader)
        {
            if (!_headerRead)
            {
                bool done = false;
                while (!done)
                {
                    string line = lineReader.ReadLine();
                    if (line.StartsWith("data"))
                    {
                        done = true;
                        _headerRead = true;
                    }
                    else if (line.StartsWith("dim"))
                    {
                        string[] dimensions = line.Split(' ');
                        _dimensions = new Vector3(int.Parse(dimensions[1]), int.Parse(dimensions[2]), int.Parse(dimensions[3]));
                        _voxels = new byte[(int)(_dimensions.X * _dimensions.Y * _dimensions.Z)];
                    }
                    else if (line.StartsWith("translate"))
                    {
                        string[] translations = line.Split(' ');
                        _translate = new Vector3(float.Parse(translations[1], CultureInfo.InvariantCulture), float.Parse(translations[2], CultureInfo.InvariantCulture), float.Parse(translations[3], CultureInfo.InvariantCulture));
                    }
                    else if (line.StartsWith("scale"))
                    {
                        string[] scales = line.Split(' ');
                        _scale = float.Parse(scales[1], CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        private static void ReadVoxels(LineReader lineReader)
        {
            if (!_voxelsRead)
            {
                int size = (int)(_dimensions.X * _dimensions.Y * _dimensions.Z);
                byte value;
                int count;
                int index = 0;
                int end_index = 0;

                while (end_index < size)
                {
                    value = lineReader.ReadByte();
                    count = (int)lineReader.ReadByte();
                    end_index = index + count;
                    for (int i = index; i < end_index; i++)
                    {
                        _voxels[i] = value;
                    }

                    index = end_index;
                }


                _voxelsRead = true;
            }
        }
    }

    public class LineReader : BinaryReader
    {
        private const int BufferSize = 1024;
        private readonly char[] _lineBuffer = new char[BufferSize];
        private readonly bool _clrf;

        public LineReader(Stream stream, Encoding encoding, bool clrf = false)
            : base(stream, encoding)
        {
            _clrf = clrf;
        }

        public string ReadLine()
        {
            int pos = 0;

            char[] buf = new char[2];

            StringBuilder stringBuffer = null;
            bool lineEndFound = false;

            while (base.Read(buf, 0, 1) > 0)
            {
                switch (buf[0])
                {
                    case '\r':
                        {
                            // grab buf[0]
                            _lineBuffer[pos++] = buf[0];
                            if (_clrf)
                            {// get the '\n'
                                char ch = base.ReadChar();
                                Debug.Assert(ch == '\n');
                            }
                            lineEndFound = true;
                            break;
                        }
                    case '\n':
                        lineEndFound = true;
                        break;
                    default:
                        {
                            _lineBuffer[pos] = buf[0];
                            pos += 1;

                            if (pos >= BufferSize)
                            {
                                stringBuffer = new StringBuilder(BufferSize + 80);
                                stringBuffer.Append(_lineBuffer, 0, BufferSize);
                                pos = 0;
                            }

                            break;
                        }
                }

                if (lineEndFound)
                {
                    switch (stringBuffer)
                    {
                        case null when pos > 0:
                            return new string(_lineBuffer, 0, pos);
                        case null:
                            return string.Empty;
                        default:
                            {
                                if (pos > 0)
                                {
                                    stringBuffer.Append(_lineBuffer, 0, pos);
                                }

                                return stringBuffer.ToString();
                            }
                    }
                }
            }

            if (stringBuffer != null)
            {
                if (pos > 0)
                {
                    stringBuffer.Append(_lineBuffer, 0, pos);
                }

                return stringBuffer.ToString();
            }

            return pos > 0 ? new string(_lineBuffer, 0, pos) : null;
        }

    }
}
