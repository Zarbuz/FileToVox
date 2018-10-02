using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;

namespace fNbt {
    /// <summary> DeflateStream wrapper that calculates Adler32 checksum of the written data,
    /// to allow writing ZLib header (RFC-1950). </summary>
    internal sealed class ZLibStream : DeflateStream {
        int adler32A = 1,
            adler32B;

        const int ChecksumModulus = 65521;

        public int Checksum {
            get { return unchecked((adler32B*65536) + adler32A); }
        }


        void UpdateChecksum([NotNull] IList<byte> data, int offset, int length) {
            for (int counter = 0; counter < length; ++counter) {
                adler32A = (adler32A + (data[offset + counter]))%ChecksumModulus;
                adler32B = (adler32B + adler32A)%ChecksumModulus;
            }
        }


        public ZLibStream([NotNull] Stream stream, CompressionMode mode, bool leaveOpen)
            : base(stream, mode, leaveOpen) {}


        public override void Write(byte[] array, int offset, int count) {
            UpdateChecksum(array, offset, count);
            base.Write(array, offset, count);
        }
    }
}
