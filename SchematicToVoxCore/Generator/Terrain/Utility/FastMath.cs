using System.Runtime.CompilerServices;

namespace FileToVox.Generator.Terrain.Utility
{
	public static class FastMath
	{
		[MethodImpl(256)]
		public static int FloorToInt(float n)
		{
			return (int)(n + 1000000d) - 1000000;
		}

		public static void FloorToInt(float x, float y, float z, out int ix, out int iy, out int iz)
		{
			ix = (int)(x + 1000000d) - 1000000;
			iy = (int)(y + 1000000d) - 1000000;
			iz = (int)(z + 1000000d) - 1000000;
		}
	}
}
