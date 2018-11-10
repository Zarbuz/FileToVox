using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SchematicReader.Tools
{
    public class Color32 : IEquatable<Color32>
    {
        public byte r;

        public byte g;

        public byte b;

        public byte a;

        public Color32(byte r, byte g, byte b, byte a)
        {
            this.r = r; this.g = g; this.b = b; this.a = a;
        }

        // Color32 can be implicitly converted to and from [[Color]].
        public static implicit operator Color(Color32 c)
        {
            return new Color(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f);
        }

        public static implicit operator Color32(System.Drawing.Color c)
        {
            return new Color32(c.R, c.G, c.B, c.A);
        }

        public static implicit operator System.Drawing.Color(Color32 c)
        {
            return System.Drawing.Color.FromArgb(c.a, c.r, c.g, c.b);
        }


        public override string ToString()
        {
            return string.Format("RGBA({0}, {1}, {2}, {3})", r, g, b, a);
        }

        public string ToString(string format)
        {
            return string.Format("RGBA({0}, {1}, {2}, {3})", r.ToString(format), g.ToString(format), b.ToString(format), a.ToString(format));
        }

        public bool Equals(Color32 other)
        {
            return r == other.r && g == other.g && b == other.b && a == other.a; 
        }
    }
}
