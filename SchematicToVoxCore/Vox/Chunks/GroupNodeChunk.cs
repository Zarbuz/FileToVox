namespace SchematicToVoxCore.Vox.Chunks
{
    public class GroupNodeChunk : NodeChunk
    { // nGRP: Group Node Chunk
        public int[] childIds;
        public override NodeType Type => NodeType.Group;
    }
}
