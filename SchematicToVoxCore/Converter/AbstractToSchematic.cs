using FileToVox.Schematics;

namespace FileToVox.Converter
{
	public abstract class AbstractToSchematic
    {
        protected string _path;

        protected AbstractToSchematic(string path)
        {
            _path = path;
        }

        public abstract Schematic WriteSchematic();

    }
}
