using FileToVox.Schematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileToVox.Converter
{
    public abstract class BaseToSchematic
    {
        public abstract Schematic WriteSchematic(string path);
    }
}
