namespace SchematicToVoxCore.Vox.Chunks
{
    public class ShapeModel
    {
        public int modelId;
        public KeyValue[] attributes; // reserved
    }

    public class ShapeNodeChunk : NodeChunk
    { // nSHP: Shape Node Chunk
        public ShapeModel[] models;
        public override NodeType Type => NodeType.Shape;
    }
}
