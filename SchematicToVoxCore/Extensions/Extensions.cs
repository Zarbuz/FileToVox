using System;
using System.Collections.Generic;
using System.Drawing;

namespace SchematicToVoxCore.Extensions
{
    public static class Extensions
    {
        public static int GetColorIntensity(this Color color)
        {
            return color.R + color.G + color.B;
        }

        public static uint ColorToUInt(this Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) |
                          (color.G << 8) | (color.B << 0));
        }

        public static uint ByteArrayToUInt(byte r, byte g, byte b, byte a)
        {
            return (uint) ((a << 24) | (r << 16) | (g << 8) | (b << 0));
        }

        public static Color UIntToColor(this uint color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
            => new HashSet<T>(source, comparer);

        
    }
}
