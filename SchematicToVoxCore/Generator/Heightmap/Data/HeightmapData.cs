using System;
using System.IO;
using FileToVox.Converter.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace FileToVox.Generator.Heightmap.Data
{
	public class HeightmapData : JsonBaseImportData
	{
		public HeightmapStep[] Steps { get; set; }
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

	public class HeightmapStep
	{
		public string TexturePath { get; set; }
		public string ColorTexturePath { get; set; }
		public int Height { get; set; }
		public int Offset { get; set; }
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

			if (!string.IsNullOrEmpty(TexturePath) && !File.Exists(TexturePath))
			{
				throw new ArgumentException("[ERROR] The TexturePath is invalid: " + TexturePath);
			}

			if (!string.IsNullOrEmpty(ColorTexturePath) && !File.Exists(ColorTexturePath))
			{
				throw new ArgumentException("[ERROR] The TexturePath is invalid: " + ColorTexturePath);
			}

			if (Height < 0)
			{
				Height = 1;
			}

			if (Offset < 0)
			{
				Offset = 0;
			}

			if (ColorLimit < 0)
			{
				ColorLimit = 256;
			}
		}
	}
}
