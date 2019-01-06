using NDesk.Options;
using SchematicToVox.Schematics;
using SchematicToVox.Vox;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox
{
    class Program
    {
        private static string _inputFile = null;
        private static string _outputDir = null;

        private static bool _show_help = false;
        private static bool _verbose = false;
        private static bool _excavate = false;
        private static bool _color = false;

        private static int _ignore_min_y = -1;
        private static int _ignore_max_y = 256;
        private static int _scale = 1;
        private static int _direction = 0;
        private static int _heightmap = 1;

        static void Main(string[] args)
        {
            OptionSet options = new OptionSet() {
                { "i|input=", "input file", v => _inputFile = v },
                { "o|output=", "output file", v => _outputDir = v },
                { "h|help", "show this message and exit", v => _show_help = v != null },
                { "v|verbose", "enable the verbose mode", v => _verbose = v != null },
                { "w|way=", "the way of schematic (0 or 1), default value is 0", (int v) => _direction = v },
                { "iminy|ignore-min-y=", "ignore blocks below the specified layer", (int v) => _ignore_min_y = v },
                { "imaxy|ignore-max-y=", "ignore blocks above the specified layer", (int v) => _ignore_max_y = v },
                { "e|excavate", "delete all blocks which doesn't have at lease one face connected with air", v => _excavate = v != null },
                { "s|scale=", "increase the scale of each block", (int v) => _scale = v },
                { "hm|heightmap=", "create voxels terrain from heightmap", (int v)=> _heightmap = v },
                { "c|color", "enable color when generating heightmap", v => _color = v != null }
            };

            try
            {
                List<string> extra = options.Parse(args);
                CheckHelp(options);
                CheckArguments();
                DisplayArguments();
                ProcessFile();
                CheckVerbose();

                Console.WriteLine("[LOG] Done.");

            }
            catch (OptionException e)
            {
                Console.Write("SchematicToVox: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `SchematicToVox --help` for more informations.");
            }

            if (_verbose)
                Console.ReadKey();
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
                throw new ArgumentNullException("[ERROR] Missing required option: --i=FILE");
            if (_outputDir == null)
                throw new ArgumentNullException("[ERROR] Missing required option: --o=FILE");
            if (_ignore_min_y < -1)
                throw new ArgumentException("[ERROR] --ignore-min-y argument must be positive");
            if (_ignore_max_y > 256)
                throw new ArgumentException("[ERROR] --ignore-max-y argument must be lower than 256");
            if (_scale <= 0)
                throw new ArgumentException("[ERROR] --scale argument must be positive");
            if (_heightmap <= 1)
                throw new ArgumentException("[ERROR] --heightmap argument must be positive");
            if (_color && _heightmap == 1)
                throw new ArgumentException("[ERROR] --color argument must be used with --heightmap");
        }

        private static void DisplayArguments()
        {
            if (_ignore_min_y != -1)
                Console.WriteLine("[INFO] Specified min Y layer : " + _ignore_min_y);
            if (_ignore_max_y != 256)
                Console.WriteLine("[INFO] Specified max Y layer : " + _ignore_max_y);
            if (_excavate)
                Console.WriteLine("[INFO] Enabled option: excavate");
            if (_color)
                Console.WriteLine("[INFO] Enabled option: color");
            if (_heightmap != 1)
                Console.WriteLine("[INFO] Enabled option: heightmap (value=" + _heightmap + ")");
            if (_scale > 1)
                Console.WriteLine("[INFO] Specified increase size: " + _scale);
            Console.WriteLine("[INFO] Way: " + _direction);

            Console.WriteLine("[INFO] Specified output path: " + Path.GetFullPath(_outputDir));
        }

        private static void ProcessFile()
        {
            switch (Path.GetExtension(_inputFile))
            {
                case ".schematic":
                    ProcessSchematicFile();
                    break;
                case ".png":
                    ProcessImageFile();
                    break;
                default:
                    Console.WriteLine("[ERROR] Unknown file extension ! ");
                    Console.ReadKey();
                    return;
            }
        }

        private static void ProcessSchematicFile()
        {
            var schematic = SchematicReader.LoadSchematic(_inputFile, _ignore_min_y, _ignore_max_y, _excavate, _scale);
            VoxWriter writer = new VoxWriter();
            writer.WriteModel(_outputDir + ".vox", schematic, _direction, true, _scale);
        }

        private static void ProcessImageFile()
        {
            var schematic = SchematicWriter.WriteSchematic(_inputFile, _heightmap, _excavate, _color);
            VoxWriter writer = new VoxWriter();
            writer.WriteModel(_outputDir + ".vox", schematic, _direction, false, _scale);
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: SchematicToVox [OPTIONS]+ INPUT OUTPUT");
            Console.WriteLine("Options: ");
            p.WriteOptionDescriptions(Console.Out);
        }

        private static void CheckVerbose()
        {
            if (_verbose)
            {
                VoxReader reader = new VoxReader();
                reader.LoadModel(_outputDir + ".vox");
            }
        }
    }
}

