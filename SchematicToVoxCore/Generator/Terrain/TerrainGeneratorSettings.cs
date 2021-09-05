using FileToVox.Generator.Terrain.Chunk;
using FileToVox.Generator.Terrain.Utility;
using System;
using FileToVoxCore.Extensions;
using FileToVoxCore.Schematics;
using FileToVoxCore.Schematics.Tools;
using BiomeSettings = FileToVox.Generator.Terrain.Data.BiomeSettings;
using TerrainGeneratorDataSettings = FileToVox.Generator.Terrain.Data.TerrainGeneratorDataSettings;
using TerrainStepType = FileToVox.Generator.Terrain.Data.TerrainStepType;

namespace FileToVox.Generator.Terrain
{
	public class TerrainGeneratorSettings : TerrainGeneratorDataSettings
	{
		#region Fields
		protected float[] mMoistureValues;
		protected int mNoiseMoistureTextureSize;
		protected HeightMapInfo[] mHeightChunkData;
		protected string mLastMoistureTextureLoaded;
		protected float mSeaLevelAlignedWithInt, mBeachLevelAlignedWithInt;
		protected int mGeneration;

		#endregion


		#region StaticConst

		public const int ONE_Y_ROW = TerrainEnvironment.CHUNK_SIZE * TerrainEnvironment.CHUNK_SIZE;

		#endregion


		#region PublicMethods

		public void Initialize()
		{
			mSeaLevelAlignedWithInt = (WaterLevel / MaxHeight);
			mBeachLevelAlignedWithInt = (WaterLevel + 1) / MaxHeight;
			if (Steps != null)
			{
				for (int i = 0; i < Steps.Length; i++)
				{
					if (!String.IsNullOrEmpty(Steps[i].NoiseTexturePath))
					{
						bool repeated = false;
						for (int j = 0; j < i - 1; j++)
						{
							if (Steps[i].NoiseTexturePath == Steps[j].NoiseTexturePath)
							{
								Steps[i].NoiseValues = Steps[j].NoiseValues;
								Steps[i].NoiseTextureSize = Steps[j].NoiseTextureSize;
								repeated = true;
								break;
							}
						}

						if (!repeated && (Steps[i].NoiseTextureSize == 0 || Steps[i].NoiseValues == null || Steps[i].LastTextureLoaded == null || Steps[i].NoiseTexturePath != Steps[i].LastTextureLoaded))
						{
							Steps[i].LastTextureLoaded = Steps[i].NoiseTexturePath;

							Steps[i].NoiseValues = NoiseTools.LoadNoiseTexture(Steps[i].NoiseTexturePath, out int NoiseTextureSize);
							Steps[i].NoiseTextureSize = NoiseTextureSize;
						}
					}

					if (Steps[i].InputIndex0 < 0 || Steps[i].InputIndex0 >= Steps.Length)
					{
						Steps[i].InputIndex0 = 0;
					}

					if (Steps[i].InputIndex1 < 0 || Steps[i].InputIndex1 >= Steps.Length)
					{
						Steps[i].InputIndex1 = 0;
					}
				}
			}

			if (!String.IsNullOrEmpty(MoisturePath) && (mNoiseMoistureTextureSize == 0 || mMoistureValues == null || mLastMoistureTextureLoaded == null || mLastMoistureTextureLoaded != MoisturePath))
			{
				mLastMoistureTextureLoaded = MoisturePath;
				mMoistureValues = NoiseTools.LoadNoiseTexture(MoisturePath, out int noiseMoistureTextureSize);
				mNoiseMoistureTextureSize = noiseMoistureTextureSize;
			}

			if (mHeightChunkData == null)
			{
				mHeightChunkData = new HeightMapInfo[TerrainEnvironment.CHUNK_SIZE * TerrainEnvironment.CHUNK_SIZE];
			}
		}

