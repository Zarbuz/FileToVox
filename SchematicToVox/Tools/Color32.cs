using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox.Tools
{
    public class Color32
    {
        private int rgba;

        public byte r;

        public byte g;

        public byte b;

        public byte a;

        public Color32(byte r, byte g, byte b, byte a)
        {
            rgba = 0; this.r = r; this.g = g; this.b = b; this.a = a;
        }

        //// Color32 can be implicitly converted to and from [[Color]].
        //public static implicit operator Color32(Color c)
        //{
        //    return new Color32((byte)(Mathf.Clamp01(c.r) * 255), (byte)(Mathf.Clamp01(c.g) * 255), (byte)(Mathf.Clamp01(c.b) * 255), (byte)(Mathf.Clamp01(c.a) * 255));
        //}

        // Color32 can be implicitly converted to and from [[Color]].
        public static implicit operator Color(Color32 c)
        {
            return new Color(c.r / 255f, c.g / 255f, c.b / 255f, c.a / 255f);
        }

        public override string ToString()
        {
            return string.Format("RGBA({0}, {1}, {2}, {3})", r, g, b, a);
        }

        public string ToString(string format)
        {
            return string.Format("RGBA({0}, {1}, {2}, {3})", r.ToString(format), g.ToString(format), b.ToString(format), a.ToString(format));
        }
    }
}
