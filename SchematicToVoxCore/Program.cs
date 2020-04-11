﻿using System;
using System.Collections.Generic;
using System.IO;
using FileToVox.CA;
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
        private static string _inputFile;
        private static string _outputFile;
        private static string _inputColorFile;

        private static bool _show_help;
        private static bool _verbose;
        private static bool _excavate;
        private static bool _color;
        private static bool _top;
        private static bool _flood;
        private static bool _holes;

        private static float _slow;

        private static int _ignoreMinY = -1;
        private static int _ignoreMaxY = 256;
        private static float _scale = 1;
        private static int _heightmap = 1;
        private static int _gridSize = 126;
        private static int _colorLimit = 256;

        public static void Main(string[] args)
        {
			OptionSet options = new OptionSet()
			{
				{"i|input=", "input mandatory file", v => _inputFile = v},
				{"o|output=", "output mandatory file", v => _outputFile = v},
				{"c|color", "enable color when generating heightmap", v => _color = v != null},
				{"cm|color-from-file=", "load colors from file", v => _inputColorFile = v },
				{"cl|color-limit=", "set the maximal number of colors for the palette", (int v) => _colorLimit =v },
				{"e|excavate", "delete all voxels which doesn't have at least one face connected with air",  v => _excavate = v != null },
				{"fl|flood", "fill all invisible voxels", v => _flood = v != null },
				{"fh|fix-holes", "fix holes", v => _holes = v != null },
				{"gs|grid-size=", "set the grid size (only for OBJ file)", (int v) => _gridSize = v },
				{"h|help", "help informations", v => _show_help = v != null},
				{"hm|heightmap=", "create voxels terrain from heightmap (only for PNG file)", (int v) => _heightmap = v},
				{"iminy|ignore-min-y=", "ignore voxels below the specified layer", (int v) => _ignoreMinY = v},
				{"imaxy|ignore-max-y=", "ignore voxels above the specified layer", (int v) => _ignoreMaxY = v},
				{"sc|scale=", "set the scale", (float v) => _scale = v},
				{"sl|slow=", "use a slower algorithm (use all cores) to generate voxels from OBJ but best result (value should be enter 0.0 and 1.0 (0.5 is recommended)", (float v) => _slow = v },
				{"t|top", "create voxels only at the top of the heightmap (only for PNG file)", v => _top = v != null},
				{"v|verbose", "enable the verbose mode", v => _verbose = v != null},
			};

            try
            {
                List<string> extra = (args.Length > 0) ? options.Parse(args) : options.Parse(CheckArgumentsFile());
                CheckHelp(options);
                CheckArguments();
                DisplayArguments();

                if (_inputFile != null)
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
            if (_show_help)
            {
                ShowHelp(options);
                Environment.Exit(0);
            }
        }

        private static void CheckArguments()
        {
            if (_inputFile == null)
                throw new ArgumentNullException("[ERROR] Missing required option: --i");
            if (_outputFile == null)
                throw new ArgumentNullException("[ERROR] Missing required option: --o");
            if (_ignoreMinY < -1)
                throw new ArgumentException("[ERROR] --ignore-min-y argument must be positive");
            if (_ignoreMaxY > 256)
                throw new ArgumentException("[ERROR] --ignore-max-y argument must be lower than 256");
            if (_scale <= 0)
                throw new ArgumentException("[ERROR] --scale argument must be positive");
            if (_heightmap < 1)
                throw new ArgumentException("[ERROR] --heightmap argument must be positive");
            if (_colorLimit < 0)
                throw new ArgumentException("[ERROR] --color-limit argument must be positive");
            if (_colorLimit > 256)
                throw new ArgumentException("[ERROR] --color-limit argument must be lower than 256");

        }

        private static void DisplayArguments()
        {
            if (_inputFile != null)
                Console.WriteLine("[INFO] Specified input file: " + _inputFile);
            if (_outputFile != null)
                Console.WriteLine("[INFO] Specified output file: " + _outputFile);
            if (_inputColorFile != null)
                Console.WriteLine("[INFO] Specified input color file: " + _inputColorFile);
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
            if (_slow != 0)
                Console.WriteLine("[INFO] Specified winding_number: " + _slow);
            if (_excavate)
                Console.WriteLine("[INFO] Enabled option: excavate");
            if (_color)
                Console.WriteLine("[INFO] Enabled option: color");
            if (_heightmap != 1)
                Console.WriteLine("[INFO] Enabled option: heightmap (value=" + _heightmap + ")");
            if (_top)
                Console.WriteLine("[INFO] Enabled option: top");
			if (_flood)
				Console.WriteLine("[INFO] Enabled option: flood");
			if (_holes)
				Console.WriteLine("[INFO] Enabled option: fix-holes");

            Console.WriteLine("[INFO] Specified output path: " + Path.GetFullPath(_outputFile));
        }

        private static void ProcessFile()
        {
            string path = Path.GetFullPath(_inputFile);
            bool isFolder = false;
            if (!Directory.Exists(path))
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("[ERROR] Input file not found", _inputFile);
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
                    converter = new FolderImageToSchematic(path, _excavate);
                }
                else
                {
                    switch (Path.GetExtension(_inputFile))
                    {
	                    case ".asc":
		                    converter = new ASCToSchematic(path);
		                    break;
	                    case ".binvox":
		                    converter = new BinvoxToSchematic(path);
		                    break;
	                    case ".csv":
		                    converter = new CSVToSchematic(path, _scale, _colorLimit, _holes, _flood);
		                    break;
	                    case ".obj":
		                    converter = new OBJToSchematic(path, _gridSize, _excavate, _slow);
		                    break;
	                    case ".ply":
		                    converter = new PLYToSchematic(path, _scale, _colorLimit, _holes, _flood);
		                    break;
	                    case ".png":
		                    converter = new PNGToSchematic(path, _inputColorFile, _heightmap, _excavate, _color, _top, _colorLimit);
		                    break;
						case ".qb":
		                    converter = new QBToSchematic(path);
		                    break;
						case ".schematic":
                            converter = new SchematicToSchematic(path, _ignoreMinY, _ignoreMaxY, _excavate, _scale);
                            break;
                        case ".tif":
                            converter = new TIFtoSchematic(path, _inputColorFile, _heightmap, _excavate, _color, _top, _colorLimit);
                            break;
                        case ".xyz":
	                        converter = new XYZToSchematic(path, _scale, _colorLimit, _holes, _flood);
                            break;
                        
                        default:
                            Console.WriteLine("[ERROR] Unknown file extension !");
                            Console.ReadKey();
                            return;
                    }

                }

                Schematic schematic = converter.WriteSchematic();
                Console.WriteLine($"[INFO] Vox Width: {schematic.Width}");
                Console.WriteLine($"[INFO] Vox Length: {schematic.Length}");
                Console.WriteLine($"[INFO] Vox Height: {schematic.Heigth}");

                if (schematic.Width > 2016 || schematic.Length > 2016 || schematic.Heigth > 2016)
                {
					Console.WriteLine("[ERROR] Voxel model is too big ! MagicaVoxel can't support model bigger than 2016^3");
					return;
                }
                VoxWriter writer = new VoxWriter();
                writer.WriteModel(_outputFile + ".vox", schematic);

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
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
                reader.LoadModel(_outputFile + ".vox");
            }
        }
    }
}
