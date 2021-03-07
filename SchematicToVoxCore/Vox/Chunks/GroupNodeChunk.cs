namespace FileToVox.Vox.Chunks
{
    public class GroupNodeChunk : NodeChunk
    { // nGRP: Group Node Chunk
        public int[] ChildIds;
        public override NodeType Type => NodeType.Group;
    }
}
