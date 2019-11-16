using System;
using System.Collections.Generic;
using System.Text;
using FileToVox.Schematics;

namespace FileToVox.Converter
{
    public class XYZToSchematic : AbstractToSchematic
    {
        public XYZToSchematic(string path, int scale) : base(path)
        {

        }

        public override Schematic WriteSchematic()
        {
            throw new NotImplementedException();
        }
    }
}
