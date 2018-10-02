using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicReader
{
    public class Sign : TileEntity
    {
        public string[] Text { get; set; }

        public Sign(int x, int y, int z, string text1, string text2, string text3, string text4) : base(x, y, z, "Sign")
        {
            Text = new string[4];
            Text[0] = text1;
            Text[1] = text2;
            Text[2] = text3;
            Text[3] = text4;
        }
    }
}
