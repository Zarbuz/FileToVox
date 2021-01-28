using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileToVox.Converter;
using FileToVox.Converter.Image;
using FileToVox.Converter.PointCloud;
using FileToVox.Schematics;
using FileToVox.Vox;
using NDesk.Options;

namespace FileToVox
{
	class Program
	{
		private static string _inputPath;
		private static string _outputPath;
		private static string _inputColorFile;
		private static string _inputPaletteFile;

		private static bool _showHelp;
		private static bool _verbose;
		private static bool _excavate;
		private static bool _color;
		private static bool _top;
		private static bool _flood;
		private static bool _holes;
		private static bool _lonely;
		private static bool _slice;

		private static float _slow;

		private static int _ignoreMinY = -1;
		private static int _ignoreMaxY = 256;
		private static float _scale = 1;
		private static int _heightMap = 1;
		private static int _gridSize = 126;
		private static int _colorLimit = 256;
		private static int _chunkSize = 125;

		private const int MAX_WORLD_WIDTH = 2001;
		private const int MAX_WORLD_HEIGHT = 2001;
		private const int MAX_WORLD_LENGTH = 2001;

		public static void Main(string[] args)
		{
			OptionSet options = new OptionSet()
			{
				{"i|input=", "input path", v => _inputPath = v},
				{"o|output=", "output path", v => _outputPath = v},
				{"c|color", "enable color when generating heightmap", v => _color = v != null},
				{"cm|color-from-file=", "load colors from file", v => _inputColorFile = v },
				{"cl|color-limit=", "set the maximal number of colors for the palette", (int v) => _colorLimit =v },
				{"cs|chunk-size=", "set the chunk size", (int v) => _chunkSize = v},
				{"e|excavate", "delete all voxels which doesn't have at least one face connected with air",  v => _excavate = v != null },
				{"fl|flood", "fill all invisible voxels", v => _flood = v != null },
				{"flo|fix-lonely", "delete all voxels where all connected voxels are air", v => _lonely = v != null },
				{"fh|fix-holes", "fix holes", v => _holes = v != null },
				{"gs|grid-size=", "set the grid size", (int v) => _gridSize = v },
				{"h|help", "help informations", v => _showHelp = v != null},
				{"hm|heightmap=", "create voxels terrain from heightmap (only for PNG file)", (int v) => _heightMap = v},
				{"iminy|ignore-min-y=", "ignore voxels below the specified layer", (int v) => _ignoreMinY = v},
				{"imaxy|ignore-max-y=", "ignore voxels above the specified layer", (int v) => _ignoreMaxY = v},
				{"p|palette=", "set the palette", v => _inputPaletteFile = v },
				{"sc|scale=", "set the scale", (float v) => _scale = v},
				{"si|slice", "indicate that each picture is a slice", v => _slice = v != null},
				{"sl|slow=", "use a slower algorithm (use all cores) to generate voxels from OBJ but best result (value should be enter 0.0 and 1.0 (0.5 is recommended)", (float v) => _slow = v },
				{"t|top", "create voxels only at the top of the heightmap", v => _top = v != null},
				{"v|verbose", "enable the verbose mode", v => _verbose = v != null},
			};

			try
			{
				List<string> extra = (args.Length > 0) ? options.Parse(args) : options.Parse(CheckArgumentsFile());
				CheckHelp(options);
				CheckArguments();
				DisplayArguments();

				if (_inputPath != null)
					ProcessFile();
				CheckVerbose();
				Console.WriteLine("[LOG] Done.");
				if (_verbose)
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
			if (_showHelp)
			{
				ShowHelp(options);
				Environment.Exit(0);
			}
		}

		private static void CheckArguments()
		{
			if (_inputPath == null)
				throw new ArgumentNullException("[ERROR] Missing required option: --i");
			if (_outputPath == null)
				throw new ArgumentNullException("[ERROR] Missing required option: --o");
			if (_ignoreMinY < -1)
				throw new ArgumentException("[ERROR] --ignore-min-y argument must be positive");
			if (_ignoreMaxY > 256)
				throw new ArgumentException("[ERROR] --ignore-max-y argument must be lower than 256");
			if (_scale <= 0)
				throw new ArgumentException("[ERROR] --scale argument must be positive");
			if (_heightMap < 1)
				throw new ArgumentException("[ERROR] --heightmap argument must be positive");
			if (_colorLimit < 0)
				throw new ArgumentException("[ERROR] --color-limit argument must be positive");
			if (_colorLimit > 256)
				throw new ArgumentException("[ERROR] --color-limit argument must be lower than 256");
			if (_chunkSize <= 10 || _chunkSize > 256)
				throw new ArgumentException("[ERROR] --chunk-size argument must be lower than 256 and greater than 10");
		}

		private static void DisplayArguments()
		{
			if (_inputPath != null)
				Console.WriteLine("[INFO] Specified input path: " + _inputPath);
			if (_outputPath != null)
				Console.WriteLine("[INFO] Specified output path: " + _outputPath);
			if (_inputColorFile != null)
				Console.WriteLine("[INFO] Specified input color file: " + _inputColorFile);
			if (_inputPaletteFile != null)
				Console.WriteLine("[INFO] Specified palette file: " + _inputPaletteFile);
			if (_ignoreMinY != -1)
				Console.WriteLine("[INFO] Specified min Y layer : " + _ignoreMinY);
			if (_ignoreMaxY != 256)
				Console.WriteLine("[INFO] Specified max Y layer : " + _ignoreMaxY);
			if (_colorLimit != 256)
				Console.WriteLine("[INFO] Specified color limit: " + _colorLimit);
			if (_scale != 0)
				Console.WriteLine("[INFO] Specified increase size: " + _scale);
			if (_gridSize != 126)
				Console.WriteLine("[INFO] Specified grid size: " + _gridSize);
			if (_chunkSize != 125)
				Console.WriteLine("[INFO] Specified chunk size: " + _chunkSize);
			if (_slow != 0)
				Console.WriteLine("[INFO] Specified winding_number: " + _slow);
			if (_excavate)
				Console.WriteLine("[INFO] Enabled option: excavate");
			if (_color)
				Console.WriteLine("[INFO] Enabled option: color");
			if (_heightMap != 1)
				Console.WriteLine("[INFO] Enabled option: heightmap (value=" + _heightMap + ")");
			if (_top)
				Console.WriteLine("[INFO] Enabled option: top");
			if (_flood)
				Console.WriteLine("[INFO] Enabled option: flood");
			if (_holes)
				Console.WriteLine("[INFO] Enabled option: fix-holes");
			if (_lonely)
				Console.WriteLine("[INFO] Enabled option: fix-lonely");
			if (_slice)
				Console.WriteLine("[INFO] Enabled option: slice");

			Console.WriteLine("[INFO] Specified output path: " + Path.GetFullPath(_outputPath));
		}

		private static void ProcessFile()
		{
			string path = Path.GetFullPath(_inputPath);
			bool isFolder = false;
			if (!Directory.Exists(path))
			{
				if (!File.Exists(path))
				{
					throw new FileNotFoundException("[ERROR] Input file not found", _inputPath);
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
					if (_slice)
					{
						converter = new FolderImageToSchematic(path, _excavate, _inputColorFile, _colorLimit);
						SchematicToVox(converter, _outputPath);
					}
					else
					{
						string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(t => Path.GetExtension(t).ToLower() != ".vox").ToArray();
						Console.WriteLine("[INFO] Processing folder " + files.Length + " files");
						for (int i = 0; i < files.Length; i++)
						{
							string file = files[i];
							Console.WriteLine("[INFO] Processing file: " + file);
							converter = GetConverter(file);
							if (converter != null)
							{
								SchematicToVox(converter, Path.Combine(_outputPath, Path.GetFileNameWithoutExtension(file)));
							}
							else
							{
								Console.WriteLine("[ERROR] Unsupported file extension !");
							}
						}
					}
				}
				else
				{
					converter = GetConverter(path);
					if (converter != null)
					{
						SchematicToVox(converter, _outputPath);
					}
					else
					{
						Console.WriteLine("[ERROR] Unsupported file extension !");
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Console.ReadLine();
			}

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
					return new CSVToSchematic(path, _scale, _colorLimit, _holes, _flood, _lonely);
				case ".obj":
					return new OBJToSchematic(path, _gridSize, _excavate, _slow);
				case ".ply":
					return new PLYToSchematic(path, _scale, _colorLimit, _holes, _flood, _lonely);
				case ".png":
					return new PNGToSchematic(path, _inputColorFile, _heightMap, _excavate, _color, _top, _colorLimit);
				case ".qb":
					return new QBToSchematic(path);
				case ".schematic":
					return new SchematicToSchematic(path, _ignoreMinY, _ignoreMaxY, _excavate, _scale);
				case ".tif":
					return new TIFtoSchematic(path, _inputColorFile, _heightMap, _excavate, _color, _top, _colorLimit);
				case ".xyz":
					return new XYZToSchematic(path, _scale, _colorLimit, _holes, _flood, _lonely);
				default:
					return null;
			}
		}

		private static void SchematicToVox(AbstractToSchematic converter, string outputPath)
		{
			Schematic schematic = converter.WriteSchematic();
			Console.WriteLine($"[INFO] Vox Width: {schematic.Width}");
			Console.WriteLine($"[INFO] Vox Length: {schematic.Length}");
			Console.WriteLine($"[INFO] Vox Height: {schematic.Height}");

			if (schematic.Width > MAX_WORLD_WIDTH || schematic.Length > MAX_WORLD_LENGTH || schematic.Height > MAX_WORLD_HEIGHT)
			{
				Console.WriteLine("[ERROR] Voxel model is too big ! MagicaVoxel can't support model bigger than 2000^3");
				return;
			}

			VoxWriter writer = new VoxWriter();

			if (_inputPaletteFile != null)
			{
				PaletteSchematicConverter converterPalette = new PaletteSchematicConverter(_inputPaletteFile, _colorLimit);
				schematic = converterPalette.ConvertSchematic(schematic);
				writer.WriteModel(_chunkSize, outputPath + ".vox", converterPalette.GetPalette(), schematic);
			}
			else
			{
				writer.WriteModel(_chunkSize, outputPath + ".vox", null, schematic);
			}
		}

		private static void ShowHelp(OptionSet p)
		{
			Console.WriteLine("Usage: FileToVox --i INPUT --o OUTPUT");
			Console.WriteLine("Options: ");
			p.WriteOptionDescriptions(Console.Out);
		}

		private static void CheckVerbose()
		{
			if (_verbose)
			{
				VoxReader reader = new VoxReader();
				reader.LoadModel(_outputPath + ".vox");
			}
		}
	}
}
