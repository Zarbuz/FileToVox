using System;
using System.IO;
using FileToVox.Schematics;

namespace FileToVox.Converter
{
	public class MeshToSchematic : AbstractToSchematic
	{
		public MeshToSchematic(string path) : base(path)
		{
			Console.WriteLine("[INFO] External program 'MeshSampler' needed! Check location of the program ...");
			if (File.Exists("MeshSampler/MeshSampler.exe"))
			{

			}
			else
			{
				throw new FileNotFoundException("MeshSampler/MeshSampler.exe not found!");
			}
		}

		public override Schematic WriteSchematic()
		{
			return null;
		}
	}
}
