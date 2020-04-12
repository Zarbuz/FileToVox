using FileToVox.Schematics.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace FileToVox.Converter.PointCloud
{
	public class XYZToSchematic : PointCloudToSchematic
    {
        public XYZToSchematic(string path, float scale, int colorLimit, bool holes, bool flood, bool lonely) : base(path, scale, colorLimit, holes, flood, lonely)
        {
			BodyDataDTO data = ReadContentFile();
			VoxelizeData(data);
        }

        protected sealed override BodyDataDTO ReadContentFile()
        {
			BodyDataDTO dataFile = new BodyDataDTO();
			StreamReader file = new StreamReader(_path);
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

						bodyVertices.Add(new Vector3(x, y, z));
						bodyColors.Add(Color.FromArgb(r, g, b));
					}
					catch (Exception e)
					{
						Console.WriteLine("[ERROR] Line not well formated : " + line + " " + e.Message);
					}

				}
			}

			dataFile.BodyVertices = bodyVertices;
			dataFile.BodyColors = bodyColors;
			return dataFile;
        }
    }
}
