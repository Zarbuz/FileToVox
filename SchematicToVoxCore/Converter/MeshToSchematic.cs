using System;
using System.Diagnostics;
using System.IO;
using FileToVox.Converter.PointCloud;
using FileToVox.Schematics;

namespace FileToVox.Converter
{
	public class MeshToSchematic : AbstractToSchematic
	{
		private PLYToSchematic mPlyToSchematic;

		public MeshToSchematic(string path, float scale, int colorLimit) : base(path)
		{
			Console.WriteLine("[INFO] External program 'MeshSampler' needed! Check location of the program ...");
			if (File.Exists("MeshSampler/MeshSampler.exe"))
			{
				Console.WriteLine("[INFO] MeshSampler/MeshSampler.exe found!");
				FileInfo fileInfo = new FileInfo("MeshSampler/MeshSampler.exe");
				// Prepare the process to run
				ProcessStartInfo start = new ProcessStartInfo();
				// Enter in the command line arguments, everything you would enter after the executable name itself
				start.Arguments = "-input " + path;
				// Enter the executable to run, including the complete path
				start.FileName = fileInfo.FullName;
				// Do you want to show a console window?
				start.WindowStyle = ProcessWindowStyle.Hidden;
				start.CreateNoWindow = true;
				int exitCode;


				// Run the external process & wait for it to finish
				using (Process proc = Process.Start(start))
				{
					proc.WaitForExit();

					// Retrieve the app's exit code
					exitCode = proc.ExitCode;
					if (exitCode == 0)
					{
						string resultFile = "MeshSampler/PMesh.ply";
						FileInfo fileResultInfo = new FileInfo(resultFile);

						if (File.Exists(resultFile))
						{
							mPlyToSchematic = new PLYToSchematic(fileResultInfo.FullName, scale, colorLimit);
						}
						else
						{
							throw new FileNotFoundException("[ERROR] MeshSampler/PMesh.ply not found! Something went wrong...");
						}
					}
				}
			}
			else
			{
				throw new FileNotFoundException("[ERROR] MeshSampler/MeshSampler.exe not found!");
			}
		}

		public override Schematic WriteSchematic()
		{
			return mPlyToSchematic.WriteSchematic();
		}
	}
}
