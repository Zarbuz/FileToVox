using FileToVox.Extensions;
using FileToVox.Schematics;
using FileToVox.Schematics.Tools;
using FileToVox.Utils;
using MoreLinq;
using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FileToVox.Converter.PointCloud
{
	public abstract class PointCloudToSchematic : AbstractToSchematic
	{
		protected readonly List<Voxel> _blocks = new List<Voxel>();
		protected readonly float _scale;
		protected readonly int _colorLimit;
		protected PointCloudToSchematic(string path, float scale, int colorLimit) : base(path)
		{
			_scale = scale;
			_colorLimit = colorLimit;
		}

		protected abstract BodyDataDTO ReadContentFile();

		protected void VoxelizeData(BodyDataDTO data)
		{
			Vector3 minX = data.BodyVertices.MinBy(t => t.X);
			Vector3 minY = data.BodyVertices.MinBy(t => t.Y);
			Vector3 minZ = data.BodyVertices.MinBy(t => t.Z);

			float min = Math.Abs(Math.Min(minX.X, Math.Min(minY.Y, minZ.Z)));
			for (int i = 0; i < data.BodyVertices.Count; i++)
			{
				data.BodyVertices[i] += new Vector3(min, min, min);
				data.BodyVertices[i] = new Vector3((float)Math.Truncate(data.BodyVertices[i].X * _scale), (float)Math.Truncate(data.BodyVertices[i].Y * _scale), (float)Math.Truncate(data.BodyVertices[i].Z * _scale));
			}

			HashSet<Vector3> set = new HashSet<Vector3>();
			List<Vector3> vertices = new List<Vector3>();
			List<Color> colors = new List<Color>();

			Console.WriteLine("[LOG] Started to voxelize data...");
			using (ProgressBar progressbar = new ProgressBar())
			{
				for (int i = 0; i < data.BodyVertices.Count; i++)
				{
					if (!set.Contains(data.BodyVertices[i]))
					{
						set.Add(data.BodyVertices[i]);
						vertices.Add(data.BodyVertices[i]);
						colors.Add(data.BodyColors[i]);
					}
					progressbar.Report(i / (float)data.BodyVertices.Count);
				}
			}
			Console.WriteLine("[LOG] Done.");

			minX = vertices.MinBy(t => t.X);
			minY = vertices.MinBy(t => t.Y);
			minZ = vertices.MinBy(t => t.Z);

			min = Math.Min(minX.X, Math.Min(minY.Y, minZ.Z));
			for (int i = 0; i < vertices.Count; i++)
			{
				float max = Math.Max(vertices[i].X, Math.Max(vertices[i].Y, vertices[i].Z));
				if (/*max - min < 8000 && */max - min >= 0)
				{
					vertices[i] -= new Vector3(min, min, min);
					_blocks.Add(new Voxel((ushort)vertices[i].X, (ushort)vertices[i].Y, (ushort)vertices[i].Z, colors[i].ColorToUInt()));
				}
			}
		}
		

		public override Schematic WriteSchematic()
		{
			float minX = _blocks.MinBy(t => t.X).X;
			float minY = _blocks.MinBy(t => t.Y).Y;
			float minZ = _blocks.MinBy(t => t.Z).Z;

			List<Voxel> list = Quantization.ApplyQuantization(_blocks, _colorLimit);
			list.ApplyOffset(new Vector3(minX, minY, minZ));

			Schematic schematic = new Schematic(list.ToVoxelDictionary());
			return schematic;
		}

	
	}
}
