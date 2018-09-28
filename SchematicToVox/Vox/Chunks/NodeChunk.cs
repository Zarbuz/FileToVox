using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Vox.Chunks
{
    public struct KeyValue
    {
        public string Key, Value;
    }

    public enum NodeType { Transform, Group, Shape, }

    public abstract class NodeChunk
    {
        public int id;
        public KeyValue[] attributes;
        public abstract NodeType Type { get; }
    }
}
