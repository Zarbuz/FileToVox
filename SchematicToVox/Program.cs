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

            var p = new OptionSet() {
                { "i|input=", "the {NAME} of input schematic file.", v => inputFile = v },
                { "o|output=", "the {NAME} of output directory.", v => outputDir = v },
                { "h|help", "show this message and exit", v => show_help = v != null },
                { "v|verbose", "enable the verbose mode", v => verbose = v != null },
                { "w|way=", "the way of schematic (0 or 1), default value is 0", (int v) => direction = v }
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
                    throw new InvalidOperationException("Missing required option -i=FILE");
                if (outputDir == null)
                    throw new InvalidOperationException("Missing required option -o=FILE");

                if (Path.GetExtension(inputFile) != ".schematic")
                {
                    Console.WriteLine("File is not a schematic");
                    Console.ReadKey();
                    return;
                }

                var schematic = SchematicReader.SchematicReader.LoadSchematic(inputFile);
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

