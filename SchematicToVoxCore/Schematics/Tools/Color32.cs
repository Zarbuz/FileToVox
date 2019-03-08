using System;

namespace SchematicToVoxCore.Schematics.Tools
{
    public struct Color32 : IEquatable<Color32>
    {
        public byte R;

        public byte G;

        public byte B;

        public byte A;

        public Color32(byte r, byte g, byte b, byte a)
        {
            this.R = r; this.G = g; this.B = b; this.A = a;
        }

        // Color32 can be implicitly converted to and from [[Color]].
        public static implicit operator Color(Color32 c)
        {
            return new Color(c.R / 255f, c.G / 255f, c.B / 255f, c.A / 255f);
        }

        public static implicit operator Color32(System.Drawing.Color c)
        {
            return new Color32(c.R, c.G, c.B, c.A);
        }

        public static implicit operator System.Drawing.Color(Color32 c)
        {
            return System.Drawing.Color.FromArgb(c.A, c.R, c.G, c.B);
        }


        public override string ToString()
        {
            return string.Format("RGBA({0}, {1}, {2}, {3})", R, G, B, A);
        }

        public string ToString(string format)
        {
            return string.Format("RGBA({0}, {1}, {2}, {3})", R.ToString(format), G.ToString(format), B.ToString(format), A.ToString(format));
        }

        public bool Equals(Color32 other)
        {
            return R == other.R && G == other.G && B == other.B && A == other.A; 
        }
    }
}
