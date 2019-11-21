using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FileToVox.Schematics;

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

        protected void RemoveHoles(ref List<Block> list, Schematic schematic)
        {
            for (ushort y = 0; y < schematic.Heigth; y++)
            {
                for (ushort z = 0; z < schematic.Length; z++)
                {
                    for (ushort x = 0; x < schematic.Width; x++)
                    {
                        Block block = list.FirstOrDefault(t => t.X == x && t.Y == y && t.Z == z);
                        if (block.IsDefaultValue())
                        {
                            if (x > 0 && x < schematic.Width && z > 0 && z < schematic.Length)
                            {
                                Block left = list.FirstOrDefault(t => t.X == x - 1 && t.Y == y && t.Z == z);
                                Block right = list.FirstOrDefault(t => t.X == x + 1 && t.Y == y && t.Z == z);
                                Block top = list.FirstOrDefault(t => t.X == x && t.Y == y && t.Z == z - 1);
                                Block bottom = list.FirstOrDefault(t => t.X == x && t.Y == y && t.Z == z + 1);

                                if (!left.IsDefaultValue() && !right.IsDefaultValue() && !top.IsDefaultValue() &&
                                    !bottom.IsDefaultValue())
                                {
                                    list.Add(new Block(x, y, z, left.Color));
                                }
                            }
                        }
                    }
                }
            }

        }
    }
}
