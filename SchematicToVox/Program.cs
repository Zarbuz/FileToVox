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
            var path = "../../schematics/gateway.schematic";
            var schematic = SchematicReader.SchematicReader.LoadSchematic(path);
            var name = Path.GetFileNameWithoutExtension(path);

            VoxWriter writer = new VoxWriter();
            VoxReader reader = new VoxReader();

            writer.WriteModel("../../exports/" + name + ".vox", schematic);
            reader.LoadModel("../../exports/" + name + ".vox");

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        
    }
}

