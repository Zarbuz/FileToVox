using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FileToVoxCore.Schematics.Tools;
using FileToVoxCore.Vox.Chunks;

namespace FileToVoxCore.Vox
{
    public class VoxModel
    {
        public Color[] Palette;
        public HashSet<int> ColorUsed = new HashSet<int>();
        public List<VoxelData> VoxelFrames = new List<VoxelData>();
        public List<MaterialChunk> MaterialChunks = new List<MaterialChunk>();
        public List<TransformNodeChunk> TransformNodeChunks = new List<TransformNodeChunk>();
        public List<GroupNodeChunk> GroupNodeChunks = new List<GroupNodeChunk>();
        public List<ShapeNodeChunk> ShapeNodeChunks = new List<ShapeNodeChunk>();
        public List<LayerChunk> LayerChunks = new List<LayerChunk>();
        public List<RendererSettingChunk> RendererSettingChunks = new List<RendererSettingChunk>();
    }

    public enum MaterialType
    {
        _diffuse, _metal, _glass, _emit
    }

    public class DICT
    { // DICT
        public KeyValue[] items;

        public DICT(KeyValue[] items)
        {
            this.items = items;
        }

        public Rotation _r
        {
            get
            {
                var item = items.FirstOrDefault(i => i.Key == "_r");
                if (item.Key == null)
                    return Rotation._PX_PY_P;
                byte result;
                if (!byte.TryParse(item.Value, out result))
                    return Rotation._PX_PY_P;
                return (Rotation)result;
            }
        }
        public Vector3 _t
        {
            get
            {
                var result = Vector3.zero;
                var item = items.FirstOrDefault(i => i.Key == "_t");
                if (item.Key == null)
                    return result;
                var data = item.Value.Split(' ');
                if (data.Length > 0)
                    float.TryParse(data[0], out result.X);
                if (data.Length > 1)
                    float.TryParse(data[1], out result.Y);
                if (data.Length > 2)
                    float.TryParse(data[2], out result.Z);
                return result;
            }
        }
    }

    

    [System.Flags]
    public enum Rotation : byte
    { // ROTATION
        _PX_PX_P, _PY_PX_P, _PZ_PX_P, _PW_PX_P,
        _PX_PY_P, _PY_PY_P, _PZ_PY_P, _PW_PY_P,
        _PX_PZ_P, _PY_PZ_P, _PZ_PZ_P, _PW_PZ_P,
        _PX_PW_P, _PY_PW_P, _PZ_PW_P, _PW_PW_P,
        _NX_PX_P, _NY_PX_P, _NZ_PX_P, _NW_PX_P,
        _NX_PY_P, _NY_PY_P, _NZ_PY_P, _NW_PY_P,
        _NX_PZ_P, _NY_PZ_P, _NZ_PZ_P, _NW_PZ_P,
        _NX_PW_P, _NY_PW_P, _NZ_PW_P, _NW_PW_P,
        _PX_NX_P, _PY_NX_P, _PZ_NX_P, _PW_NX_P,
        _PX_NY_P, _PY_NY_P, _PZ_NY_P, _PW_NY_P,
        _PX_NZ_P, _PY_NZ_P, _PZ_NZ_P, _PW_NZ_P,
        _PX_NW_P, _PY_NW_P, _PZ_NW_P, _PW_NW_P,
        _NX_NX_P, _NY_NX_P, _NZ_NX_P, _NW_NX_P,
        _NX_NY_P, _NY_NY_P, _NZ_NY_P, _NW_NY_P,
        _NX_NZ_P, _NY_NZ_P, _NZ_NZ_P, _NW_NZ_P,
        _NX_NW_P, _NY_NW_P, _NZ_NW_P, _NW_NW_P,

        _PX_PX_N, _PY_PX_N, _PZ_PX_N, _PW_PX_N,
        _PX_PY_N, _PY_PY_N, _PZ_PY_N, _PW_PY_N,
        _PX_PZ_N, _PY_PZ_N, _PZ_PZ_N, _PW_PZ_N,
        _PX_PW_N, _PY_PW_N, _PZ_PW_N, _PW_PW_N,
        _NX_PX_N, _NY_PX_N, _NZ_PX_N, _NW_PX_N,
        _NX_PY_N, _NY_PY_N, _NZ_PY_N, _NW_PY_N,
        _NX_PZ_N, _NY_PZ_N, _NZ_PZ_N, _NW_PZ_N,
        _NX_PW_N, _NY_PW_N, _NZ_PW_N, _NW_PW_N,
        _PX_NX_N, _PY_NX_N, _PZ_NX_N, _PW_NX_N,
        _PX_NY_N, _PY_NY_N, _PZ_NY_N, _PW_NY_N,
        _PX_NZ_N, _PY_NZ_N, _PZ_NZ_N, _PW_NZ_N,
        _PX_NW_N, _PY_NW_N, _PZ_NW_N, _PW_NW_N,
        _NX_NX_N, _NY_NX_N, _NZ_NX_N, _NW_NX_N,
        _NX_NY_N, _NY_NY_N, _NZ_NY_N, _NW_NY_N,
        _NX_NZ_N, _NY_NZ_N, _NZ_NZ_N, _NW_NZ_N,
        _NX_NW_N, _NY_NW_N, _NZ_NW_N, _NW_NW_N,
    }  

}
