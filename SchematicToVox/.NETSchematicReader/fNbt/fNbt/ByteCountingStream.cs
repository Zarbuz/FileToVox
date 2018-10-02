using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;

namespace fNbt {
    // Class used to count bytes read-from/written-to non-seekable streams.
    internal class ByteCountingStream : Stream {
        readonly Stream baseStream;

        // These are necessary to avoid counting bytes twice if ReadByte/WriteByte call Read/Write internally.
        bool readingOneByte, writingOneByte;

        // These are necessary to avoid counting bytes twice if Read/Write call ReadByte/WriteByte internally.
        bool readingManyBytes, writingManyBytes;


        public ByteCountingStream([NotNull] Stream stream) {
            Debug.Assert(stream != null);
            baseStream = stream;
        }


        public override void Flush() {
            baseStream.Flush();
        }


        public override long Seek(long offset, SeekOrigin origin) {
            return baseStream.Seek(offset, origin);
        }


        public override void SetLength(long value) {
            baseStream.SetLength(value);
        }


        public override int Read(byte[] buffer, int offset, int count) {
            readingManyBytes = true;
            int bytesActuallyRead = baseStream.Read(buffer, offset, count);
            readingManyBytes = false;
            if (!readingOneByte) BytesRead += bytesActuallyRead;
            return bytesActuallyRead;
        }


        public override void Write(byte[] buffer, int offset, int count) {
            writingManyBytes = true;
            baseStream.Write(buffer, offset, count);
            writingManyBytes = false;
            if (!writingOneByte) BytesWritten += count;
        }


        public override int ReadByte() {
            readingOneByte = true;
            int value = base.ReadByte();
            readingOneByte = false;
            if (value >= 0 && !readingManyBytes) BytesRead++;
            return value;
        }


        public override void WriteByte(byte value) {
            writingOneByte = true;
            base.WriteByte(value);
            writingOneByte = false;
            if (!writingManyBytes) BytesWritten++;
        }


        public override bool CanRead {
            get { return baseStream.CanRead; }
        }

        public override bool CanSeek {
            get { return baseStream.CanSeek; }
        }

        public override bool CanWrite {
            get { return baseStream.CanWrite; }
        }

        public override long Length {
            get { return baseStream.Length; }
        }

        public override long Position {
            get { return baseStream.Position; }
            set { baseStream.Position = value; }
        }

        public long BytesRead { get; private set; }
        public long BytesWritten { get; private set; }
    }
}
