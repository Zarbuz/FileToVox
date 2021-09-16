using System.Linq;

namespace FileToVoxCore.Vox.Chunks
{
    public class LayerChunk
    { // LAYR: Layer Chunk
        public int Id;
        public KeyValue[] Attributes;
        public int Unknown;

        public string Name
        {
            get
            {
                var item = Attributes.FirstOrDefault(i => i.Key == "_name");
                return item.Key != null ? item.Value : string.Empty;
            }
        }
        public bool Hidden
        {
            get
            {
                var item = Attributes.FirstOrDefault(i => i.Key == "_hidden");
                return item.Key != null ? item.Value != "0" : false;
            }
        }
    }
}
