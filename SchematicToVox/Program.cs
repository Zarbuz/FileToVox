using NDesk.Options;
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

        private static int _ignore_min_y = -1;
        private static int _ignore_max_y = 256;
        private static int _scale = 1;
        private static int _direction = 0;

        static void Main(string[] args)
        {
            var p = new OptionSet() {
                { "i|input=", "the {NAME} of input schematic file.", v => _inputFile = v },
                { "o|output=", "the {NAME} of output directory.", v => _outputDir = v },
                { "h|help", "show this message and exit", v => _show_help = v != null },
                { "v|verbose", "enable the verbose mode", v => _verbose = v != null },
                { "w|way=", "the way of schematic (0 or 1), default value is 0", (int v) => _direction = v },
                { "iminy|ignore-min-y=", "Ignore blocks below the specified layer", (int v) => _ignore_min_y = v },
                { "imaxy|ignore-max-y=", "Ignore blocks above the specified layer", (int v) => _ignore_max_y = v },
                { "e|excavate", "Delete all blocks which doesn't have at lease one face connected with air", v => _excavate = v != null },
                { "s|scale=", "Increase the scale of each block", (int v) => _scale = v }
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);

                if (_show_help)
                {
                    ShowHelp(p);
                    return;
                }

                if (_inputFile == null)
                    throw new ArgumentNullException("Missing required option -i=FILE");
                if (_outputDir == null)
                    throw new ArgumentNullException("Missing required option -o=FILE");
                if (_ignore_min_y < -1)
                    throw new ArgumentException("ignore-min-y argument must be positive");
                if (_ignore_max_y > 256)
                    throw new ArgumentException("ignore-max-y argument must be lower than 256");
                if (_scale <= 0)
                    throw new ArgumentException("scale must be greater than 0");

                if (_ignore_min_y != -1)
                    Console.WriteLine("Specified min Y layer : " + _ignore_min_y);
                if (_ignore_max_y != 256)
                    Console.WriteLine("Specified max Y layer : " + _ignore_max_y);
                if (_excavate)
                    Console.WriteLine("Enabled option: excavate");
                if (_scale > 1)
                    Console.WriteLine("Specified increase size : " + _scale);

                string extension = Path.GetExtension(_inputFile);
                Console.WriteLine("Specified output path: " + Path.GetFullPath(_outputDir));

                switch (extension)
                {
                    case ".schematic":
                        ProcessSchematicFile();
                        break;
                    case ".png":
                        ProcessImageFile();
                        break;
                    default:
                        Console.WriteLine("Unknown file extension ! ");
                        Console.ReadKey();
                        return;
                }

                if (_verbose)
                {
                    VoxReader reader = new VoxReader();
                    reader.LoadModel(_outputDir + ".vox");
                }

                Console.WriteLine("Done");

            }
            catch (OptionException e)
            {
                Console.Write("SchematicToVox: ");
                Console.WriteLine(e.Message);
                Console.WriteLine("Try `SchematicToVox --help` for more informations.");
            }


            Console.ReadKey();
        }

        private static void ProcessSchematicFile()
        {
            var schematic = SchematicReader.SchematicReader.LoadSchematic(_inputFile, _ignore_min_y, _ignore_max_y, _excavate, _scale);
            VoxWriter writer = new VoxWriter();
            writer.WriteModel(_outputDir + ".vox", schematic, _direction, true, _scale);
        }

        private static void ProcessImageFile()
        {
            var schematic = SchematicReader.SchematicWriter.WriteSchematic(_inputFile);
            VoxWriter writer = new VoxWriter();
            writer.WriteModel(_outputDir + ".vox", schematic, _direction, false, _scale);
        }

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: SchematicToVox [OPTIONS]+ INPUT OUTPUT");
            Console.WriteLine("Options: ");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}

