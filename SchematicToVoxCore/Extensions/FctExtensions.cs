using FileToVox.Schematics.Tools;
using System.Collections.Generic;
using System.Drawing;
using FileToVox.Schematics;

namespace SchematicToVoxCore.Extensions
{
    public static class FctExtensions
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
            return (uint)((a << 24) | (r << 16) | (g << 8) | (b << 0));
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

        public static List<Block> ApplyOffset(this List<Block> list, Vector3 vector)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = new Block((ushort)(list[i].X - vector.X), (ushort)(list[i].Y - vector.Y), (ushort)(list[i].Z - vector.Z), list[i].Color);
            }

            return list;
        }
    }
}
