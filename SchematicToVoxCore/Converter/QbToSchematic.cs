using SchematicToVoxCore.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using FileToVox.Schematics;
using FileToVox.Utils;
using Motvin.Collections;

namespace FileToVox.Converter
{
    public class QBToSchematic : AbstractToSchematic
    {
        private const int CodeFlag = 2;
        private const int Nextsliceflag = 6;

        public QBToSchematic(string path) : base(path)
        {

        }

        public override Schematic WriteSchematic()
        {
            List<VoxelDTO> voxels = LoadVoxels();
            return Convert(voxels);
        }

        private List<VoxelDTO> LoadVoxels()
        {
            List<VoxelDTO> voxels = new List<VoxelDTO>();
            using (FileStream fs = File.OpenRead(_path))
            {
                BinaryReader reader = new BinaryReader(fs);
                uint version = reader.ReadUInt32();
                uint colorFormat = reader.ReadUInt32();
                uint zAxisOrientation = reader.ReadUInt32();
                uint compressed = reader.ReadUInt32();
                uint visibilityMaskEncoded = reader.ReadUInt32();
                uint numMatrices = reader.ReadUInt32();

                for (int i = 0; i < numMatrices; i++)
                {
                    //Read matrix name
                    byte nameLength = reader.ReadByte();
                    string name = Encoding.UTF8.GetString(reader.ReadBytes(nameLength));

                    //Read matrix size
                    uint sizeX = reader.ReadUInt32();
                    uint sizeY = reader.ReadUInt32();
                    uint sizeZ = reader.ReadUInt32();

                    //Read matrix position
                    int posX = reader.ReadInt32();
                    int posY = reader.ReadInt32();
                    int posZ = reader.ReadInt32();

                    //uint[] matrix = new uint[sizeX * sizeY * sizeZ];
                    //matrixList.Add(matrix);

                    if (compressed == 0)
                    {
                        for (uint z = 0; z < sizeZ; z++)
                        {
                            for (uint y = 0; y < sizeY; y++)
                            {
                                for (uint x = 0; x < sizeX; x++)
                                {
                                    Color data = reader.ReadUInt32().UIntToColor();
                                    if (data.A != 0)
                                    {
                                        voxels.Add(new VoxelDTO(
                                            (int) (zAxisOrientation == 1 ? (z + posZ) : (x + posX)),
                                            (int) y + posY,
                                            (int) (zAxisOrientation == 1 ? (x + posX) : (z + posZ)),
                                            colorFormat == 0 ? data.R : data.B,
                                            data.G,
                                            colorFormat == 0 ? data.B : data.R,
                                            data.A));
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        int z = 0;
                        while (z < sizeZ)
                        {
                            int index = -1;
                            while (true)
                            {
                                uint data = reader.ReadUInt32();
                                if (data == Nextsliceflag)
                                {
                                    break;
                                }
                                else if (data == CodeFlag)
                                {
                                    uint count = reader.ReadUInt32();
                                    data = reader.ReadUInt32();

                                    for (int j = 0; j < count; j++)
                                    {
                                        long x = ((index + 1) % sizeX);
                                        long y = ~~((index + 1) / sizeX);
                                        index++;
                                        Color color = data.UIntToColor();
                                        if (color.A != 0)
                                        {
                                            voxels.Add(new VoxelDTO(
                                                (int)(zAxisOrientation == 1 ? (z + posZ) : (x + posX)),
                                                (int)y,
                                                (int)(zAxisOrientation == 1 ? (x + posX) : (z + posZ)),
                                                colorFormat == 0 ? color.R : color.B,
                                                color.G,
                                                colorFormat == 0 ? color.B : color.R,
                                                color.A));
                                        }
                                    }
                                }
                                else
                                {
                                    long x = ((index + 1) % sizeX);
                                    long y = ~~((index + 1) / sizeX);
                                    index++;
                                    Color color = data.UIntToColor();
                                    if (color.A != 0)
                                    {
                                        voxels.Add(new VoxelDTO(
                                            (int)(zAxisOrientation == 1 ? (z + posZ) : (x + posX)),
                                            (int)y,
                                            (int)(zAxisOrientation == 1 ? (x + posX) : (z + posZ)),
                                            colorFormat == 0 ? color.R : color.B,
                                            color.G,
                                            colorFormat == 0 ? color.B : color.R,
                                            color.A));
                                    }
                                }
                            }
                            z++;
                        }
                    }
                }

                return voxels;
            }
        }

        private Schematic Convert(List<VoxelDTO> voxels)
        {
            Schematic schematic = new Schematic();
            int minX = voxels.Min(x => x.X);
            int minY = voxels.Min(x => x.Y);
            int minZ = voxels.Min(x => x.Z);
            int width = Math.Abs(voxels.Min(x => x.X) - voxels.Max(x => x.X)) + 1;
            int length = Math.Abs(voxels.Min(x => x.Z) - voxels.Max(x => x.Z)) + 1;
            int height = Math.Abs(voxels.Min(x => x.Y) - voxels.Max(x => x.Y)) + 1;
            schematic.Width = (ushort)width;
            schematic.Heigth = (ushort)height;
            schematic.Length = (ushort)length;
            schematic.Blocks = new HashSet<Block>();

            LoadedSchematic.LengthSchematic = schematic.Length;
            LoadedSchematic.HeightSchematic = schematic.Heigth;
            LoadedSchematic.WidthSchematic = schematic.Width;

            Console.WriteLine("[LOG] Started to write schematic from qb...");
            Console.WriteLine("[INFO] Qb Width: " + schematic.Width);
            Console.WriteLine("[INFO] Qb Length: " + schematic.Length);
            Console.WriteLine("[INFO] Qb Height: " + schematic.Heigth);
            using (ProgressBar progressbar = new ProgressBar())
            {
                for (var index = 0; index < voxels.Count; index++)
                {
                    VoxelDTO voxel = voxels[index];
                    ushort x = (ushort) (minX < 0 ? voxel.X + Math.Abs(minX) : voxel.X);
                    ushort y = (ushort) (minY < 0 ? voxel.Y + Math.Abs(minY) : voxel.Y);
                    ushort z = (ushort) (minZ < 0 ? voxel.Z + Math.Abs(minZ) : voxel.Z);
                    schematic.Blocks.Add(new Block(x, y, z,
                        FctExtensions.ByteArrayToUInt(voxel.R, voxel.G, voxel.B, voxel.A)));
                    progressbar.Report((index / (float)voxels.Count));
                }
            }
            Console.WriteLine("[LOG] Done.");

            return schematic;
        }
    }

    public class VoxelDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }

        public byte R { get; set; }
        public byte G { get; set; }
        public byte B { get; set; }
        public byte A { get; set; }

        public VoxelDTO(int x, int y, int z, byte r, byte g, byte b, byte a)
        {
            X = x;
            Y = y;
            Z = z;
            R = r;
            G = g;
            B = b;
            A = a;
        }
    }
}
