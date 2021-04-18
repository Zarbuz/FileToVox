using FileToVox.Schematics;

namespace FileToVox.Converter
{
	public abstract class AbstractToSchematic
    {
        protected string Path;

        protected AbstractToSchematic(string path)
        {
            Path = path;
        }

        public abstract Schematic WriteSchematic();

    }
}
