using System;
using System.IO;
using JetBrains.Annotations;

namespace fNbt.Test {
    class PartialReadStream : Stream {
        readonly byte[] placeholderBuffer = new byte[1];
        readonly Stream baseStream;


        public PartialReadStream([NotNull] Stream baseStream) {
            if (baseStream == null) throw new ArgumentNullException("baseStream");
            this.baseStream = baseStream;
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
            int rv = baseStream.Read(placeholderBuffer, 0, 1);
            if (rv <= 0) {
                return rv;
            } else {
                buffer[offset] = placeholderBuffer[0];
                return 1;
            }
        }


        public override void Write(byte[] buffer, int offset, int count) {
            throw new NotSupportedException();
        }


        public override bool CanRead {
            get { return true; }
        }

        public override bool CanSeek {
            get { return baseStream.CanSeek; }
        }

        public override bool CanWrite {
            get { return false; }
        }

        public override long Length {
            get { return baseStream.Length; }
        }

        public override long Position {
            get { return baseStream.Position; }
            set { baseStream.Position= value; }
        }
    }
}
