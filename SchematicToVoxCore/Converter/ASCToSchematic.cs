using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using FileToVox.Schematics;
using SchematicToVoxCore.Extensions;

namespace FileToVox.Converter
{
    public class ASCToSchematic : AbstractToSchematic
    {
        public ASCToSchematic(string path) : base(path)
        {

        }
        public override Schematic WriteSchematic()
        {
            return WriteSchematicFromASC();
        }

        private Schematic WriteSchematicFromASC()
        {
            int nCols = 0;
            int nRows = 0;
            int cellSize = 0;
            int xllcorner = 0;
            int yllcorner = 0;
            int nodata = -9999;
            float[,] points = new float[0,0]; //rows, cols

            using (StreamReader file = new StreamReader(Path))
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

            Schematic schematic = new Schematic();

            for (int i = 0; i < points.GetLength(0); i++)
            {
                for (int j = 0; j < points.GetLength(1); j++)
                {
                    if (points[i, j] != nodata)
                    {
                        schematic.AddVoxel(i, (int)points[i,j], j, Color.White);
                    }
                }
            }

            return schematic;
        }

        private void ProcessLine(string[] data, int row, ref float[,] points)
        {
            for (int i = 0; i < data.Length; i++)
            {
                points[row, i] = float.Parse(data[i], CultureInfo.InvariantCulture.NumberFormat);
            }

        }
    }
}
