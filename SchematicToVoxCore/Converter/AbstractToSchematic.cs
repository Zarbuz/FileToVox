using FileToVoxCore.Schematics;

namespace FileToVox.Converter
{
	public abstract class AbstractToSchematic
    {
        protected string PathFile;

        protected AbstractToSchematic(string path)
        {
            PathFile = path;
        }

        protected AbstractToSchematic()
        {

        }

        public abstract Schematic WriteSchematic();

    }
}
