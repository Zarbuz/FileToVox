using FileToVox.Schematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileToVox.Converter
{
    public abstract class AbstractToSchematic
    {
        protected string _path;

        public AbstractToSchematic(string path)
        {
            _path = path;
        }

        public abstract Schematic WriteSchematic();
    }
}
