using FileToVox.Schematics;
using FileToVoxCommon.Generator.Shaders.Data;

namespace FileToVox.Generator.Shaders
{
	public interface IShaderGenerator
	{
		Schematic ApplyShader(Schematic schematic, ShaderStep shaderStep);
	}
}
