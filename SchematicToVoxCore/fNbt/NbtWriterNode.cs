namespace fNbt {
    // Represents state of a node in the NBT file tree, used by NbtWriter
    internal sealed class NbtWriterNode {
        public NbtTagType ParentType;
        public NbtTagType ListType;
        public int ListSize;
        public int ListIndex;
    }
}
