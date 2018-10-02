using System;

namespace fNbt.Serialization {
    [Flags]
    internal enum TypeCategory {
        NotSupported = 0,
        Primitive = 1,
        ConvertiblePrimitive = 2,
        Enum = 4,
        MappedToPrimitive = Primitive | ConvertiblePrimitive | Enum,
        String = 8,
        ByteArray = 16,
        IntArray = 32,
        DirectlyMapped = MappedToPrimitive | String | ByteArray | IntArray,
        Array = 64,
        IList = 128,
        MappedToList = Array | IList,
        IDictionary = 256,
        ConvertibleByProperties = 512,
        MappedToCompound = ConvertibleByProperties | IDictionary,
        Mapped = DirectlyMapped | MappedToList | MappedToCompound,
        NbtTag = 1024,
        INbtSerializable = 2048
    }
}