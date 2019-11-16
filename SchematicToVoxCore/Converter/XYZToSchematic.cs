using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using FileToVox.Extensions;
using FileToVox.Schematics;
using FileToVox.Schematics.Tools;
using FileToVox.Utils;
using MoreLinq;
using nQuant;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Converter
{
    public class XYZToSchematic : AbstractToSchematic
    {
        private readonly List<Block> _blocks = new List<Block>();

        public XYZToSchematic(string path, int scale) : base(path)
        {
            StreamReader file = new StreamReader(path);
            string line;

            List<Vector3> bodyVertices = new List<Vector3>();
            List<Color> bodyColors = new List<Color>();

            while ((line = file.ReadLine()) != null)
            {
                string[] data = line.Split(' ');
                if (data.Length < 6)
                {
                    Console.WriteLine("[ERROR] Line not well formated : " + line);
                }
                else
                {
                    try
                    {
                        float x = float.Parse(data[0], CultureInfo.InvariantCulture);
                        float y = float.Parse(data[1], CultureInfo.InvariantCulture);
                        float z = float.Parse(data[2], CultureInfo.InvariantCulture);
                        int r = int.Parse(data[3], CultureInfo.InvariantCulture);
                        int g = int.Parse(data[4], CultureInfo.InvariantCulture);
                        int b = int.Parse(data[5], CultureInfo.InvariantCulture);

                        bodyVertices.Add(new Vector3(x,y,z));
                        bodyColors.Add(Color.FromArgb(r,g,b));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[ERROR] Line not well formated : " + line + " " + e.Message);
                    }

                }
            }

            Vector3 minX = bodyVertices.MinBy(t => t.X);
            Vector3 minY = bodyVertices.MinBy(t => t.Y);
            Vector3 minZ = bodyVertices.MinBy(t => t.Z);

            float min = Math.Abs(Math.Min(minX.X, Math.Min(minY.Y, minZ.Z)));
            for (int i = 0; i < bodyVertices.Count; i++)
            {
                bodyVertices[i] += new Vector3(min, min, min);
                bodyVertices[i] = new Vector3((float)Math.Truncate(bodyVertices[i].X * scale), (float)Math.Truncate(bodyVertices[i].Y * scale), (float)Math.Truncate(bodyVertices[i].Z * scale));
            }

            HashSet<Vector3> set = new HashSet<Vector3>();
            List<Vector3> vertices = new List<Vector3>();
            List<Color> colors = new List<Color>();

            using (ProgressBar progressbar = new ProgressBar())
            {
                for (int i = 0; i < bodyVertices.Count; i++)
                {
                    if (!set.Contains(bodyVertices[i]))
                    {
                        set.Add(bodyVertices[i]);
                        vertices.Add(bodyVertices[i]);
                        colors.Add(bodyColors[i]);
                    }
                    progressbar.Report(i / (float)bodyVertices.Count);
                }
            }

            minX = vertices.MinBy(t => t.X);
            minY = vertices.MinBy(t => t.Y);
            minZ = vertices.MinBy(t => t.Z);

            min = Math.Min(minX.X, Math.Min(minY.Y, minZ.Z));
            for (int i = 0; i < vertices.Count; i++)
            {
                float max = Math.Max(vertices[i].X, Math.Max(vertices[i].Y, vertices[i].Z));
                if (max - min < 2016 && max - min >= 0)
                {
                    vertices[i] -= new Vector3(min, min, min);
                    _blocks.Add(new Block((ushort)vertices[i].X, (ushort)vertices[i].Y, (ushort)vertices[i].Z, colors[i].ColorToUInt()));
                }
            }


        }

        public override Schematic WriteSchematic()
        {
            Block minX = _blocks.MinBy(t => t.X);
            Block minY = _blocks.MinBy(t => t.Y);
            Block minZ = _blocks.MinBy(t => t.Z);

            Schematic schematic = new Schematic()
            {
                Length = (ushort)(_blocks.MaxBy(t => t.Z).Z - minZ.Z),
                Width = (ushort)(_blocks.MaxBy(t => t.X).X - minX.X),
                Heigth = (ushort)(_blocks.MaxBy(t => t.Y).Y - minY.Y),
                Blocks = new HashSet<Block>()
            };

            LoadedSchematic.LengthSchematic = schematic.Length;
            LoadedSchematic.WidthSchematic = schematic.Width;
            LoadedSchematic.HeightSchematic = schematic.Heigth;
            List<Block> list = Quantization.ApplyQuantization(_blocks);
            list.ApplyOffset(new Vector3(minX.X, minY.Y, minZ.Z));
            foreach (Block t in list)
            {
                schematic.Blocks.Add(t);
            }

            return schematic;
        }

        
    }
}
