using System;
using System.Drawing;
using System.Text.Json.Serialization;
using FileToVox.Generator.Terrain.Utility;

namespace FileToVox.Generator.Terrain.Data
{
	public enum TerrainStepType
	{
		SampleHeightMapTexture,
		SampleRidgeNoiseFromTexture,
		Constant,
		Copy,
		Random,
		Invert,
		Shift,
		BeachMask,
		AddAndMultiply,
		MultiplyAndAdd,
		Exponential,
		FlattenOrRaise,
		BlendAdditive,
		BlendMultiply,
		Clamp,
		Select,
		Fill
	}

	public struct StepData
	{
		public bool Enabled { get; set; }
		public TerrainStepType OperationType { get; set; }
		public string NoiseTexturePath { get; set; }
		public float Frequency { get; set; }
		public float NoiseRangeMin { get; set; }
		public float NoiseRangeMax { get; set; }
		public int InputIndex0 { get; set; }
		public int InputIndex1 { get; set; }
		public float Threshold { get; set; }
		public float ThresholdShift { get; set; }
		public float ThresholdParam { get; set; }
		public float Param { get; set; }
		public float Param2 { get; set; }
		public float Min { get; set; }
		public float Max { get; set; }

		[JsonIgnore] public float[] NoiseValues { get; set; }
		[JsonIgnore] public int NoiseTextureSize { get; set; }
		[JsonIgnore] public float Value { get; set; }
		[JsonIgnore] public string LastTextureLoaded { get; set; }
	}

	public class TerrainGeneratorSettings
	{
		public float MaxHeight { get; set; } = 100;

		public float MinHeight { get; set; }

		public int WaterLevel { get; set; } = 25;

		public StepData[] Steps { get; set; }

		public float SeaDepthMultiplier { get; set; }
		public float BeachWidth { get; set; } = 0.001f;
		public Color WaterColor { get; set; }
		public Color ShoreColor { get; set; }
		public Color BedrockColor { get; set; }

		public string MoisturePath { get; set; }
		public float MoistureScale { get; set; } = 0.2f;


		protected float[] mMoistureValues;
		protected int mNoiseMoistureTextureSize;
		protected bool mPaintShore;
		protected HeightMapInfo[] mHeightChunkData;
		protected string mLastMoistureTextureLoaded;
		protected float mSeaLevelAlignedWithInt, mBeachLevelAlignedWithInt;
		protected int mGeneration;

		public void Initialize()
		{
			mSeaLevelAlignedWithInt = (WaterLevel / MaxHeight);
			mBeachLevelAlignedWithInt = (WaterLevel + 1) / MaxHeight;
			if (Steps != null)
			{
				for (int i = 0; i < Steps.Length; i++)
				{
					if (!string.IsNullOrEmpty(Steps[i].NoiseTexturePath))
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

							Steps[i].NoiseValues = NoiseTools.LoadNoiseTexture(Steps[i].NoiseTexturePath, out int noiseTextureSize);
							Steps[i].NoiseTextureSize = noiseTextureSize;
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

			if (!string.IsNullOrEmpty(MoisturePath) && (mNoiseMoistureTextureSize == 0 || mMoistureValues == null || mLastMoistureTextureLoaded == null || mLastMoistureTextureLoaded != MoisturePath))
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
	}
}
