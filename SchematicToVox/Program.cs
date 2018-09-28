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

            voxParser.LoadModel("D:\\Documents\\MagicaVoxel\\vox\\test.vox", voxModel);
            Console.WriteLine(voxModel.voxelFrames.Count);
            Console.ReadKey();
        }
    }
}
