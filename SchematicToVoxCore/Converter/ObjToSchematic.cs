using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FileToVox.Schematics;
using FileToVox.Utils;
using g3;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Converter
{
    public class OBJToSchematic : AbstractToSchematic
    {
        private readonly int _gridSize;
        private readonly bool _excavate;
        private readonly float _winding_number;

        public OBJToSchematic(string path, int gridSize, bool excavate, float winding_number) : base(path)
        {
            _gridSize = gridSize;
            _excavate = excavate;
            _winding_number = winding_number;
        }

        public override Schematic WriteSchematic()
        {
            DMesh3 mesh = StandardMeshReader.ReadMesh(_path);
            AxisAlignedBox3d bounds = mesh.CachedBounds;

            DMeshAABBTree3 spatial = new DMeshAABBTree3(mesh, autoBuild: true);
            double cellsize = mesh.CachedBounds.MaxDim / _gridSize;
            ShiftGridIndexer3 indexer = new ShiftGridIndexer3(bounds.Min, cellsize);

            MeshSignedDistanceGrid sdf = new MeshSignedDistanceGrid(mesh, cellsize);
            sdf.Compute();

            Bitmap3 bmp = new Bitmap3(sdf.Dimensions);

            Schematic schematic = new Schematic()
            {
                Blocks = new HashSet<Block>(),
                Width = (short)bmp.Dimensions.x,
                Heigth = (short)bmp.Dimensions.y,
                Length = (short)bmp.Dimensions.z
            };

            LoadedSchematic.WidthSchematic = schematic.Width;
            LoadedSchematic.HeightSchematic = schematic.Heigth;
            LoadedSchematic.LengthSchematic = schematic.Length;

            if (_winding_number != 0)
            {
                spatial.WindingNumber(Vector3d.Zero);  // seed cache outside of parallel eval

                using (ProgressBar progressbar = new ProgressBar())
                {
                    List<Vector3i> list = bmp.Indices().ToList();
                    int count = 0;
                    gParallel.ForEach(bmp.Indices(), (idx) =>
                    {
                        Vector3d v = indexer.FromGrid(idx);
                        bmp.SafeSet(idx, spatial.WindingNumber(v) > _winding_number);
                        count++;
                        progressbar.Report(count / (float)list.Count);
                    });
                }
                if (!_excavate)
                {
                    foreach (Vector3i idx in bmp.Indices())
                    {
                        if (bmp.Get(idx))
                            schematic.Blocks.Add(new Block((short)idx.x, (short)idx.y, (short)idx.z, Color.White.ColorToUInt()));
                    }
                }
            }
            else
            {
                using (ProgressBar progressbar = new ProgressBar())
                {
                    int count = bmp.Indices().Count();
                    List<Vector3i> list = bmp.Indices().ToList();
                    for (int i = 0; i < count; i++)
                    {
                        Vector3i idx = list[i];
                        float f = sdf[idx.x, idx.y, idx.z];
                        bool isInside = f < 0;
                        bmp.Set(idx, (f < 0) ? true : false);

                        if (!_excavate && isInside)
                        {
                            schematic.Blocks.Add(new Block((short)idx.x, (short)idx.y, (short)idx.z, Color.White.ColorToUInt()));
                        }
                        progressbar.Report((i / (float)count));
                    }
                }
            }



            if (_excavate)
            {
                foreach (Vector3i idx in bmp.Indices())
                {
                    if (bmp.Get(idx) && IsBlockConnectedToAir(bmp, idx))
                        schematic.Blocks.Add(new Block((short)idx.x, (short)idx.y, (short)idx.z, Color.White.ColorToUInt()));
                }
            }

            return schematic;
        }


        private bool IsBlockConnectedToAir(Bitmap3 bmp, Vector3i idx)
        {
            if (idx.x - 1 >= 0 && idx.x + 1 < bmp.Dimensions.x && idx.y - 1 >= 0 && idx.y + 1 < bmp.Dimensions.y && idx.z - 1 >= 0 && idx.z + 1 < bmp.Dimensions.z)
            {
                return (!bmp.Get(new Vector3i(idx.x - 1, idx.y, idx.z)) || !bmp.Get(new Vector3i(idx.x, idx.y + 1, idx.z)) || !bmp.Get(new Vector3i(idx.x + 1, idx.y, idx.z))
                    || !bmp.Get(new Vector3i(idx.x, idx.y - 1, idx.z)) || !bmp.Get(new Vector3i(idx.x, idx.y, idx.z + 1)) || !bmp.Get(new Vector3i(idx.x, idx.y, idx.z - 1)));
            }
            return false;
        }
    }
}
