namespace FileToVoxCore.Vox.Chunks
{
    public class ShapeModel
    {
        public int ModelId;
        public KeyValue[] Attributes; // reserved
    }

    public class ShapeNodeChunk : NodeChunk
    { // nSHP: Shape Node Chunk
        public ShapeModel[] Models;
        public override NodeType Type => NodeType.Shape;
    }
}