		public void GetHeightAndMoisture(float x, float z, out float altitude, out float moisture)
		{
			bool allowBeach = true;
			if (Steps != null && Steps.Length > 0)
			{
				float value = 0;
				for (int k = 0; k < Steps.Length; k++)
				{
					if (Steps[k].Enabled)
					{
						switch (Steps[k].OperationType)
						{
							case TerrainStepType.SampleHeightMapTexture:
								value = NoiseTools.GetNoiseValueBilinear(Steps[k].NoiseValues, Steps[k].NoiseTextureSize, x * Steps[k].Frequency, z * Steps[k].Frequency);
								value = value * (Steps[k].NoiseRangeMax - Steps[k].NoiseRangeMin) + Steps[k].NoiseRangeMin;
								break;
							case TerrainStepType.SampleRidgeNoiseFromTexture:
								value = NoiseTools.GetNoiseValueBilinear(Steps[k].NoiseValues, Steps[k].NoiseTextureSize, x * Steps[k].Frequency, z * Steps[k].Frequency, true);
								value = value * (Steps[k].NoiseRangeMax - Steps[k].NoiseRangeMin) + Steps[k].NoiseRangeMin;
								break;
							case TerrainStepType.Shift:
								value += Steps[k].Param;
								break;
							case TerrainStepType.BeachMask:
								{
									int i1 = Steps[k].InputIndex0;
									if (Steps[i1].Value > Steps[k].Threshold)
									{
										allowBeach = false;
									}
								}
								break;
							case TerrainStepType.AddAndMultiply:
								value = (value + Steps[k].Param) * Steps[k].Param2;
								break;
							case TerrainStepType.MultiplyAndAdd:
								value = (value * Steps[k].Param) + Steps[k].Param2;
								break;
							case TerrainStepType.Exponential:
								if (value < 0)
									value = 0;
								value = (float)Math.Pow(value, Steps[k].Param);
								break;
							case TerrainStepType.Constant:
								value = Steps[k].Param;
								break;
							case TerrainStepType.Invert:
								value = 1f - value;
								break;
							case TerrainStepType.Copy:
								{
									int i1 = Steps[k].InputIndex0;
									value = Steps[i1].Value;
								}
								break;
							case TerrainStepType.Random:
								value = WorldRandom.GetValue(x, z);
								break;
							case TerrainStepType.BlendAdditive:
								{
									int i1 = Steps[k].InputIndex0;
									int i2 = Steps[k].InputIndex1;
									value = Steps[i1].Value * Steps[k].Weight0 + Steps[i2].Value * Steps[k].Weight1;
								}
								break;
							case TerrainStepType.BlendMultiply:
								{
									int i1 = Steps[k].InputIndex0;
									int i2 = Steps[k].InputIndex1;
									value = Steps[i1].Value * Steps[i2].Value;
								}
								break;
							case TerrainStepType.Threshold:
								{
									int i1 = Steps[k].InputIndex0;
									if (Steps[i1].Value >= Steps[k].Threshold)
									{
										value = Steps[i1].Value + Steps[k].ThresholdShift;
									}
									else
									{
										value = Steps[k].ThresholdParam;
									}
								}
								break;
							case TerrainStepType.FlattenOrRaise:
								if (value >= Steps[k].Threshold)
								{
									value = (value - Steps[k].Threshold) * Steps[k].ThresholdParam + Steps[k].Threshold;
								}
								break;
							case TerrainStepType.Clamp:
								if (value < Steps[k].Min)
									value = Steps[k].Min;
								else if (value > Steps[k].Max)
									value = Steps[k].Max;
								break;
							case TerrainStepType.Select:
								{
									int i1 = Steps[k].InputIndex0;
									if (Steps[i1].Value < Steps[k].Min)
									{
										value = Steps[k].ThresholdParam;
									}
									else if (Steps[i1].Value > Steps[k].Max)
									{
										value = Steps[k].ThresholdParam;
									}
									else
									{
										value = Steps[i1].Value;
									}
								}
								break;
							case TerrainStepType.Fill:
								{
									int i1 = Steps[k].InputIndex0;
									if (Steps[i1].Value >= Steps[k].Min && Steps[i1].Value <= Steps[k].Max)
									{
										value = Steps[k].ThresholdParam;
									}
								}
								break;
						}
					}
					Steps[k].Value = value;
				}
				altitude = value;
			}
			else
			{
				altitude = -9999;
			}

			// Moisture
			moisture = NoiseTools.GetNoiseValueBilinear(mMoistureValues, mNoiseMoistureTextureSize, x * MoistureScale, z * MoistureScale);

			if (altitude < mBeachLevelAlignedWithInt && altitude >= mSeaLevelAlignedWithInt)
			{
				float depth = mBeachLevelAlignedWithInt - altitude;
				if (depth > BeachWidth || !allowBeach)
				{
					altitude = mSeaLevelAlignedWithInt - 0.0001f;
				}
			}

			if (altitude < mSeaLevelAlignedWithInt)
			{
				float depth = mSeaLevelAlignedWithInt - altitude;
				altitude = mSeaLevelAlignedWithInt - 0.0001f - depth * SeaDepthMultiplier;
			}

		}

