using FileToVoxCore.Extensions;
using FileToVoxCore.Schematics;
using FileToVoxCore.Schematics.Tools;
using FileToVoxCore.Utils;
using FileToVoxCore.Vox;
using MoreLinq;
using System.Drawing;

namespace FileToVox.Converter
{
	public class VoxToSchematic : AbstractToSchematic
	{
		private VoxModel mVoxModel;
		public VoxToSchematic(string path) : base(path)
		{
			VoxReader reader = new VoxReader();
			mVoxModel = reader.LoadModel(path);
		}

		public override Schematic WriteSchematic()
		{
			Schematic schematic = new Schematic();
			Color[] colorsPalette = mVoxModel.Palette;
			using (ProgressBar progressbar = new ProgressBar())
			{
				int minX = (int) mVoxModel.TransformNodeChunks.MinBy(t => t.TranslationAt().X).TranslationAt().X;
				int minY = (int) mVoxModel.TransformNodeChunks.MinBy(t => t.TranslationAt().Y).TranslationAt().Y;
				int minZ = (int) mVoxModel.TransformNodeChunks.MinBy(t => t.TranslationAt().Z).TranslationAt().Z;

				for (int i = 0; i < mVoxModel.VoxelFrames.Count; i++)
				{
					VoxelData data = mVoxModel.VoxelFrames[i];
					Vector3 worldPositionFrame = mVoxModel.TransformNodeChunks[i + 1].TranslationAt();

					if (worldPositionFrame == Vector3.zero)
						continue;

					for (int y = 0; y < data.VoxelsTall; y++)
					{
						for (int z = 0; z < data.VoxelsDeep; z++)
						{
							for (int x = 0; x < data.VoxelsWide; x++)
							{
								int indexColor = data.Get(x, y, z);
								Color color = colorsPalette[indexColor];
								if (color != Color.Empty)
								{
									schematic.AddVoxel((int)(z + worldPositionFrame.X - minX), (int)(y + (worldPositionFrame.Z - minZ)), (int)(x + worldPositionFrame.Y - minY), color.ColorToUInt());
								}
							}
						}
					}
					progressbar.Report(i / (float)mVoxModel.VoxelFrames.Count);
				}
			}


			return schematic;
		}
	}
}
