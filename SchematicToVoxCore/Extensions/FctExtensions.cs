using ImageMagick;
using Color = FileToVoxCore.Drawing.Color;

namespace FileToVox.Extensions
{
	public static class FctExtensions
    {
		public static byte ParsedChannel(this ushort channel)
		{
			return (byte)((channel / (float)ushort.MaxValue) * 255);
		}

		public static System.Drawing.Color GetPixelColor(this IPixel<ushort> pixel)
		{
			Color color = Color.FromArgb(pixel.GetChannel(3).ParsedChannel(), pixel.GetChannel(0).ParsedChannel(), pixel.GetChannel(1).ParsedChannel(), pixel.GetChannel(2).ParsedChannel());
			return color.ToSystemDrawingColor();
		}

		public static uint ColorToUInt(this Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) |
                          (color.G << 8) | (color.B << 0));
        }

        public static uint ColorToUInt(this System.Drawing.Color color)
        {
	        return (uint)((color.A << 24) | (color.R << 16) |
	                      (color.G << 8) | (color.B << 0));
        }

        public static Color ToFileToVoxCoreColor(this System.Drawing.Color color)
        {
	        return Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static System.Drawing.Color ToSystemDrawingColor(this Color color)
        {
	        return System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
        }

        public static uint ByteArrayToUInt(byte r, byte g, byte b, byte a)
        {
            return (uint)((a << 24) | (r << 16) | (g << 8) | (b << 0));
        }

        public static Color UIntToColor(this uint color)
        {
            byte a = (byte)(color >> 24);
            byte r = (byte)(color >> 16);
            byte g = (byte)(color >> 8);
            byte b = (byte)(color >> 0);
            return Color.FromArgb(a, r, g, b);
        }


    }

	
}
