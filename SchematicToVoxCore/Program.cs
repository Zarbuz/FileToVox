using FileToVox.Converter;
using FileToVox.Converter.Image;
using FileToVox.Converter.Json;
using FileToVox.Converter.PointCloud;
using FileToVoxCore.Schematics;
using FileToVoxCore.Vox;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FileToVox.Converter.PaletteSchematic;

namespace FileToVox
{
	class Program
	{
		private static string INPUT_PATH;
		private static string OUTPUT_PATH;
		private static string INPUT_COLOR_FILE;
		private static string INPUT_PALETTE_FILE;
		private static string INPUT_SHADER_FILE;

		private static bool SHOW_HELP;
		private static bool EXCAVATE;
		private static bool COLOR;


		private static float GRID_SIZE = 10;
		private static int HEIGHT_MAP = 1;
		private static int COLOR_LIMIT = 256;



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
				{"cs|chunk-size=", "set the chunk size", (int v) => Schematic.CHUNK_SIZE = v},
				{"e|excavate", "delete all voxels which doesn't have at least one face connected with air",  v => EXCAVATE = v != null },
				{"h|help", "help informations", v => SHOW_HELP = v != null},
				{"hm|heightmap=", "create voxels terrain from heightmap (only for PNG file)", (int v) => HEIGHT_MAP = v},
				{"p|palette=", "set the palette", v => INPUT_PALETTE_FILE = v },
				{"gs|grid-size=", "set the grid-size", (float v) => GRID_SIZE = v},
				{"d|debug", "enable the debug mode", v => Schematic.DEBUG = v != null},
			};

			try
			{
				List<string> extra = options.Parse(args);
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
				if (Schematic.DEBUG)
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
			if (GRID_SIZE < 10 || GRID_SIZE > Schematic.MAX_WORLD_LENGTH)
				throw new ArgumentException("[ERROR] --grid-size argument must be greater than 10 and smaller than " + Schematic.MAX_WORLD_LENGTH);
			if (HEIGHT_MAP < 1)
				throw new ArgumentException("[ERROR] --heightmap argument must be positive");
			if (COLOR_LIMIT < 0 || COLOR_LIMIT > 256)
				throw new ArgumentException("[ERROR] --color-limit argument must be between 1 and 256");
			if (Schematic.CHUNK_SIZE <= 10 || Schematic.CHUNK_SIZE > 256)
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
			if (GRID_SIZE != 10)
				Console.WriteLine("[INFO] Specified grid size: " + GRID_SIZE);
			if (Schematic.CHUNK_SIZE != 128)
				Console.WriteLine("[INFO] Specified chunk size: " + Schematic.CHUNK_SIZE);
			if (EXCAVATE)
				Console.WriteLine("[INFO] Enabled option: excavate");
			if (COLOR)
				Console.WriteLine("[INFO] Enabled option: color");
			if (HEIGHT_MAP != 1)
				Console.WriteLine("[INFO] Enabled option: heightmap (value=" + HEIGHT_MAP + ")");
			if (Schematic.DEBUG)
				Console.WriteLine("[INFO] Enabled option: debug");
			
			Console.WriteLine("[INFO] Specified output path: " + Path.GetFullPath(OUTPUT_PATH));
		}

		private static bool ProcessFile()
		{
			string path = Path.GetFullPath(INPUT_PATH);
			bool isFolder = Directory.Exists(path);

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
				string[] files = INPUT_PATH.Split(";");
				if (isFolder)
				{
					List<string> images = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(s => s.EndsWith(".png")).ToList();
					converter = new MultipleImageToSchematic(images, EXCAVATE, INPUT_COLOR_FILE, COLOR_LIMIT);
					return SchematicToVox(converter);
				}
				if (files.Length > 0)
				{
					converter = new MultipleImageToSchematic(files.ToList(), EXCAVATE, INPUT_COLOR_FILE, COLOR_LIMIT);
					return SchematicToVox(converter);
				}

				if (!File.Exists(path))
				{
					throw new FileNotFoundException("[ERROR] File not found at: " + path);
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
					return new CSVToSchematic(path, GRID_SIZE, COLOR_LIMIT);
				case ".ply":
					return new PLYToSchematic(path, GRID_SIZE, COLOR_LIMIT);
				case ".png":
					return new PNGToSchematic(path, INPUT_COLOR_FILE, HEIGHT_MAP, EXCAVATE, COLOR, COLOR_LIMIT);
				case ".qb":
					return new QBToSchematic(path);
				case ".schematic":
					return new SchematicToSchematic(path, EXCAVATE);
				case ".tif":
					return new TIFtoSchematic(path, INPUT_COLOR_FILE, HEIGHT_MAP, EXCAVATE, COLOR, COLOR_LIMIT);
				case ".xyz":
					return new XYZToSchematic(path, GRID_SIZE, COLOR_LIMIT);
				case ".json":
					return new JsonToSchematic(path);
				case ".vox":
					return new VoxToSchematic(path);
				case ".obj":
				case ".fbx":
					throw new Exception("[FAILED] Voxelization of 3D models is no longer done in FileToVox but with MeshSampler. Check the url : https://github.com/Zarbuz/FileToVox/releases for download link");
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
				return writer.WriteModel(FormatOutputDestination(OUTPUT_PATH), converterPalette.GetPalette(), schematic);
			}

			if (INPUT_SHADER_FILE != null)
			{
				JsonToSchematic jsonParser = new JsonToSchematic(INPUT_SHADER_FILE, schematic);
				schematic = jsonParser.WriteSchematic();
			}

			return writer.WriteModel(FormatOutputDestination(OUTPUT_PATH), null, schematic);
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
			if (Schematic.DEBUG)
			{
				VoxReader reader = new VoxReader();
				reader.LoadModel(FormatOutputDestination(OUTPUT_PATH));
			}
		}
	}
}
