using FileToVoxCore.Schematics;

namespace FileToVox.Generator
{
	public interface IGenerator
	{
		Schematic WriteSchematic();
	}
}
