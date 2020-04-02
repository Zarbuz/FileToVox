using System;
using FileToVox.Schematics.Tools;
using System.Collections.Generic;
using System.Drawing;
using FileToVox.Schematics;
using Motvin.Collections;

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

        public static FastHashSet<T> ToHashSetFast<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer = null)
            => new FastHashSet<T>(source, comparer);

        public static List<Block> ApplyOffset(this List<Block> list, Vector3 vector)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = new Block((ushort)(list[i].X - vector.X), (ushort)(list[i].Y - vector.Y), (ushort)(list[i].Z - vector.Z), list[i].Color);
            }

            return list;
        }

        public static uint[,,] To3DArray(this HashSet<Block> source, Schematic schematic)
        {
            uint[,,] blocks = new uint[schematic.Width + 1, schematic.Heigth + 1, schematic.Length + 1];

            foreach (Block block in source)
            {
                blocks[block.X, block.Y, block.Z] = block.Color;
            }

            return blocks;
        }

        public static HashSet<Block> ToHashSetFrom3DArray(this uint[,,] source)
        {
            HashSet<Block> blocks = new HashSet<Block>();

            for (int y = 0; y < source.GetLength(1); y++)
            {
                for (int z = 0; z < source.GetLength(2); z++)
                {
                    for (int x = 0; x < source.GetLength(0); x++)
                    {
                        if (source[x, y, z] != 0)
                        {
                            blocks.Add(new Block((ushort)x, (ushort)y, (ushort)z, source[x, y, z]));
                        }
                    }
                }
            }

            return blocks;
        }
    }
}
