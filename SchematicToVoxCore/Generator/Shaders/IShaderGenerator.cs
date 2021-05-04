using FileToVox.Schematics;

namespace FileToVox.Generator.Shaders
{
	public interface IShaderGenerator
	{
		Schematic ApplyShader(Schematic schematic, ShaderStep shaderStep);
	}
}
