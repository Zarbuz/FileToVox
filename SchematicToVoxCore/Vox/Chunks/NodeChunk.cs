namespace FileToVox.Vox.Chunks
{
    public struct KeyValue
    {
        public string Key, Value;
    }

    public enum NodeType { Transform, Group, Shape, }

    public abstract class NodeChunk
    {
        public int Id;
        public KeyValue[] Attributes;
        public abstract NodeType Type { get; }
    }
}
