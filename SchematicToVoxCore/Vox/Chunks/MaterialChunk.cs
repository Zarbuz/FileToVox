using System;
using System.Linq;

namespace SchematicToVoxCore.Vox.Chunks
{
    public class MaterialChunk
    {
        public int id;
        public KeyValue[] properties;

        public MaterialType Type
        {
            get
            {
                var result = MaterialType._diffuse;
                var item = properties.FirstOrDefault(i => i.Key == "_type");
                if (item.Key != null)
                    Enum.TryParse(item.Value, out result);
                return result;
            }
        }

        public float Weight
        {
            get
            {
                var result = 1f;
                var item = properties.FirstOrDefault(i => i.Key == "_weight");
                if (item.Key != null)
                    float.TryParse(item.Value, out result);
                return result;
            }
        }

        public float Rough
        {
            get
            {
                var result = 1f;
                var item = properties.FirstOrDefault(i => i.Key == "_rough");
                if (item.Key != null)
                    float.TryParse(item.Value, out result);
                return result;
            }
        }

        public float Spec
        {
            get
            {
                var result = 1f;
                var item = properties.FirstOrDefault(i => i.Key == "_spec");
                if (item.Key != null)
                    float.TryParse(item.Value, out result);
                return result;
            }
        }
        public float Flux
        {
            get
            {
                var result = 1f;
                var item = properties.FirstOrDefault(i => i.Key == "_flux");
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
