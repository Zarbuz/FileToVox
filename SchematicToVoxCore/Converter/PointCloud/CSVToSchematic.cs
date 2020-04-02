using FileToVox.Schematics;
using FileToVox.Schematics.Tools;
using FileToVox.Utils;
using MoreLinq;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace FileToVox.Converter.PointCloud
{
	public class CSVToSchematic : PointCloudToSchematic
    {
        public CSVToSchematic(string path, float scale, int colorLimit, bool holes, bool flood) : base(path, scale, colorLimit, holes, flood)
        {
            List<Vector3> bodyVertices = new List<Vector3>();
            List<Color> bodyColors = new List<Color>();
            using (var reader = new StreamReader(_path))
            {
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    line = line.Replace(" ", "");
                    string[] data = line.Split(',');
                    if (data.Length > 14)
                    {
                        try
                        {
                            float[] values = new float[data.Length];
                            for (var i = 0; i < data.Length; i++)
                            {
                                string s = data[i];
                                values[i] = float.Parse(s, CultureInfo.InvariantCulture);
                            }

                            Vector3 vertice = new Vector3(values[11], values[12], values[13]);
                            bodyVertices.Add(vertice);
                            bodyColors.Add(Color.FromArgb((byte)Math.Round(values[7] * 255),
                                (byte)Math.Round(values[8] * 255),
                                (byte)Math.Round(values[9] * 255)));
                        }
                        catch (Exception e)
                        {
                        }
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
    }
}