		public void PaintChunk(VoxelChunk chunk)
		{
			Vector3 position = chunk.Position;
			if (position.Y + TerrainEnvironment.CHUNK_HALF_SIZE < MinHeight)
			{
				return;
			}

			int bedrockRow = -1;
			if (position.Y < MinHeight + TerrainEnvironment.CHUNK_HALF_SIZE)
			{
				bedrockRow = (int)(MinHeight - (position.Y - TerrainEnvironment.CHUNK_HALF_SIZE) + 1) * ONE_Y_ROW - 1;
			}

			position.X -= TerrainEnvironment.CHUNK_HALF_SIZE;
			position.Y -= TerrainEnvironment.CHUNK_HALF_SIZE;
			position.Z -= TerrainEnvironment.CHUNK_HALF_SIZE;
			Vector3 pos = new Vector3();
			int waterLevel = WaterLevel > 0 ? WaterLevel : -1;
			Voxel[] voxels = chunk.Voxels;
			mGeneration++;
			TerrainEnvironment.Instance.GetHeightMapInfo(position.X, position.Z, mHeightChunkData);
			int shiftAmount = (int)MathF.Log(TerrainEnvironment.CHUNK_SIZE, 2);

			for (int arrayIndex = 0; arrayIndex < TerrainEnvironment.CHUNK_SIZE * TerrainEnvironment.CHUNK_SIZE; arrayIndex++)
			{
				float groundLevel = mHeightChunkData[arrayIndex].GroundLevel;
				float surfaceLevel = waterLevel > groundLevel ? waterLevel : groundLevel;
				if (surfaceLevel < pos.Y)
				{
					continue;
				}

				BiomeSettings biome = mHeightChunkData[arrayIndex].Biome;
				if (biome == null)
				{
					biome = TerrainEnvironment.Instance.WorldTerrainData.DefaultBiome;
					if (biome == null)
						continue;
				}

				int y = (int)(surfaceLevel - position.Y);
				if (y >= TerrainEnvironment.CHUNK_SIZE)
				{
					y = TerrainEnvironment.CHUNK_SIZE - 1;
				}

				pos.Y = position.Y + y;
				pos.X = position.X + (arrayIndex & (TerrainEnvironment.CHUNK_SIZE - 1));
				pos.Z = position.Z + (arrayIndex >> shiftAmount);

				int voxelIndex = y * ONE_Y_ROW + arrayIndex;
				if (voxelIndex < 0)
				{
					continue;
				}
				if (pos.Y > groundLevel)
				{
					while (pos.Y > groundLevel && voxelIndex >= 0)
					{
						voxels[voxelIndex].Color = WaterColor.ColorToUInt();
						voxelIndex -= ONE_Y_ROW;
						pos.Y--;
					}
				}
				else if (pos.Y == groundLevel)
				{
					voxels[voxelIndex].Color = 0;

					if (voxels[voxelIndex].Color == 0)
					{
						if (pos.Y == waterLevel)
						{
							voxels[voxelIndex].Color = ShoreColor.ColorToUInt();
						}
						else
						{
							//float moisture = mHeightChunkData[arrayIndex].Moisture;
							//Color colorTop = biome.VoxelTop;
							//Color newColor = Color.FromArgb((int) (colorTop.R * moisture), (int)(colorTop.G * moisture), (int)(colorTop.B * moisture));
							//voxels[voxelIndex].Color = newColor.ColorToUInt();

							voxels[voxelIndex].Color = biome.VoxelTop.ColorToUInt();

							if (pos.Y > waterLevel)
							{
								float rdn = WorldRandom.GetValue(pos);
								if (biome.TreeDensity > 0 && rdn < biome.TreeDensity && biome.Trees.Length > 0)
								{
									TerrainEnvironment.Instance.RequestTreeCreation(chunk, pos, TerrainEnvironment.Instance.GetTree(biome.Trees, rdn / biome.TreeDensity));
								}
								else if (biome.VegetationDensity > 0 && rdn < biome.VegetationDensity && biome.Vegetation.Length > 0)
								{
									if (voxelIndex >= (TerrainEnvironment.CHUNK_SIZE - 1) * ONE_Y_ROW)
									{
										TerrainEnvironment.Instance.RequestVegetationCreation(chunk.Top, voxelIndex - ONE_Y_ROW * (TerrainEnvironment.CHUNK_SIZE - 1), TerrainEnvironment.Instance.GetVegetation(biome, rdn / biome.VegetationDensity));
									}
									else
									{
										voxels[voxelIndex + ONE_Y_ROW].Color = TerrainEnvironment.Instance.GetVegetation(biome, rdn / biome.VegetationDensity).ColorToUInt();
									}
								}
							}
						}

						voxelIndex -= ONE_Y_ROW;
						pos.Y--;
					}
				}

				biome.BiomeGeneration = mGeneration;

				while (voxelIndex >= 0 && voxels[voxelIndex].Color == 0 && pos.Y <= waterLevel)
				{
					voxels[voxelIndex].Color = WaterColor.ColorToUInt();
					voxelIndex -= ONE_Y_ROW;
					pos.Y--;
				}

				for (; voxelIndex > bedrockRow; voxelIndex -= ONE_Y_ROW, pos.Y--)
				{
					if (voxels[voxelIndex].Color == 0)
					{
						voxels[voxelIndex].Color = biome.VoxelDirt.ColorToUInt();
					}
					else if (voxels[voxelIndex].Color == 0 && pos.Y <= waterLevel)
					{ // hole under water level -> fill with water
						voxels[voxelIndex].Color = WaterColor.ColorToUInt();
					}
				}
				if (bedrockRow >= 0 && voxelIndex >= 0)
				{
					voxels[voxelIndex].Color = BedrockColor.ColorToUInt();
				}
			}
		}

		#endregion
	}
}
