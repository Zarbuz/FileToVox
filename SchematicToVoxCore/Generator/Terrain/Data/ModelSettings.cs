using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace FileToVox.Generator.Terrain.Data
{
	public class ModelSettings
	{
		public int SizeX { get; set; }
		public int SizeY { get; set; }
		public int SizeZ { get; set; }

		public int OffsetX { get; set; }
		public int OffsetY { get; set; }
		public int OffsetZ { get; set; }

		public ModelBit[] Bits { get; set; }
	}

	[Serializable]
	public struct ModelBit
	{
		public int VoxelIndex { get; set; }
		public Color Color { get; set; }
		public bool IsEmpty { get; set; }

	}
}
