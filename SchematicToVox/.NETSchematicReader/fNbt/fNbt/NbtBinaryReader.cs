using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace fNbt {
    /// <summary> BinaryReader wrapper that takes care of reading primitives from an NBT stream,
    /// while taking care of endianness, string encoding, and skipping. </summary>
    internal sealed class NbtBinaryReader : BinaryReader {
        readonly byte[] buffer = new byte[sizeof(double)];

        byte[] seekBuffer;
        const int SeekBufferSize = 8*1024;
        readonly bool swapNeeded;
        readonly byte[] stringConversionBuffer = new byte[64];


        public NbtBinaryReader([NotNull] Stream input, bool bigEndian)
            : base(input) {
            swapNeeded = (BitConverter.IsLittleEndian == bigEndian);
        }


        public NbtTagType ReadTagType() {
            int type = ReadByte();
            if (type < 0) {
                throw new EndOfStreamException();
            } else if (type > (int)NbtTagType.IntArray) {
                throw new NbtFormatException("NBT tag type out of range: " + type);
            }
            return (NbtTagType)type;
        }


        public override short ReadInt16() {
            if (swapNeeded) {
                return Swap(base.ReadInt16());
            } else {
                return base.ReadInt16();
            }
        }


        public override int ReadInt32() {
            if (swapNeeded) {
                return Swap(base.ReadInt32());
            } else {
                return base.ReadInt32();
            }
        }


        public override long ReadInt64() {
            if (swapNeeded) {
                return Swap(base.ReadInt64());
            } else {
                return base.ReadInt64();
            }
        }


        public override float ReadSingle() {
            if (swapNeeded) {
                FillBuffer(sizeof(float));
                Array.Reverse(buffer, 0, sizeof(float));
                return BitConverter.ToSingle(buffer, 0);
            } else {
                return base.ReadSingle();
            }
        }


        public override double ReadDouble() {
            if (swapNeeded) {
                FillBuffer(sizeof(double));
                Array.Reverse(buffer);
                return BitConverter.ToDouble(buffer, 0);
            }
            return base.ReadDouble();
        }


        public override string ReadString() {
            short length = ReadInt16();
            if (length < 0) {
                throw new NbtFormatException("Negative string length given!");
            }
            if (length < stringConversionBuffer.Length) {
                int stringBytesRead = 0;
                while (stringBytesRead < length) {
                    int bytesReadThisTime = BaseStream.Read(stringConversionBuffer, stringBytesRead, length);
                    if (bytesReadThisTime == 0) {
                        throw new EndOfStreamException();
                    }
                    stringBytesRead += bytesReadThisTime;
                }
                return Encoding.UTF8.GetString(stringConversionBuffer, 0, length);
            } else {
                byte[] stringData = ReadBytes(length);
                if (stringData.Length < length) {
                    throw new EndOfStreamException();
                }
                return Encoding.UTF8.GetString(stringData);
            }
        }


        public void Skip(int bytesToSkip) {
            if (bytesToSkip < 0) {
                throw new ArgumentOutOfRangeException("bytesToSkip");
            } else if (BaseStream.CanSeek) {
                BaseStream.Position += bytesToSkip;
            } else if (bytesToSkip != 0) {
                if (seekBuffer == null) seekBuffer = new byte[SeekBufferSize];
                int bytesSkipped = 0;
                while (bytesSkipped < bytesToSkip) {
                    int bytesToRead = Math.Min(SeekBufferSize, bytesToSkip - bytesSkipped);
                    int bytesReadThisTime = BaseStream.Read(seekBuffer, 0, bytesToRead);
                    if (bytesReadThisTime == 0) {
                        throw new EndOfStreamException();
                    }
                    bytesSkipped += bytesReadThisTime;
                }
            }
        }


        new void FillBuffer(int numBytes) {
            int offset = 0;
            do {
                int num = BaseStream.Read(buffer, offset, numBytes - offset);
                if (num == 0) throw new EndOfStreamException();
                offset += num;
            } while (offset < numBytes);
        }


        public void SkipString() {
            short length = ReadInt16();
            if (length < 0) {
                throw new NbtFormatException("Negative string length given!");
            }
            Skip(length);
        }


        [DebuggerStepThrough]
        static short Swap(short v) {
            unchecked {
                return (short)((v >> 8) & 0x00FF |
                               (v << 8) & 0xFF00);
            }
        }


        [DebuggerStepThrough]
        static int Swap(int v) {
            unchecked {
                var v2 = (uint)v;
                return (int)((v2 >> 24) & 0x000000FF |
                             (v2 >> 8) & 0x0000FF00 |
                             (v2 << 8) & 0x00FF0000 |
                             (v2 << 24) & 0xFF000000);
            }
        }


        [DebuggerStepThrough]
        static long Swap(long v) {
            unchecked {
                return (Swap((int)v) & uint.MaxValue) << 32 |
                       Swap((int)(v >> 32)) & uint.MaxValue;
            }
        }


        [CanBeNull]
        public TagSelector Selector { get; set; }
    }
}
