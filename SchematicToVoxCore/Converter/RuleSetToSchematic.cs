using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using FileToVox.CA;
using FileToVox.Schematics;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Converter
{
    public class RuleSetToSchematic : BaseToSchematic
    {
        private RuleSet _ruleSet;
        private int _lifeTime;

        public RuleSetToSchematic(string path, RuleSet ruleSet, int lifeTime) : base(path)
        {
            _ruleSet = ruleSet;
            _lifeTime = lifeTime;
        }

        public override Schematic WriteSchematic()
        {
            for  (int i = 0; i < _lifeTime; i++)
            {
                _ruleSet.Tick();
            }

            Schematic schematic = new Schematic()
            {
                Width = (short)_ruleSet.GetWidth(),
                Heigth = (short)_ruleSet.GetHeight(),
                Length = (short)_ruleSet.GetLength(),
                Blocks = new HashSet<Block>()
            };

            LoadedSchematic.HeightSchematic = schematic.Heigth;
            LoadedSchematic.LengthSchematic = schematic.Length;
            LoadedSchematic.WidthSchematic = schematic.Width;

            for (int y = 0; y < _ruleSet.GetHeight(); y++)
            {
                for (int z = 0; z < _ruleSet.GetLength(); z++)
                {
                    for (int x = 0; x < _ruleSet.GetWidth(); x++)
                    {
                        if (_ruleSet.GetField()[x, y, z] == 1)
                        {
                            schematic.Blocks.Add(new Block((short)x, (short)y, (short)z, Color.White.ColorToUInt()));
                        }
                    }
                }
            }

            return schematic;
        }
    }
}
