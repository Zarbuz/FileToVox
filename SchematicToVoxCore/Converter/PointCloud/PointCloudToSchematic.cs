using System;
using FileToVox.Schematics;
using System.Collections.Generic;
using FileToVox.Utils;

namespace FileToVox.Converter.PointCloud
{
    public abstract class PointCloudToSchematic : AbstractToSchematic
    {
        protected readonly List<Block> _blocks = new List<Block>();
        protected readonly int _scale;

        protected PointCloudToSchematic(string path, int scale) : base(path)
        {
            _scale = scale;
        }

        protected void RemoveHoles(ref HashSet<Block> hashSet, Schematic schematic)
        {
            Console.WriteLine("[LOG] Start to fill holes ...");
            using (ProgressBar progressBar = new ProgressBar())
            {
                for (ushort y = 0; y < schematic.Heigth; y++)
                {
                    for (ushort z = 0; z < schematic.Length; z++)
                    {
                        for (ushort x = 0; x < schematic.Width; x++)
                        {
                            hashSet.TryGetValue(new Block(x, y, z, 0), out Block block);
                            if (block.IsDefaultValue())
                            {
                                if (x > 0 && x < schematic.Width && z > 0 && z < schematic.Length)
                                {
                                    hashSet.TryGetValue(new Block((ushort) (x - 1), y, z, 0), out Block left);
                                    hashSet.TryGetValue(new Block((ushort) (x + 1), y, z, 0), out Block right);
                                    hashSet.TryGetValue(new Block(x, y, (ushort) (z - 1), 0), out Block top);
                                    hashSet.TryGetValue(new Block(x, y, (ushort) (z + 1), 0), out Block bottom);

                                    if (!left.IsDefaultValue() && !right.IsDefaultValue() && !top.IsDefaultValue() &&
                                        !bottom.IsDefaultValue())
                                    {
                                        hashSet.Add(new Block(x, y, z, left.Color));
                                    }
                                }
                            }
                        }
                    }
                    progressBar.Report(y / (float)schematic.Heigth);
                }
            }
            Console.WriteLine("[LOG] Done.");
        }
    }
}
