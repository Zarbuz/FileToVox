using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicReader
{
    public class TileEntity
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public string ID { get; set; }

        public TileEntity(int x, int y, int z, string iD)
        {
            X = x;
            Y = y;
            Z = z;
            ID = iD;
        }

        public override string ToString()
        {
            return string.Format("ID: {3}, X: {0}, Y: {1}, Z: {2}", X, Y, Z, ID);
        }
    }
}
