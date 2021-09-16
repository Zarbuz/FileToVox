namespace FileToVoxCore.Utils
{
	public static class MathUtils
	{
		/// <summary>
		/// Computes standard mathematical modulo (as opposed to remainder).
		/// </summary>
		/// <param name="value">The value.</param>
		/// <param name="divisor">The divisor.</param>
		/// <returns>A value between 0 and divisor. The result will have the same sign as divisor.</returns>
		public static float Mod(float value, float divisor)
		{
			return ((value % divisor) + divisor) % divisor;
		}
    }
}
