using System.Linq;

namespace FileToVox.Vox.Chunks
{
    public class LayerChunk
    { // LAYR: Layer Chunk
        public int id;
        public KeyValue[] attributes;
        public int unknown;

        public string Name
        {
            get
            {
                var item = attributes.FirstOrDefault(i => i.Key == "_name");
                return item.Key != null ? item.Value : string.Empty;
            }
        }
        public bool Hidden
        {
            get
            {
                var item = attributes.FirstOrDefault(i => i.Key == "_hidden");
                return item.Key != null ? item.Value != "0" : false;
            }
        }
    }
}
