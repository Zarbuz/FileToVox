using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Schematics
{
    public class RawSchematic
    {
        public short Width;
        public short Heigth;
        public short Length;
        public byte[] Blocks;
        public byte[] Data;
    }
}
