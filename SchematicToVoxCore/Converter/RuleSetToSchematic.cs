using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using FileToVox.CA;
using FileToVox.Schematics;
using FileToVox.Utils;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Converter
{
    public class RuleSetToSchematic : BaseToSchematic
    {
        private readonly RuleSet _ruleSet;
        private readonly int _lifeTime;

        public RuleSetToSchematic(string path, RuleSet ruleSet, int lifeTime) : base(path)
        {
            _ruleSet = ruleSet;
            _lifeTime = lifeTime;

            Console.WriteLine("[INFO] Schematic Width: " + _ruleSet.GetWidth());
            Console.WriteLine("[INFO] Schematic Height: " + _ruleSet.GetHeight());
            Console.WriteLine("[INFO] Schematic Length: " + _ruleSet.GetLength());
            Console.WriteLine("[INFO] Lifetime: " + lifeTime);
        }

        public override Schematic WriteSchematic()
        {
            Console.WriteLine("[LOG] Started to launch ticks");
            using (ProgressBar progress = new ProgressBar())
            {
                for (int i = 0; i < _lifeTime; i++)
                {
                    _ruleSet.Tick();
                    progress.Report(i / (float)_lifeTime);
                }
            }
            Console.WriteLine("[LOG] Done.");

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
