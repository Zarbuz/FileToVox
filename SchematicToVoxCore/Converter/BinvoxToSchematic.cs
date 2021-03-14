using FileToVox.Schematics;
using FileToVox.Schematics.Tools;
using SchematicToVoxCore.Extensions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;

namespace FileToVox.Converter
{
	public class BinvoxToSchematic : AbstractToSchematic
    {
        private bool mHeaderRead;
        private bool mVoxelsRead;
        private Vector3 mDimensions;
        private Vector3 mTranslate;
        private double mScale;
        private byte[] mVoxels;

        public BinvoxToSchematic(string path) : base(path)
        {
        }

        public override Schematic WriteSchematic()
        {
            mVoxels = null;
            mHeaderRead = false;
            mVoxelsRead = false;
            return WriteSchematicFromBinvox();
        }

        private Schematic WriteSchematicFromBinvox()
        {
            using (LineReader lineReader = new LineReader(File.Open(_path, FileMode.Open), Encoding.UTF8))
            {
                ReadHeader(lineReader);
                ReadVoxels(lineReader);
                Schematic schematic = new Schematic();

                int xmult = (int)(mDimensions.Z * mDimensions.Y);
                int zmult = (int) (mDimensions.Z);

                for (int Y = 0; Y < mDimensions.Y; Y++)
                {
                    for (int Z = 0; Z < mDimensions.Z; Z++)
                    {
                        for (int X = 0; X < mDimensions.X; X++)
                        {
                            int index = X * xmult + Z * zmult + Y;
                            if (mVoxels[index] == 1)
                            {
                                schematic.Blocks.Add(new Voxel((ushort) X, (ushort) Y, (ushort) Z, Color.Wheat.ColorToUInt()));
                            }
                        }
                    }
                }

                return schematic;
            }
        }



        private void ReadHeader(LineReader lineReader)
        {
            if (!mHeaderRead)
            {
                bool done = false;
                while (!done)
                {
                    string line = lineReader.ReadLine();
                    if (line.StartsWith("data"))
                    {
                        done = true;
                        mHeaderRead = true;
                    }
                    else if (line.StartsWith("dim"))
                    {
                        string[] dimensions = line.Split(' ');
                        mDimensions = new Vector3(int.Parse(dimensions[1]), int.Parse(dimensions[2]), int.Parse(dimensions[3]));
                        mVoxels = new byte[(int)(mDimensions.X * mDimensions.Y * mDimensions.Z)];
                    }
                    else if (line.StartsWith("translate"))
                    {
                        string[] translations = line.Split(' ');
                        mTranslate = new Vector3(float.Parse(translations[1], CultureInfo.InvariantCulture), float.Parse(translations[2], CultureInfo.InvariantCulture), float.Parse(translations[3], CultureInfo.InvariantCulture));
                    }
                    else if (line.StartsWith("scale"))
                    {
                        string[] scales = line.Split(' ');
                        mScale = float.Parse(scales[1], CultureInfo.InvariantCulture);
                    }
                }
            }
        }

        private void ReadVoxels(LineReader lineReader)
        {
            if (!mVoxelsRead)
            {
                int size = (int)(mDimensions.X * mDimensions.Y * mDimensions.Z);
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
                        mVoxels[i] = value;
                    }

                    index = end_index;
                }


                mVoxelsRead = true;
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
