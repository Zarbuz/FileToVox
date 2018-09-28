using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Vox.Chunks
{
    public class GroupNodeChunk : NodeChunk
    { // nGRP: Group Node Chunk
        public int[] childIds;
        public override NodeType Type => NodeType.Group;
    }
}
