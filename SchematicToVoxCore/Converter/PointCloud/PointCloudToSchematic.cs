using System;
using System.Linq;
using FileToVox.Schematics;
using System.Collections.Generic;
using System.Threading.Tasks;
using FileToVox.Utils;

namespace FileToVox.Converter.PointCloud
{
    public abstract class PointCloudToSchematic : AbstractToSchematic
    {
        protected readonly List<Block> _blocks = new List<Block>();
        protected readonly int _scale;
        protected readonly int _colorLimit;
        protected PointCloudToSchematic(string path, int scale, int colorLimit) : base(path)
        {
            _scale = scale;
            _colorLimit = colorLimit;
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
                            if (block.IsDefaultValue() && x > 0 && x < schematic.Width && z > 0 && z < schematic.Length)
                            {
                                hashSet.TryGetValue(new Block((ushort)(x - 1), y, z, 0), out Block left);
                                hashSet.TryGetValue(new Block((ushort)(x + 1), y, z, 0), out Block right);
                                hashSet.TryGetValue(new Block(x, y, (ushort)(z - 1), 0), out Block top);
                                hashSet.TryGetValue(new Block(x, y, (ushort)(z + 1), 0), out Block bottom);

                                hashSet.TryGetValue(new Block((ushort)(x+1), y, (ushort)(z+1), 0), out Block diagonal);

                                List<Block> blocks = new List<Block>();
                                blocks.AddRange(new[] { left, right, top, bottom });
                                if (blocks.Count(t => !t.IsDefaultValue()) >= 3)
                                {
                                    hashSet.Add(new Block(x, y, z, blocks.First(t => !t.IsDefaultValue()).Color));
                                    x = 0;
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
