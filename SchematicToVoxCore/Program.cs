using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileToVox.Converter;
using FileToVox.Converter.Image;
using FileToVox.Converter.Json;
using FileToVox.Converter.PointCloud;
using FileToVox.Schematics;
using FileToVox.Utils;
using FileToVox.Vox;
using NDesk.Options;

namespace FileToVox
{
	class Program
	{
		public static bool DEBUG;

		private static string INPUT_PATH;
		private static string OUTPUT_PATH;
		private static string INPUT_COLOR_FILE;
		private static string INPUT_PALETTE_FILE;

		private static bool SHOW_HELP;
		private static bool EXCAVATE;
		private static bool COLOR;
		private static bool SLICE;

		private static bool SHADER_FIX_HOLES;
		private static bool SHADER_FIX_LONELY;
		private static int SHADER_CASE;

		private static float SLOW;

		private static float SCALE = 1;
		private static int HEIGHT_MAP = 1;
		private static int GRID_SIZE = 128;
		private static int COLOR_LIMIT = 256;
		public static int CHUNK_SIZE = 128;


		

		public static void Main(string[] args)
		{
			OptionSet options = new OptionSet()
			{
				{"i|input=", "input path", v => INPUT_PATH = v},
				{"o|output=", "output path", v => OUTPUT_PATH = v},
				{"c|color", "enable color when generating heightmap", v => COLOR = v != null},
				{"cm|color-from-file=", "load colors from file", v => INPUT_COLOR_FILE = v },
				{"cl|color-limit=", "set the maximal number of colors for the palette", (int v) => COLOR_LIMIT =v },
				{"cs|chunk-size=", "set the chunk size", (int v) => CHUNK_SIZE = v},
				{"e|excavate", "delete all voxels which doesn't have at least one face connected with air",  v => EXCAVATE = v != null },
				{"gs|grid-size=", "set the grid size", (int v) => GRID_SIZE = v },
				{"h|help", "help informations", v => SHOW_HELP = v != null},
				{"hm|heightmap=", "create voxels terrain from heightmap (only for PNG file)", (int v) => HEIGHT_MAP = v},
				{"p|palette=", "set the palette", v => INPUT_PALETTE_FILE = v },
				{"shader-fix-lonely", "delete all voxels where all connected voxels are air", v => SHADER_FIX_LONELY = v != null },
				{"shader-fix-holes", "fix holes", v => SHADER_FIX_HOLES = v != null },
				{"shader-case=", "shader case",(int v) => SHADER_CASE = v },
				{"sc|scale=", "set the scale", (float v) => SCALE = v},
				{"si|slice", "indicate that each picture is a slice", v => SLICE = v != null},
				{"sl|slow=", "use a slower algorithm (use all cores) to generate voxels from OBJ but best result (value should be enter 0.0 and 1.0 (0.5 is recommended)", (float v) => SLOW = v },
				{"d|debug", "enable the debug mode", v => DEBUG = v != null},
			};

			try
			{
				List<string> extra = (args.Length > 0) ? options.Parse(args) : options.Parse(CheckArgumentsFile());
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
					CheckVerbose();
				}

				Console.WriteLine("[LOG] Done.");
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
			if (SHADER_CASE < 0)
				throw new ArgumentException("[ERROR] --shader-case argument must be positive");
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
			if (COLOR_LIMIT != 256)
				Console.WriteLine("[INFO] Specified color limit: " + COLOR_LIMIT);
			if (SCALE != 1)
				Console.WriteLine("[INFO] Specified increase size: " + SCALE);
			if (GRID_SIZE != 128)
				Console.WriteLine("[INFO] Specified grid size: " + GRID_SIZE);
			if (CHUNK_SIZE != 128)
				Console.WriteLine("[INFO] Specified chunk size: " + CHUNK_SIZE);
			if (SLOW != 0)
				Console.WriteLine("[INFO] Specified winding_number: " + SLOW);
			if (EXCAVATE)
				Console.WriteLine("[INFO] Enabled option: excavate");
			if (COLOR)
				Console.WriteLine("[INFO] Enabled option: color");
			if (HEIGHT_MAP != 1)
				Console.WriteLine("[INFO] Enabled option: heightmap (value=" + HEIGHT_MAP + ")");
			if (SHADER_FIX_HOLES)
				Console.WriteLine("[INFO] Enabled shader: fix-holes");
			if (SHADER_FIX_LONELY)
				Console.WriteLine("[INFO] Enabled shader: fix-lonely");
			if (SHADER_CASE != 0)
				Console.WriteLine("[INFO] Enabled shader: case (" + SHADER_CASE + " iterations)");
			if (SLICE)
				Console.WriteLine("[INFO] Enabled option: slice");
			if (DEBUG)
				Console.WriteLine("[INFO] Enabled option: debug");

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
					throw new FileNotFoundException("[ERROR] Input file not found", INPUT_PATH);
				}
			}
			else
			{
				isFolder = true;
			}
			try
			{
				AbstractToSchematic converter;
				if (isFolder)
				{
					if (SLICE)
					{
						converter = new FolderImageToSchematic(path, EXCAVATE, INPUT_COLOR_FILE, COLOR_LIMIT);
						return SchematicToVox(converter, OUTPUT_PATH);
					}

					string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(t => Path.GetExtension(t).ToLower() != ".vox").ToArray();
					Console.WriteLine("[INFO] Processing folder " + files.Length + " files");
					for (int i = 0; i < files.Length; i++)
					{
						string file = files[i];
						Console.WriteLine("[INFO] Processing file: " + file);
						converter = GetConverter(file);
						if (converter != null)
						{
							SchematicToVox(converter, Path.Combine(OUTPUT_PATH, Path.GetFileNameWithoutExtension(file)));
						}
						else
						{
							Console.WriteLine("[ERROR] Unsupported file extension !");
						}
					}
				}
				else
				{
					converter = GetConverter(path);
					if (converter != null)
					{
						return SchematicToVox(converter, OUTPUT_PATH);
					}

					Console.WriteLine("[ERROR] Unsupported file extension !");
				}
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
				case ".obj":
					return new OBJToSchematic(path, GRID_SIZE, EXCAVATE, SLOW);
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
				default:
					return null;
			}
		}

		private static bool SchematicToVox(AbstractToSchematic converter, string outputPath)
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
				PaletteSchematicConverter converterPalette = new PaletteSchematicConverter(INPUT_PALETTE_FILE, COLOR_LIMIT);
				schematic = converterPalette.ConvertSchematic(schematic);
				return writer.WriteModel(CHUNK_SIZE, outputPath + ".vox", converterPalette.GetPalette(), schematic);
			}

			if (SHADER_FIX_HOLES)
			{
				schematic = ShaderUtils.ApplyShader(schematic, ShaderUtils.SHADER_FIX_HOLES_KEY);
			}

			if (SHADER_FIX_LONELY)
			{
				schematic = ShaderUtils.ApplyShader(schematic, ShaderUtils.SHADER_FIX_LONELY_KEY);
			}

			if (SHADER_CASE != 0)
			{
				schematic = ShaderUtils.ApplyShader(schematic, ShaderUtils.SHADER_CASE_KEY, SHADER_CASE);
			}

			return writer.WriteModel(CHUNK_SIZE, outputPath + ".vox", null, schematic);
		}

		private static void ShowHelp(OptionSet p)
		{
			Console.WriteLine("Usage: FileToVox --i INPUT --o OUTPUT");
			Console.WriteLine("Options: ");
			p.WriteOptionDescriptions(Console.Out);
		}

		private static void CheckVerbose()
		{
			if (DEBUG)
			{
				VoxReader reader = new VoxReader();
				reader.LoadModel(OUTPUT_PATH + ".vox");
			}
		}
	}
}
