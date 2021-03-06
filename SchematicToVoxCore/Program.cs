﻿using FileToVox.Converter;
using FileToVox.Converter.Image;
using FileToVox.Converter.Json;
using FileToVox.Converter.PointCloud;
using FileToVox.Schematics;
using FileToVox.Vox;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FileToVox
{
	class Program
	{
		public static bool DEBUG;

		private static string INPUT_PATH;
		private static string OUTPUT_PATH;
		private static string INPUT_COLOR_FILE;
		private static string INPUT_PALETTE_FILE;
		private static string INPUT_SHADER_FILE;

		private static bool SHOW_HELP;
		private static bool EXCAVATE;
		private static bool COLOR;
		private static bool MESH_SKIP_CAPTURE;

		private static float SCALE = 1;
		private static int HEIGHT_MAP = 1;
		private static int COLOR_LIMIT = 256;
		private static int MESH_SEGMENT_X = 4;
		private static int MESH_SEGMENT_Y = 4;
		private static int MESH_SUBSAMPLE = 0;

		public static int CHUNK_SIZE = 128;

		public static void Main(string[] args)
		{
			OptionSet options = new OptionSet()
			{
				{"i|input=", "input path", v => INPUT_PATH = v},
				{"o|output=", "output path", v => OUTPUT_PATH = v},
				{"s|shaders=", "input shader path", v => INPUT_SHADER_FILE = v},
				{"c|color", "enable color when generating heightmap", v => COLOR = v != null},
				{"cm|color-from-file=", "load colors from file", v => INPUT_COLOR_FILE = v },
				{"cl|color-limit=", "set the maximal number of colors for the palette", (int v) => COLOR_LIMIT =v },
				{"cs|chunk-size=", "set the chunk size", (int v) => CHUNK_SIZE = v},
				{"e|excavate", "delete all voxels which doesn't have at least one face connected with air",  v => EXCAVATE = v != null },
				{"h|help", "help informations", v => SHOW_HELP = v != null},
				{"hm|heightmap=", "create voxels terrain from heightmap (only for PNG file)", (int v) => HEIGHT_MAP = v},
				{"msx|mesh-segment-x=", "set the number of segment in X axis (for MeshSampler)", (int v) => MESH_SEGMENT_X = v},
				{"msy|mesh-segment-y=", "set the number of segment in Y axis (for MeshSampler)", (int v) => MESH_SEGMENT_Y = v},
				{"msub|mesh-subsample=", "set the number of subsample (for MeshSampler)", (int v) => MESH_SUBSAMPLE = v},
				{"mskip", "skip the capturing points part and load the previous PLY (for MeshSampler)", v => MESH_SKIP_CAPTURE = v != null},
				{"p|palette=", "set the palette", v => INPUT_PALETTE_FILE = v },
				{"sc|scale=", "set the scale", (float v) => SCALE = v},
				{"d|debug", "enable the debug mode", v => DEBUG = v != null},
			};

			try
			{
				List<string> extra = (args.Length > 0) ? options.Parse(args) : options.Parse(CheckArgumentsFile());
				DisplayInformations();
				CheckHelp(options);
				CheckArguments();
				DisplayArguments();

				bool success = false;
				if (INPUT_PATH != null)
				{
					success = ProcessFile();
				}

				if (success)
				{
					CheckDebug();
				}

				Console.WriteLine("[INFO] Done.");
				if (DEBUG)
				{
					Console.ReadKey();
				}
			}
			catch (Exception e)
			{
				Console.Write("FileToVox: ");
				Console.WriteLine(e.Message);
				Console.WriteLine("Try `FileToVox --help` for more informations.");
				Console.ReadLine();
			}
		}

		private static void DisplayInformations()
		{
			Console.WriteLine("[INFO] FileToVox v" + Assembly.GetExecutingAssembly().GetName().Version);
			Console.WriteLine("[INFO] Author: @Zarbuz. Contact : https://twitter.com/Zarbuz");
		}

		private static string[] CheckArgumentsFile()
		{
			if (!File.Exists("settings.ini"))
			{
				File.Create("settings.ini");
			}

			Console.WriteLine("[INFO] Reading arguments from settings.ini");
			string[] args = new string[0];
			using (StreamReader file = new StreamReader("settings.ini"))
			{
				string line;
				while ((line = file.ReadLine()) != null)
				{
					if (line.Contains("#"))
					{
						continue;
					}

					Console.WriteLine($"[INFO] {line}");
					args = line.Split(" ");
				}
			}

			return args;
		}

		private static void CheckHelp(OptionSet options)
		{
			if (SHOW_HELP)
			{
				ShowHelp(options);
				Environment.Exit(0);
			}
		}

		private static void CheckArguments()
		{
			if (INPUT_PATH == null)
				throw new ArgumentNullException("[ERROR] Missing required option: --i");
			if (OUTPUT_PATH == null)
				throw new ArgumentNullException("[ERROR] Missing required option: --o");
			if (SCALE <= 0)
				throw new ArgumentException("[ERROR] --scale argument must be positive");
			if (HEIGHT_MAP < 1)
				throw new ArgumentException("[ERROR] --heightmap argument must be positive");
			if (COLOR_LIMIT < 0 || COLOR_LIMIT > 256)
				throw new ArgumentException("[ERROR] --color-limit argument must be between 1 and 256");
			if (CHUNK_SIZE <= 10 || CHUNK_SIZE > 257)
				throw new ArgumentException("[ERROR] --chunk-size argument must be between 10 and 256");
		}

		private static void DisplayArguments()
		{
			if (INPUT_PATH != null)
				Console.WriteLine("[INFO] Specified input path: " + INPUT_PATH);
			if (OUTPUT_PATH != null)
				Console.WriteLine("[INFO] Specified output path: " + OUTPUT_PATH);
			if (INPUT_COLOR_FILE != null)
				Console.WriteLine("[INFO] Specified input color file: " + INPUT_COLOR_FILE);
			if (INPUT_PALETTE_FILE != null)
				Console.WriteLine("[INFO] Specified palette file: " + INPUT_PALETTE_FILE);
			if (INPUT_SHADER_FILE != null)
				Console.WriteLine("[INFO] Specified shaders file: " + INPUT_SHADER_FILE);
			if (COLOR_LIMIT != 256)
				Console.WriteLine("[INFO] Specified color limit: " + COLOR_LIMIT);
			if (SCALE != 1)
				Console.WriteLine("[INFO] Specified increase size: " + SCALE);
			if (CHUNK_SIZE != 128)
				Console.WriteLine("[INFO] Specified chunk size: " + CHUNK_SIZE);
			if (EXCAVATE)
				Console.WriteLine("[INFO] Enabled option: excavate");
			if (COLOR)
				Console.WriteLine("[INFO] Enabled option: color");
			if (HEIGHT_MAP != 1)
				Console.WriteLine("[INFO] Enabled option: heightmap (value=" + HEIGHT_MAP + ")");
			if (DEBUG)
				Console.WriteLine("[INFO] Enabled option: debug");
			if (MESH_SEGMENT_X != 4)
				Console.WriteLine("[INFO] Specified segment X (for MeshSampler): " + MESH_SEGMENT_X);
			if (MESH_SEGMENT_Y != 4)
				Console.WriteLine("[INFO] Specified segment Y (for MeshSampler): " + MESH_SEGMENT_Y);
			if (MESH_SUBSAMPLE != 0)
				Console.WriteLine("[INFO] Specified subsample (for MeshSampler): " + MESH_SUBSAMPLE);
			if (MESH_SKIP_CAPTURE)
				Console.WriteLine("[INFO] Enabled option: mesh skip capture (for MeshSampler)");

			Console.WriteLine("[INFO] Specified output path: " + Path.GetFullPath(OUTPUT_PATH));
		}

		private static bool ProcessFile()
		{
			string path = Path.GetFullPath(INPUT_PATH);
			bool isFolder = false;
			if (!Directory.Exists(path))
			{
				if (!File.Exists(path))
				{
					throw new FileNotFoundException("[ERROR] Input file not found at: ", path);
				}
			}
			else
			{
				isFolder = true;
			}

			if (!string.IsNullOrEmpty(INPUT_SHADER_FILE))
			{
				string pathShaders = Path.GetFullPath(INPUT_SHADER_FILE);
				if (!File.Exists(pathShaders))
				{
					throw new FileNotFoundException("[ERROR] Input shaders file not found at: ", pathShaders);
				}
			}
			try
			{
				AbstractToSchematic converter;
				if (isFolder)
				{
					converter = new FolderImageToSchematic(path, EXCAVATE, INPUT_COLOR_FILE, COLOR_LIMIT);
					return SchematicToVox(converter);
				}

				converter = GetConverter(path);
				if (converter != null)
				{
					return SchematicToVox(converter);
				}

				Console.WriteLine("[ERROR] Unsupported file extension !");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Console.ReadLine();
				return false;

			}

			return true;
		}

		private static AbstractToSchematic GetConverter(string path)
		{
			switch (Path.GetExtension(path))
			{
				case ".asc":
					return new ASCToSchematic(path);
				case ".binvox":
					return new BinvoxToSchematic(path);
				case ".csv":
					return new CSVToSchematic(path, SCALE, COLOR_LIMIT);
				case ".ply":
					return new PLYToSchematic(path, SCALE, COLOR_LIMIT);
				case ".png":
					return new PNGToSchematic(path, INPUT_COLOR_FILE, HEIGHT_MAP, EXCAVATE, COLOR, COLOR_LIMIT);
				case ".qb":
					return new QBToSchematic(path);
				case ".schematic":
					return new SchematicToSchematic(path, EXCAVATE, SCALE);
				case ".tif":
					return new TIFtoSchematic(path, INPUT_COLOR_FILE, HEIGHT_MAP, EXCAVATE, COLOR, COLOR_LIMIT);
				case ".xyz":
					return new XYZToSchematic(path, SCALE, COLOR_LIMIT);
				case ".json":
					return new JsonToSchematic(path);
				case ".vox":
					return new VoxToSchematic(path);
				case ".obj":
				case ".fbx":
					return new MeshToSchematic(path, SCALE, COLOR_LIMIT, MESH_SEGMENT_X, MESH_SEGMENT_Y, MESH_SUBSAMPLE, MESH_SKIP_CAPTURE);
				default:
					return null;
			}
		}

		private static bool SchematicToVox(AbstractToSchematic converter)
		{
			Schematic schematic = converter.WriteSchematic();
			Console.WriteLine($"[INFO] Vox Width: {schematic.Width}");
			Console.WriteLine($"[INFO] Vox Length: {schematic.Length}");
			Console.WriteLine($"[INFO] Vox Height: {schematic.Height}");

			if (schematic.Width > Schematic.MAX_WORLD_WIDTH || schematic.Length > Schematic.MAX_WORLD_LENGTH || schematic.Height > Schematic.MAX_WORLD_HEIGHT)
			{
				Console.WriteLine("[ERROR] Model is too big ! MagicaVoxel can't support model bigger than 2000x2000x1000");
				return false;
			}

			VoxWriter writer = new VoxWriter();

			if (INPUT_PALETTE_FILE != null)
			{
				PaletteSchematicConverter converterPalette = new PaletteSchematicConverter(INPUT_PALETTE_FILE);
				schematic = converterPalette.ConvertSchematic(schematic);
				return writer.WriteModel(CHUNK_SIZE, FormatOutputDestination(OUTPUT_PATH), converterPalette.GetPalette(), schematic);
			}

			if (INPUT_SHADER_FILE != null)
			{
				JsonToSchematic jsonParser = new JsonToSchematic(INPUT_SHADER_FILE, schematic);
				schematic = jsonParser.WriteSchematic();
			}

			return writer.WriteModel(CHUNK_SIZE, FormatOutputDestination(OUTPUT_PATH), null, schematic);
		}

		private static string FormatOutputDestination(string outputPath)
		{
			outputPath = outputPath.Replace(".vox", "");
			outputPath += ".vox";
			return outputPath;
		}

		private static void ShowHelp(OptionSet p)
		{
			Console.WriteLine("Usage: FileToVox --i INPUT --o OUTPUT");
			Console.WriteLine("Options: ");
			p.WriteOptionDescriptions(Console.Out);
		}

		private static void CheckDebug()
		{
			if (DEBUG)
			{
				VoxReader reader = new VoxReader();
				reader.LoadModel(FormatOutputDestination(OUTPUT_PATH));
			}
		}
	}
}
