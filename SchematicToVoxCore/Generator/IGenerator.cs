using FileToVox.Schematics;

namespace FileToVox.Generator
{
	public interface IGenerator
	{
		Schematic WriteSchematic();
	}
}
