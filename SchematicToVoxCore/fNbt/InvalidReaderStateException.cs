using System;
using JetBrains.Annotations;

namespace fNbt {
    /// <summary> Exception thrown when an operation is attempted on an NbtReader that
    /// cannot recover from a previous parsing error. </summary>
    [Serializable]
    public sealed class InvalidReaderStateException : InvalidOperationException {
        internal InvalidReaderStateException([NotNull] string message)
            : base(message) {}
    }
}
