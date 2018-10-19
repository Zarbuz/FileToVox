using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicReader
{
    public class Schematic
    {
        public string Name { get; set; }
        public short Width { get; set; }
        public short Heigth { get; set; }
        public short Length { get; set; }
        /// <summary>Contains all usual blocks</summary>
        public List<HashSet<Block>> Blocks { get; set; }
        /// <summary>Contains TileEntities such as hoppers and chests</summary>
        public List<TileEntity> TileEntities { get; set; }
        /// <summary>Returns how much blocks and tile entities there are in total.</summary>
        public int TotalCount { get { return Blocks.Count + TileEntities.Count; } }

        public Schematic()
        {
            Blocks = new List<HashSet<Block>>();
            TileEntities = new List<TileEntity>();
        }

        public Schematic(string name) : this()
        {
            Name = name;
        }

        public Schematic(string name, short width, short heigth, short length) : this(name)
        {
            this.Width = width;
            this.Heigth = heigth;
            this.Length = length;
        }

        public Schematic(string name, short width, short heigth, short length, List<HashSet<Block>> blocks, List<TileEntity> tileEntities) : this(name, width, heigth, length)
        {
            this.Blocks = blocks;
            this.TileEntities = tileEntities;
        }

        /// <summary>Returns all signs from the TileEntities.</summary>
        public List<Sign> GetSigns()
        {
            List<Sign> signs = new List<Sign>(20); //20 is general enough and faster than 0 in the most cases
            foreach (TileEntity item in this.TileEntities)
            {
                Sign sign = item as Sign;
                if (sign != null)
                {
                    signs.Add(sign);
                }
            }
            return signs;
        }
    }
}
