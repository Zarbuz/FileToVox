using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Schematics
{
    internal class RawSchematic
    {
        public short Width;
        public short Heigth;
        public short Length;
        public string Materials; //ignored later
        public byte[] Blocks;
        public byte[] Data;

    }
}
