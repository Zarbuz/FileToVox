namespace fNbt {
    /// <summary> Compression method used for loading/saving NBT files. </summary>
    public enum NbtCompression {
        /// <summary> Automatically detect file compression. Not a valid format for saving. </summary>
        AutoDetect,

        /// <summary> No compression. </summary>
        None,

        /// <summary> Compressed, with GZip header (default). </summary>
        GZip,

        /// <summary> Compressed, with ZLib header (RFC-1950). </summary>
        ZLib
    }
}
