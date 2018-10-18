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
            if (args.Length < 1)
            {
                Console.WriteLine("Missing arguments");
                Console.ReadKey();
                return;
            }

            var path = args[0];
            if (Path.GetExtension(path) != ".schematic")
            {
                Console.WriteLine("File is not a schematic");
                Console.ReadKey();
                return;
            }
            var schematic = SchematicReader.SchematicReader.LoadSchematic(path);
            VoxWriter writer = new VoxWriter();
            VoxReader reader = new VoxReader();

            if (args.Length == 2)
            {
                var outputPath = args[1];
                Console.WriteLine("Specified output path: " + Path.GetFullPath(outputPath));
                writer.WriteModel(outputPath + ".vox", schematic);
                reader.LoadModel(outputPath + ".vox");
            }
            else
            {
                var name = Path.GetFileNameWithoutExtension(path);
                writer.WriteModel(name + ".vox", schematic);
                reader.LoadModel(name + ".vox");
            }

            Console.WriteLine("Done");
            Console.ReadKey();
        }


    }
}

