using SchematicToVoxCore.Schematics;
using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using SchematicToVoxCore.Extensions;

namespace SchematicToVoxCore.Converter
{
    public static class ASCToSchematic
    {
        public static Schematic WriteSchematic(string path)
        {
            return WriteSchematicFromASC(path);
        }

        private static Schematic WriteSchematicFromASC(string path)
        {
            int nCols = 0;
            int nRows = 0;
            int cellSize = 0;
            int xllcorner = 0;
            int yllcorner = 0;
            int nodata = -9999;
            float[,] points = new float[0,0]; //rows, cols

            using (StreamReader file = new StreamReader(path))
            {
                string line;
                int row = 0;
                while ((line = file.ReadLine()) != null)
                {
                    string[] data = line.Split(" ").Where(d => !string.IsNullOrEmpty(d)).ToArray();
                    switch (data[0])
                    {
                        case "ncols":
                            nCols = Convert.ToInt32(data[1]);
                            points = new float[nRows, nCols];
                            break;
                        case "nrows":
                            nRows = Convert.ToInt32(data[1]);
                            points = new float[nRows, nCols];
                            break;
                        case "xllcorner":
                            xllcorner = Convert.ToInt32(data[1]);
                            break;
                        case "yllcorner":
                            yllcorner = Convert.ToInt32(data[1]);
                            break;
                        case "cellsize":
                            cellSize = Convert.ToInt32(data[1]);
                            break;
                        case "NODATA_value":
                            nodata = Convert.ToInt32(data[1]);
                            break;
                        default:
                            ProcessLine(data, row++, ref points);
                            break;
                    }
                }
            }

            Schematic schematic = new Schematic()
            {
                Length = (short) nRows,
                Width = (short) nCols,
                Heigth = (short) points.Cast<float>().Max()
            };

            SchematicReader.WidthSchematic = schematic.Width;
            SchematicReader.HeightSchematic = schematic.Heigth;
            SchematicReader.LengthSchematic = schematic.Length;

            Console.WriteLine(points.GetLength(0));
            Console.WriteLine(points.GetLength(1));

            for (int i = 0; i < points.GetLength(0); i++)
            {
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    schematic.Blocks.Add(new Block((ushort) i, (ushort) points[i, j], (ushort) j, Color.White.ColorToUInt()));
                }
            }

            return schematic;
        }

        private static void ProcessLine(string[] data, int row, ref float[,] points)
        {
            for (int i = 0; i < data.Length; i++)
            {
                points[row, i] = float.Parse(data[i], CultureInfo.InvariantCulture.NumberFormat);
            }

        }
    }
}
