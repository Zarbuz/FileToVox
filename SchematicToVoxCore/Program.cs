using System;
using System.Collections.Generic;
using System.IO;
using FileToVox.Converter;
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

        private static float _slow = 0.0f;

        private static int _ignoreMinY = -1;
        private static int _ignoreMaxY = 256;
        private static int _scale = 1;
        private static int _direction = 0;
        private static int _heightmap = 1;
        private static int _gridSize = 126;

        public static void Main(string[] args)
        {
            OptionSet options = new OptionSet()
            {
                {"i|input=", "input file", v => _inputFile = v},
                {"o|output=", "output file", v => _outputFile = v},
                {"h|help", "show this message and exit", v => _show_help = v != null},
                {"v|verbose", "enable the verbose mode", v => _verbose = v != null},
                {"w|way=", "the way of schematic (0 or 1), default value is 0", (int v) => _direction = v},
                {"iminy|ignore-min-y=", "ignore blocks below the specified layer (only schematic file)", (int v) => _ignoreMinY = v},
                {"imaxy|ignore-max-y=", "ignore blocks above the specified layer (only schematic file)", (int v) => _ignoreMaxY = v},
                {
                    "e|excavate", "delete all blocks which doesn't have at lease one face connected with air (only schematic file)",
                    v => _excavate = v != null
                },
                {"s|scale=", "increase the scale of each block (only schematic file)", (int v) => _scale = v},
                {"hm|heightmap=", "create voxels terrain from heightmap (only for PNG file)", (int v) => _heightmap = v},
                {"c|color", "enable color when generating heightmap (only for PNG file)", v => _color = v != null},
                {"t|top", "create voxels only for top (only for PNG file)", v => _top = v != null},
                {"cm|color-from-file=", "load colors from file", v => _inputColorFile = v },
                {"gs|grid-size=", "set the grid size (only for OBJ file)", (int v) => _gridSize = v },
                {"slow=", "use a slower algorithm (use all cores) to generate voxels from OBJ but best result (value should be enter 0.0 and 1.0 (0.5 is recommanded)", (float v) => _slow = v }
            };

            try
            {
                List<string> extra = (args.Length > 0) ? options.Parse(args) : options.Parse(CheckArgumentsFile());
                CheckHelp(options);
                CheckArguments();
                DisplayArguments();
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
            if (_color && _heightmap == 1)
                throw new ArgumentException("[ERROR] --color argument must be used with --heightmap");
        }

        private static void DisplayArguments()
        {
            if (_inputFile != null)
                Console.WriteLine("[INFO] Specified input file: " + _inputFile);
            if (_outputFile != null)
                Console.WriteLine("[INFO] Specifid output file: " + _outputFile);
            if (_inputColorFile != null)
                Console.WriteLine("[INFO] Specified input color file: " + _inputColorFile);
            if (_ignoreMinY != -1)
                Console.WriteLine("[INFO] Specified min Y layer : " + _ignoreMinY);
            if (_ignoreMaxY != 256)
                Console.WriteLine("[INFO] Specified max Y layer : " + _ignoreMaxY);
            if (_scale > 1)
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



            Console.WriteLine("[INFO] Way: " + _direction);
            Console.WriteLine("[INFO] Specified output path: " + Path.GetFullPath(_outputFile));
        }

        private static void ProcessFile()
        {
            if (!File.Exists(_inputFile))
                throw new FileNotFoundException("[ERROR] Input file not found", _inputFile);

            switch (Path.GetExtension(_inputFile))
            {
                case ".schematic":
                    ProcessSchematicFile();
                    break;
                case ".png":
                    ProcessImageFile();
                    break;
                case ".asc":
                    ProcessAscFile();
                    break;
                case ".binvox":
                    ProcessBinvoxFile();
                    break;
                case ".qb":
                    ProcessQbFile();
                    break;
                case ".obj":
                    ProcessObjFile();
                    break;
                default:
                    Console.WriteLine("[ERROR] Unknown file extension !");
                    Console.ReadKey();
                    return;
            }
        }



        private static void ProcessSchematicFile()
        {
            try
            {
                Schematic schematic = SchematicReader.LoadSchematic(_inputFile, _ignoreMinY, _ignoreMaxY, _excavate, _scale);
                VoxWriter writer = new VoxWriter();
                writer.WriteModel(_outputFile + ".vox", schematic, _direction);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        private static void ProcessBinvoxFile()
        {
            try
            {
                BinvoxToSchematic converter = new BinvoxToSchematic();
                Schematic schematic = converter.WriteSchematic(_inputFile);
                VoxWriter writer = new VoxWriter();
                writer.WriteModel(_outputFile + ".vox", schematic, _direction);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        private static void ProcessQbFile()
        {
            try
            {
                QbToSchematic converter = new QbToSchematic();
                Schematic schematic = converter.WriteSchematic(_inputFile);
                VoxWriter writer = new VoxWriter();
                writer.WriteModel(_outputFile + ".vox", schematic, _direction);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        private static void ProcessImageFile()
        {
            try
            {
                PNGToSchematic converter = new PNGToSchematic(_inputColorFile, _heightmap, _excavate, _color, _top);
                Schematic schematic = converter.WriteSchematic(_inputFile);
                VoxWriter writer = new VoxWriter();
                writer.WriteModel(_outputFile + ".vox", schematic, _direction);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        private static void ProcessAscFile()
        {
            try
            {
                ASCToSchematic converter = new ASCToSchematic();
                Schematic schematic = converter.WriteSchematic(_inputFile);
                VoxWriter writer = new VoxWriter();
                writer.WriteModel(_outputFile + ".vox", schematic, _direction);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadLine();
            }
        }

        private static void ProcessObjFile()
        {
            try
            {
                ObjToSchematic converter = new ObjToSchematic(_gridSize, _excavate, _slow);
                Schematic schematic = converter.WriteSchematic(_inputFile);
                VoxWriter writer = new VoxWriter();
                writer.WriteModel(_outputFile + ".vox", schematic, _direction);
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
