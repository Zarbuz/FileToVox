using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using FileToVoxCommon.Json;
using FileToVoxCore.Schematics;
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
		[Display(Name = "Additive", Description = "Adds the result of the heightmap generation to the final result")]
		ADDITIVE = 0,
		[Display(Name = "Replace", Description = "Replaces the color of the voxels that matches the previous generation step")]
		REPLACE = 1,
		[Display(Name = "Substract", Description = "Removes voxels that match with the previous generation step")]
		SUBSTRACT = 2,
		[Display(Name = "TopOnly", Description = "Add voxels only if there are voxels from the previous step")]
		TOP_ONLY
	}



	public class HeightmapStep : StepData.StepData
	{
		public string TexturePath { get; set; }
		public string ColorTexturePath { get; set; }

		[Description("Height: The desired height")]
		[Range(1, 1000)]
		public int Height { get; set; }

		[Description("Offset: Offset to shift the base of the generation")]
		[Range(1, 1000)]
		public int Offset { get; set; }

		[Description("OffsetMerge: Offset to offset the base of the generation with respect to the previous step. Only valid for a placementMode at 'TOP_ONLY'")]
		[Range(1, 1000)]
		public int OffsetMerge { get; set; }

		[Description("EnableColor: Activate yes or no colors. If ColorTexturePath is not specified, then the rendering will only be shades of gray")]
		public bool EnableColor { get; set; }

		[Description("ColorLimit: Limit the number of colors imported")]
		[Range(1, 256)]
		public int ColorLimit { get; set; } = 256;

		[Description("Excavate: Removes all voxels which are not visible (all voxels which do not have at least one empty voxel as a neighbor)")]
		public bool Excavate { get; set; }

		[Description("Reverse: Reverse the direction of generation")]
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
