using System;
using System.IO;
using FileToVoxCommon.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FileToVoxCommon.Generator.Heightmap.Data
{
	public class HeightmapData : JsonBaseImportData
	{
		public HeightmapStep[] Steps { get; set; }
		public override GeneratorType GeneratorType { get; set; } = GeneratorType.Heightmap;
	}

	public enum PlacementMode
	{
		ADDITIVE,
		REPLACE,
		SUBSTRACT,
		TOP_ONLY
	}

	public enum RotationMode
	{
		X,
		Y,
		Z,
	}

	public class HeightmapStep : StepData.StepData
	{
		public string TexturePath { get; set; }
		public string ColorTexturePath { get; set; }
		public int Height { get; set; }
		public int Offset { get; set; }
		public int OffsetMerge { get; set; }
		public bool EnableColor { get; set; }
		public int ColorLimit { get; set; } = 256;
		public bool Excavate { get; set; }
		public bool Reverse { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public PlacementMode PlacementMode { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public RotationMode RotationMode { get; set; }

		public void ValidateSettings()
		{
			if (string.IsNullOrEmpty(TexturePath))
			{
				throw new ArgumentException("[ERROR] Missing TexturePath");
			}

			TexturePath = Path.GetFullPath(TexturePath);
			if (!string.IsNullOrEmpty(TexturePath) && !File.Exists(TexturePath))
			{
				throw new ArgumentException("[ERROR] The TexturePath is invalid: " + TexturePath);
			}

			if (!string.IsNullOrEmpty(ColorTexturePath))
			{
				ColorTexturePath = Path.GetFullPath(ColorTexturePath);
				if (!File.Exists(ColorTexturePath))
				{
					throw new ArgumentException("[ERROR] The TexturePath is invalid: " + ColorTexturePath);
				}
			}

			if (Offset < 0)
			{
				Offset = 0;
			}

			if (Height < 0)
			{
				Height = 1;
			}

			if (ColorLimit < 0)
			{
				ColorLimit = 256;
			}
		}

		public void DisplayInfo()
		{
			Console.WriteLine("[INFO] ###############################");
			Console.WriteLine("[INFO] TexturePath: " + TexturePath);
			Console.WriteLine("[INFO] ColorTexturePath: " + ColorTexturePath);
			Console.WriteLine("[INFO] Height: " + Height);
			Console.WriteLine("[INFO] Offset: " + Offset);
			Console.WriteLine("[INFO] OffsetMerge: " + OffsetMerge);
			Console.WriteLine("[INFO] EnableColor: " + EnableColor);
			Console.WriteLine("[INFO] ColorLimit: " + ColorLimit);
			Console.WriteLine("[INFO] Excavate: " + Excavate);
			Console.WriteLine("[INFO] Reverse: " + Reverse);
			Console.WriteLine("[INFO] PlacementMode: " + PlacementMode);
			Console.WriteLine("[INFO] RotationMode: " + RotationMode);
			Console.WriteLine("[INFO] ###############################");
		}
	}
}
