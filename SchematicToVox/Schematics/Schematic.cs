using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Schematics
{
    public class Schematic
    {
        public short Width { get; set; }
        public short Heigth { get; set; }
        public short Length { get; set; }
        /// <summary>Contains all usual blocks</summary>
        public List<HashSet<Block>> Blocks { get; set; }
        /// <summary>Returns how much blocks and tile entities there are in total.</summary>
        public int TotalCount { get { return Blocks.Count; } }

        public Schematic()
        {
            Blocks = new List<HashSet<Block>>();
        }

        public Schematic(string name, short width, short heigth, short length) : this(name)
        {
            this.Width = width;
            this.Heigth = heigth;
            this.Length = length;
        }

        public Schematic(string name, short width, short heigth, short length, List<HashSet<Block>> blocks) : this(name, width, heigth, length)
        {
            this.Blocks = blocks;
        }
       
        
    }
}
