using FileToVox.Schematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileToVox.Converter
{
    public abstract class BaseToSchematic
    {
        protected string _path;

        public BaseToSchematic(string path)
        {
            _path = path;
        }

        public abstract Schematic WriteSchematic();
    }
}
