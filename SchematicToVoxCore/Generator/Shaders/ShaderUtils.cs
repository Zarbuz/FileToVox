using FileToVox.Schematics;

namespace FileToVox.Generator.Shaders
{
	public static partial class ShaderUtils
	{
		#region PublicMethods

		public static Schematic ApplyShader(Schematic schematic, ShaderStep shaderStep)
		{
			switch (shaderStep.ShaderType)
			{
				case ShaderType.FIX_HOLES:
					return ApplyShaderFillHoles(schematic);
				case ShaderType.FIX_LONELY:
					return ApplyShaderLonely(schematic);
				case ShaderType.CASE:
					return ApplyShaderCase(schematic, shaderStep.Iterations);
				case ShaderType.PATINA:
					return ApplyShaderPatina(schematic, shaderStep);
			}

			return schematic;
		}

		

		#endregion

	}
}
