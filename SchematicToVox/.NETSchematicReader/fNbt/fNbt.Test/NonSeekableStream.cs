using System;
using System.IO;

namespace fNbt.Test {
    internal class NonSeekableStream : Stream {
        readonly Stream stream;


        public NonSeekableStream(Stream baseStream) {
            stream = baseStream;
        }


        public override bool CanRead {
            get { return stream.CanRead; }
        }

        public override bool CanSeek {
            get { return false; }
        }

        public override bool CanWrite {
            get { return stream.CanWrite; }
        }


        public override void Flush() {
            stream.Flush();
        }


        public override long Length {
            get { throw new NotSupportedException(); }
        }

        public override long Position {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }


        public override int Read(byte[] buffer, int offset, int count) {
            return stream.Read(buffer, offset, count);
        }


        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }


        public override void SetLength(long value) {
            throw new NotSupportedException();
        }


        public override void Write(byte[] buffer, int offset, int count) {
            stream.Write(buffer, offset, count);
        }
    }
}
