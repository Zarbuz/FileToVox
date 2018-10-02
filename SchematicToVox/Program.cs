using SchematicToVox.Vox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchematicToVox
{
    class Program
    {
        static void Main(string[] args)
        {
            VoxReader reader = new VoxReader();
            VoxModel voxModel = new VoxModel();
            reader.LoadModel("../../exports/export.vox", voxModel);

            //var schematic = SchematicReader.SchematicReader.LoadSchematic("../../schematics/gateway.schematic");
            //VoxWriter writer = new VoxWriter();
            //writer.WriteModel("../../exports/" + "export" + ".vox", schematic);
            Console.ReadLine();
        }

    }
}

