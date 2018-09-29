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
            VoxParser voxParser = new VoxParser();
            VoxModel voxModel = new VoxModel();

            voxParser.LoadModel("../../models/1x1x1.vox", voxModel);

            Console.WriteLine("");
            Console.WriteLine("VOXEL MODEL COUNT FRAMES: " + voxModel.voxelFrames.Count);
            Console.ReadKey();
        }
    }
}
