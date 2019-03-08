namespace fNbt {
    /// <summary> Enumeration of named binary tag types, and their corresponding codes. </summary>
    public enum NbtTagType : byte {
        /// <summary> Placeholder TagType used to indicate unknown/undefined tag type in NbtList. </summary>
        Unknown = 0xff,

        /// <summary> TAG_End: This unnamed tag serves no purpose but to signify the end of an open TAG_Compound. </summary>
        End = 0x00,

        /// <summary> TAG_Byte: A single byte. </summary>
        Byte = 0x01,

        /// <summary> TAG_Short: A single signed 16-bit integer. </summary>
        Short = 0x02,

        /// <summary> TAG_Int: A single signed 32-bit integer. </summary>
        Int = 0x03,

        /// <summary> TAG_Long: A single signed 64-bit integer. </summary>
        Long = 0x04,

        /// <summary> TAG_Float: A single IEEE-754 single-precision floating point number. </summary>
        Float = 0x05,

        /// <summary> TAG_Double: A single IEEE-754 double-precision floating point number. </summary>
        Double = 0x06,

        /// <summary> TAG_Byte_Array: A length-prefixed array of bytes. </summary>
        ByteArray = 0x07,

        /// <summary> TAG_String: A length-prefixed UTF-8 string. </summary>
        String = 0x08,

        /// <summary> TAG_List: A list of nameless tags, all of the same type. </summary>
        List = 0x09,

        /// <summary> TAG_Compound: A set of named tags. </summary>
        Compound = 0x0a,

        /// <summary> TAG_Byte_Array: A length-prefixed array of signed 32-bit integers. </summary>
        IntArray = 0x0b
    }
}
