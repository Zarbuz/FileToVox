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
            //VoxReader reader = new VoxReader();
            //VoxModel voxModel = new VoxModel();

            //reader.LoadModel("../../models/126x126x126.vox", voxModel);

            //Console.WriteLine("");
            //Console.WriteLine("VOXEL MODEL COUNT FRAMES: " + voxModel.voxelFrames.Count);

            VoxWriter writer = new VoxWriter();
            var schematic = SchematicReader.SchematicReader.LoadSchematic("../../models/126x126x126.schematic");
            writer.WriteModel("../../exports/test.vox", schematic);
            Console.ReadKey();
        }
    }
}
