using FileToVox.Extensions;
using FileToVoxCore.Schematics;
using FileToVoxCore.Schematics.Tools;
using FileToVoxCore.Utils;
using MoreLinq;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace FileToVox.Converter.PointCloud
{
	public abstract class PointCloudToSchematic : AbstractToSchematic
	{
		protected readonly float Scale;
		protected readonly int ColorLimit;
		private Schematic mSchematic;
		protected PointCloudToSchematic(string path, float scale, int colorLimit) : base(path)
		{
			Scale = scale;
			ColorLimit = colorLimit;
		}

		protected abstract BodyDataDTO ReadContentFile();

		protected void VoxelizeData(BodyDataDTO data)
		{
			mSchematic = new Schematic();

			if (data.BodyVertices.Count == 0)
			{
				return;
			}

			Vector3 minX = data.BodyVertices.MinBy(t => t.X);
			Vector3 minY = data.BodyVertices.MinBy(t => t.Y);
			Vector3 minZ = data.BodyVertices.MinBy(t => t.Z);

			float min = Math.Abs(Math.Min(minX.X, Math.Min(minY.Y, minZ.Z)));

			for (int i = 0; i < data.BodyVertices.Count; i++)
			{
				data.BodyVertices[i] += new Vector3(min, min, min);
				//data.BodyVertices[i] = new Vector3((float)Math.Truncate(data.BodyVertices[i].X * Scale), (float)Math.Truncate(data.BodyVertices[i].Y * Scale), (float)Math.Truncate(data.BodyVertices[i].Z * Scale));
			}

			Vector3 maxX = data.BodyVertices.MaxBy(t => t.X);
			Vector3 maxY = data.BodyVertices.MaxBy(t => t.Y);
			Vector3 maxZ = data.BodyVertices.MaxBy(t => t.Z);

			Console.WriteLine("[INFO] Max X: " + maxX.X + ", Y: " + maxY.Y + ", " + maxZ.Z);

			minX = data.BodyVertices.MinBy(t => t.X);
			minY = data.BodyVertices.MinBy(t => t.Y);
			minZ = data.BodyVertices.MinBy(t => t.Z);

			Console.WriteLine("[INFO] Min X: " + minX.X + ", Y: " + minY.Y + ", " + minZ.Z);

			Vector3 size = new Vector3(maxX.X - minX.X, maxY.Y - minY.Y, maxZ.Z - minZ.Z);

			Console.WriteLine("[INFO] Size X: " + size.X + ", Y: " + size.Y + ", " + size.Z);

			float max = Math.Max(size.X, Math.Max(size.Y, size.Z));
			float factor = Scale / max;

			for (int i = 0; i < data.BodyVertices.Count; i++)
			{
				data.BodyVertices[i] = new Vector3((float)Math.Truncate(data.BodyVertices[i].X * factor), (float)Math.Truncate(data.BodyVertices[i].Y * factor), (float)Math.Truncate(data.BodyVertices[i].Z * factor));
			}

			minX = data.BodyVertices.MinBy(t => t.X);
			minY = data.BodyVertices.MinBy(t => t.Y);
			minZ = data.BodyVertices.MinBy(t => t.Z);

			//HashSet<Vector3> set = new HashSet<Vector3>();

			min = Math.Min(minX.X, Math.Min(minY.Y, minZ.Z));

			Console.WriteLine("[INFO] Started to voxelize data...");
			using (ProgressBar progressbar = new ProgressBar())
			{
				for (int i = 0; i < data.BodyVertices.Count; i++)
				{
					float maxV = Math.Max(data.BodyVertices[i].X, Math.Max(data.BodyVertices[i].Y, data.BodyVertices[i].Z));
					if (maxV - min >= 0)
					{
						data.BodyVertices[i] -= new Vector3(min, min, min);
						mSchematic.AddVoxel((int)(data.BodyVertices[i].X - minX.X), (int)(data.BodyVertices[i].Y - minY.Y), (int)(data.BodyVertices[i].Z - minZ.Z), data.BodyColors[i].ColorToUInt());
					}
					//if (!set.Contains(data.BodyVertices[i]))
					//{
					//	set.Add(data.BodyVertices[i]);
					//	//vertices.Add(data.BodyVertices[i]);
					//	//colors.Add(data.BodyColors[i]);
					//}
					progressbar.Report(i / (float)data.BodyVertices.Count);
				}
			}
			Console.WriteLine("[INFO] Done.");

			//minX = vertices.MinBy(t => t.X);
			//minY = vertices.MinBy(t => t.Y);
			//minZ = vertices.MinBy(t => t.Z);

			//for (int i = 0; i < vertices.Count; i++)
			//{
			//	float max = Math.Max(vertices[i].X, Math.Max(vertices[i].Y, vertices[i].Z));
			//	if (/*max - min < 8000 && */max - min >= 0)
			//	{
			//		vertices[i] -= new Vector3(min, min, min);
			//		_blocks.Add(new Voxel((ushort)vertices[i].X, (ushort)vertices[i].Y, (ushort)vertices[i].Z, colors[i].ColorToUInt()));
			//	}
			//}
		}


		public override Schematic WriteSchematic()
		{
			List<Voxel> list = Quantization.ApplyQuantization(mSchematic.GetAllVoxels(), ColorLimit);
			Schematic schematic = new Schematic(list);
			return schematic;
		}


	}
}
