using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicReader
{
    public class Block : IEquatable<Block>
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int BlockID { get; set; }
        public int Data { get; set; }
        public int ID { get; set; }
        /// <summary>Returns ItemID:SubID</summary>
        public string ItemID { get { return BlockID + ":" + Data; } }

        public Block()
        {
            X = 0;
            Y = 0;
            Z = 0;
            BlockID = 0;
            ID = -1;
        }

        public Block(int x, int y, int z, int blockID, int data, int iD)
        {
            X = x;
            Y = y;
            Z = z;
            BlockID = blockID;
            Data = data;
            ID = iD;
        }

        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
        public bool Equals(Block other)
        {
            return this.ID.Equals(other.ID);
        }

        public override string ToString()
        {
            return string.Format("ID: {3}:{4}, X: {0}, Y: {1}, Z: {2}", X, Y, Z, BlockID, Data);
        }
    }
}
