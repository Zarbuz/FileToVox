using FileToVox.Schematics;
using System;

namespace FileToVox.Generator.Shaders
{
	public static class ShaderUtils
	{
		#region PublicMethods

		public static Schematic ApplyShader(Schematic schematic, ShaderStep shaderStep)
		{
			IShaderGenerator shaderGenerator;

			switch (shaderStep.ShaderType)
			{
				case ShaderType.FIX_HOLES:
					shaderGenerator = new ApplyShaderFixHoles();
					break;
				case ShaderType.FIX_LONELY:
					shaderGenerator = new ApplyShaderFixLonely();
					break;
				case ShaderType.CASE:
					shaderGenerator = new ApplyShaderCase();
					break;
				case ShaderType.PATINA:
					shaderGenerator = new ApplyShaderPatina();
					break;
				default:
					throw new NotImplementedException();
			}

			return shaderGenerator.ApplyShader(schematic, shaderStep);
		}

		

		#endregion

	}
}
