using System.Linq;
using FileToVox.Schematics.Tools;

namespace FileToVox.Vox.Chunks
{
    public class TransformNodeChunk : NodeChunk
    { // nTRN: Transform Node Chunk
        public int childId;
        public int reservedId;
        public int layerId;
        public DICT[] frameAttributes;

        public override NodeType Type => NodeType.Transform;

        public Rotation RotationAt(int frame = 0)
            => frame < frameAttributes.Length ? frameAttributes[frame]._r : Rotation._PX_PY_P;
        public Vector3 TranslationAt(int frame = 0)
            => frame < frameAttributes.Length ? frameAttributes[frame]._t : Vector3.zero;

        public string Name
        {
            get
            {
                var kv = attributes.FirstOrDefault(a => a.Key == "_name");
                return kv.Key != null ? kv.Value : string.Empty;
            }
        }
        public bool Hidden
        {
            get
            {
                var kv = attributes.FirstOrDefault(a => a.Key == "_hidden");
                return kv.Key != null ? kv.Value != "0" : false;
            }
        }
    }
}
