using System.Drawing;

namespace FileToVoxCore.Extensions
{
	public static class FctExtensions
    {
        public static Color UIntToColor(this uint color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }

        public static uint ColorToUInt(this Color color)
        {
	        return (uint)((color.A << 24) | (color.R << 16) |
	                      (color.G << 8) | (color.B << 0));
        }

    }

	
}
