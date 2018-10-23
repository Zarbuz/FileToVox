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
        static void Main(string[] args)
        {
            string inputFile = null;
            string outputDir = null;
            int direction = 0;
            bool show_help = false;
            bool verbose = false;
            int ignore_min_y = -1;
            int ignore_max_y = 256;

            var p = new OptionSet() {
                { "i|input=", "the {NAME} of input schematic file.", v => inputFile = v },
                { "o|output=", "the {NAME} of output directory.", v => outputDir = v },
                { "h|help", "show this message and exit", v => show_help = v != null },
                { "v|verbose", "enable the verbose mode", v => verbose = v != null },
                { "w|way=", "the way of schematic (0 or 1), default value is 0", (int v) => direction = v },
                { "iminy|ignore-min-y=", "Ignore blocks below the specified layer", (int v) => ignore_min_y = v },
                { "imaxy|ignore-max-y=", "Ignore blocks above the specified layer", (int v) => ignore_max_y = v },
            };

            List<string> extra;
            try
            {
                extra = p.Parse(args);

                if (show_help)
                {
                    ShowHelp(p);
                    return;
                }

                if (inputFile == null)
                    throw new ArgumentNullException("Missing required option -i=FILE");
                if (outputDir == null)
                    throw new ArgumentNullException("Missing required option -o=FILE");
                if (ignore_min_y < -1)
                    throw new ArgumentException("ignore-min-y argument must be positive");
                if (ignore_max_y > 256)
                    throw new ArgumentException("ignore-max-y argument must be lower than 256");

                if (Path.GetExtension(inputFile) != ".schematic")
                {
                    Console.WriteLine("File is not a schematic");
                    Console.ReadKey();
                    return;
                }

                if (ignore_min_y != -1)
                    Console.WriteLine("Specified min Y layer : " + ignore_min_y);
                if (ignore_max_y != 256)
                    Console.WriteLine("Specified max Y layer : " + ignore_max_y);

                var schematic = SchematicReader.SchematicReader.LoadSchematic(inputFile, ignore_min_y, ignore_max_y);
                VoxWriter writer = new VoxWriter();

                Console.WriteLine("Specified output path: " + Path.GetFullPath(outputDir));
                writer.WriteModel(outputDir + ".vox", schematic, direction);

                if (verbose)
                {
                    VoxReader reader = new VoxReader();
                    reader.LoadModel(outputDir + ".vox");
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

        private static void ShowHelp(OptionSet p)
        {
            Console.WriteLine("Usage: SchematicToVox [OPTIONS]+ INPUT OUTPUT");
            Console.WriteLine("Options: ");
            p.WriteOptionDescriptions(Console.Out);
        }
    }
}

