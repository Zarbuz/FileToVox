using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using FileToVoxCore.Schematics.Tools;

namespace FileToVox.Converter.PointCloud
{
	public class BodyDataDTO
	{
		public List<Vector3> BodyVertices { get; set; }
		public List<Color> BodyColors { get; set; }
	}
}
