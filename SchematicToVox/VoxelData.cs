using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox
{
    public class VoxelData
    {
        private int _voxelWide, _voxelsTall, _voxelsDeep;
        private byte[] colors;

        /// <summary>
        /// Creates a voxeldata with provided dimensions
        /// </summary>
        /// <param name="voxelWide"></param>
        /// <param name="voxelsTall"></param>
        /// <param name="voxelsDeep"></param>
        public VoxelData(int voxelWide, int voxelsTall, int voxelsDeep)
        {
            Resize(voxelWide, voxelsTall, voxelsDeep);
        }

        public VoxelData()
        {
        }

        public void Resize(int voxelsWide, int voxelsTall, int voxelsDeep)
        {
            _voxelWide = voxelsWide;
            _voxelsTall = voxelsTall;
            _voxelsDeep = voxelsDeep;
            colors = new byte[_voxelWide * _voxelsTall * _voxelsDeep];
        }

        /// <summary>
        /// Gets a grid position from voxel coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public int GetGridPos(int x, int y, int z)
            => (_voxelWide * _voxelsTall) * z + (_voxelWide * y) + x;

        /// <summary>
        /// Set a color index from voxel coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Set(int x, int y, int z, byte value)
            => colors[GetGridPos(x, y, z)] = value;

        /// <summary>
        /// Sets a colors index from grid position
        /// </summary>
        /// <param name="x"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public int Set(int x, byte value)
            => colors[x] = value;

        /// <summary>
        /// Gets a palette index from voxel coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public int Get(int x, int y, int z)
            => colors[GetGridPos(x, y, z)];

        /// <summary>
        /// Gets a palette index from a grid position
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public byte Get(int x) => colors[x];

        /// <summary>
        /// Width of the data in voxels
        /// </summary>
        public int VoxelsWide => _voxelWide;

        /// <summary>
        /// Height of the data in voxels
        /// </summary>
        public int VoxelsTall => _voxelsTall;

        /// <summary>
        /// Depth of the data in voxels
        /// </summary>
        public int VoxelsDeep => _voxelsDeep;

        public byte[] Colors => colors;

        public bool Contains(int x, int y, int z)
            => x >= 0 && y >= 0 && z >= 0 && x < _voxelWide && y < _voxelsTall && z < _voxelsDeep;

        public byte GetSafe(int x, int y, int z)
            => Contains(x, y, z) ? colors[GetGridPos(x, y, z)] : (byte)0;

        public VoxelData ToSmaller()
        {
            var work = new byte[8];
            var result = new VoxelData(
                (_voxelWide + 1) >> 1,
                (_voxelsTall + 1) >> 1,
                (_voxelsDeep + 1) >> 1);
            int i = 0;
            for (int z = 0; z < _voxelsDeep; z += 2)
            {
                int z1 = z + 1;
                for (int y = 0; y < _voxelsTall; y += 2)
                {
                    int y1 = y + 1;
                    for (int x = 0; x < _voxelWide; x += 2)
                    {
                        int x1 = x + 1;
                        work[0] = GetSafe(x, y, z);
                        work[1] = GetSafe(x1, y, z);
                        work[2] = GetSafe(x, y1, z);
                        work[3] = GetSafe(x1, y1, z);
                        work[4] = GetSafe(x, y, z1);
                        work[5] = GetSafe(x1, y1, z1);
                        work[6] = GetSafe(x, y1, z1);
                        work[7] = GetSafe(x1, y1, z1);

                        if (work.Any(color => color != 0))
                        {
                            var groups = work.Where(color => color != 0).GroupBy(v => v).OrderByDescending(v => v.Count());
                            var count = groups.ElementAt(0).Count();
                            var group = groups.TakeWhile(v => v.Count() == count)
                                .OrderByDescending(v => v.Key).First();
                            result.colors[i++] = group.Key;
                        }
                        else
                        {
                            result.colors[i++] = 0;
                        }
                    }
                }
            }
            return result;
        }
    }
}
