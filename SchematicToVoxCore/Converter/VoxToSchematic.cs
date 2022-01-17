using System;
using FileToVox.Extensions;
using FileToVoxCore.Extensions;
using FileToVoxCore.Schematics;
using FileToVoxCore.Utils;
using FileToVoxCore.Vox;
using MoreLinq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FileToVoxCore.Schematics.Tools;
using FileToVoxCore.Vox.Chunks;
using Vector3 = FileToVoxCore.Schematics.Tools.Vector3;

namespace FileToVox.Converter
{
	public class VoxToSchematic : AbstractToSchematic
	{
		private VoxModel mVoxModel;
		private static readonly Dictionary<int, Matrix4x4> mModelMatrix = new Dictionary<int, Matrix4x4>();

		public VoxToSchematic(string path) : base(path)
		{
			VoxReader reader = new VoxReader();
			mVoxModel = reader.LoadModel(path);
		}

		public override Schematic WriteSchematic()
		{
			Schematic schematic = new Schematic();
			FileToVoxCore.Drawing.Color[] colorsPalette = mVoxModel.Palette;
			using (ProgressBar progressbar = new ProgressBar())
			{
				for (int i = 0; i < mVoxModel.TransformNodeChunks.Count; i++)
				{
					TransformNodeChunk transformNodeChunk = mVoxModel.TransformNodeChunks[i];
					int childId = transformNodeChunk.ChildId;

					if (mModelMatrix.ContainsKey(transformNodeChunk.Id))
					{
						mModelMatrix[transformNodeChunk.Id] *= ReadMatrix4X4FromRotation(transformNodeChunk.RotationAt(), transformNodeChunk.TranslationAt());
					}
					else
					{
						mModelMatrix[transformNodeChunk.Id] = ReadMatrix4X4FromRotation(transformNodeChunk.RotationAt(), transformNodeChunk.TranslationAt());
					}

					GroupNodeChunk groupNodeChunk = mVoxModel.GroupNodeChunks.FirstOrDefault(grp => grp.Id == childId);
					if (groupNodeChunk != null)
					{
						foreach (int child in groupNodeChunk.ChildIds)
						{
							mModelMatrix[child] = ReadMatrix4X4FromRotation(transformNodeChunk.RotationAt(), transformNodeChunk.TranslationAt());
						}
					}
					else
					{
						ShapeNodeChunk shapeNodeChunk = mVoxModel.ShapeNodeChunks.FirstOrDefault(shp => shp.Id == childId);
						if (shapeNodeChunk == null)
						{
							Console.WriteLine("[ERROR] Failed to find chunk with ID: " + childId);
						}
						else
						{
							foreach (ShapeModel shapeModel in shapeNodeChunk.Models)
							{
								int modelId = shapeModel.ModelId;
								VoxelData data = mVoxModel.VoxelFrames[modelId];
								
								Vector3 initialVolumeSize = new Vector3(data.VoxelsWide, data.VoxelsTall, data.VoxelsDeep);

								Vector3 pivot = new Vector3(initialVolumeSize.X / 2, initialVolumeSize.Y / 2, initialVolumeSize.Z / 2);
								Vector3 fpivot = new Vector3(initialVolumeSize.X / 2f, initialVolumeSize.Y / 2f, initialVolumeSize.Z / 2f);

								Matrix4x4 matrix4X4 = mModelMatrix[transformNodeChunk.Id];
								for (int y = 0; y < data.VoxelsTall; y++)
								{
									for (int z = 0; z < data.VoxelsDeep; z++)
									{
										for (int x = 0; x < data.VoxelsWide; x++)
										{
											int indexColor = data.Get(x, y, z);
											if (indexColor != 0)
											{
												FileToVoxCore.Drawing.Color color = colorsPalette[indexColor - 1];
												Vector3Int tmpVoxel = new Vector3Int(x, y, z);

												Vector3 pos = new(tmpVoxel.X + 0.5f, tmpVoxel.Y + 0.5f, tmpVoxel.Z + 0.5f);
												pos -= pivot;
												pos = matrix4X4.MultiplyPoint(pos);
												pos += pivot;

												pos.X += fpivot.X;
												pos.Y += fpivot.Y;
												pos.Z -= fpivot.Z;

												tmpVoxel.X = (int)Math.Floor(pos.X);
												tmpVoxel.Y = (int)Math.Floor(pos.Y);
												tmpVoxel.Z = (int)Math.Floor(pos.Z);

												schematic.AddVoxel(tmpVoxel.X, tmpVoxel.Y, tmpVoxel.Z, color.ColorToUInt());
											}
										}
									}
								}
							}
						}
					}

					progressbar.Report(i / (float)mVoxModel.TransformNodeChunks.Count);

				}
			}

			return schematic;
		}

		public static Matrix4x4 ReadMatrix4X4FromRotation(Rotation rotation, Vector3 transform)
		{
			Matrix4x4 result = Matrix4x4.identity;
			{
				byte r = Convert.ToByte(rotation);
				int indexRow0 = (r & 3);
				int indexRow1 = (r & 12) >> 2;
				bool signRow0 = (r & 16) == 0;
				bool signRow1 = (r & 32) == 0;
				bool signRow2 = (r & 64) == 0;

				result.SetRow(0, Vector4.zero);
				switch (indexRow0)
				{
					case 0: result[0, 0] = signRow0 ? 1f : -1f; break;
					case 1: result[0, 1] = signRow0 ? 1f : -1f; break;
					case 2: result[0, 2] = signRow0 ? 1f : -1f; break;
				}
				result.SetRow(1, Vector4.zero);
				switch (indexRow1)
				{
					case 0: result[1, 0] = signRow1 ? 1f : -1f; break;
					case 1: result[1, 1] = signRow1 ? 1f : -1f; break;
					case 2: result[1, 2] = signRow1 ? 1f : -1f; break;
				}
				result.SetRow(2, Vector4.zero);
				switch (indexRow0 + indexRow1)
				{
					case 1: result[2, 2] = signRow2 ? 1f : -1f; break;
					case 2: result[2, 1] = signRow2 ? 1f : -1f; break;
					case 3: result[2, 0] = signRow2 ? 1f : -1f; break;
				}

				result.SetColumn(3, new Vector4(transform.X, transform.Y, transform.Z, 1f));
			}
			return result;
		}
	}
}
