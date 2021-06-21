using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
		Threshold,
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

		[JsonConverter(typeof(StringEnumConverter))]
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
		public float Weight0 { get; set; }
		public float Weight1 { get; set; }
		public float Min { get; set; }
		public float Max { get; set; }

		[JsonIgnore] public float[] NoiseValues { get; set; }
		[JsonIgnore] public int NoiseTextureSize { get; set; }
		[JsonIgnore] public float Value { get; set; }
		[JsonIgnore] public string LastTextureLoaded { get; set; }
	}

	public class TerrainGeneratorDataSettings
	{
		#region Fields

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



		#endregion
	}
}
