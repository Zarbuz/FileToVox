using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace FileToVox.Quantizer
{
    public class QuantizedPalette
    {
        public QuantizedPalette(int size)
        {
            this.Colors = (IList<Color>)new List<Color>();
            this.PixelIndex = new int[size];
        }

        public IList<Color> Colors { get; private set; }

        public int[] PixelIndex { get; private set; }
    }
}
