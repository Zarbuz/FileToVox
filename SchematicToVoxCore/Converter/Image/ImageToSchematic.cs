using FileToVox.Schematics;
using System.Runtime.InteropServices;

namespace FileToVox.Converter.Image
{
	public abstract class ImageToSchematic : AbstractToSchematic
    {
        protected readonly bool Excavate;
        protected readonly int MaxHeight;
        protected readonly bool Color;
        protected readonly string ColorPath;
        protected readonly int ColorLimit;

        [StructLayout(LayoutKind.Explicit)]
        public struct RGB
        {
	        // Structure of pixel for a 24 bpp bitmap
	        [FieldOffset(0)] public byte blue;
	        [FieldOffset(1)] public byte green;
	        [FieldOffset(2)] public byte red;
	        [FieldOffset(3)] public byte alpha;
	        [FieldOffset(0)] public int argb;
        }

        protected ImageToSchematic(string path, string colorPath, int height, bool excavate, bool color, int colorLimit) : base(path)
        {
            ColorPath = colorPath;
            MaxHeight = height;
            Excavate = excavate;
            Color = color;
            ColorLimit = colorLimit;
        }

        protected abstract Schematic WriteSchematicMain();
       
    }
}
