using System;
using System.Linq;

namespace FileToVoxCore.Vox.Chunks
{
    public class MaterialChunk
    {
        public int Id;
        public KeyValue[] Properties;

        public MaterialType Type
        {
            get
            {
                MaterialType result = MaterialType._diffuse;
                KeyValue item = Properties.FirstOrDefault(i => i.Key == "_type");
                if (item.Key != null)
                    Enum.TryParse(item.Value, out result);
                return result;
            }
        }

        public float Weight
        {
            get
            {
                float result = 1f;
                KeyValue item = Properties.FirstOrDefault(i => i.Key == "_weight");
                if (item.Key != null)
                    float.TryParse(item.Value, out result);
                return result;
            }
        }

        public float Rough
        {
            get
            {
                float result = 1f;
                KeyValue item = Properties.FirstOrDefault(i => i.Key == "_rough");
                if (item.Key != null)
                    float.TryParse(item.Value, out result);
                return result;
            }
        }

        public float Spec
        {
            get
            {
                float result = 1f;
                KeyValue item = Properties.FirstOrDefault(i => i.Key == "_spec");
                if (item.Key != null)
                    float.TryParse(item.Value, out result);
                return result;
            }
        }
        public float Flux
        {
            get
            {
                float result = 1f;
                KeyValue item = Properties.FirstOrDefault(i => i.Key == "_flux");
                if (item.Key != null)
                    float.TryParse(item.Value, out result);
                return result;
            }
        }

        public float Smoothness => 1 - Rough;
        public float Metallic => Type == MaterialType._metal ? Weight : 0;
        public float Emission => Type == MaterialType._emit ? Weight * Flux : 0;
        public float Transparency => Type == MaterialType._glass ? Weight : 0;
        public float Alpha => 1 - Transparency;
    }
}
