using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using FileToVox.Converter.PointCloud;
using FileToVoxCore.Schematics;

namespace FileToVox.Converter
{
	public class MeshToSchematic : AbstractToSchematic
	{
		private PLYToSchematic mPlyToSchematic;

		public MeshToSchematic(string path, float scale, int colorLimit, int segmentX, int segmentY, int subsample, bool skipCapture) : base(path)
		{
			Console.WriteLine("[INFO] External program 'MeshSampler' needed! Check location of the program ...");
			string appRoot = AppDomain.CurrentDomain.BaseDirectory;
			string fullPath = Path.Combine(appRoot, "MeshSampler/MeshSampler.exe");
			Console.WriteLine("[INFO] Check at: " + fullPath);
			if (File.Exists(fullPath))
			{
				Console.WriteLine("[INFO] MeshSampler/MeshSampler.exe found!");
				FileInfo fileInfo = new FileInfo(fullPath);
				// Prepare the process to run
				ProcessStartInfo start = new ProcessStartInfo();
				// Enter in the command line arguments, everything you would enter after the executable name itself
				start.Arguments = "-input " + path + " -segmentX " + segmentX + " -segmentY " + segmentY + " -subsample " + subsample;
				// Enter the executable to run, including the complete path
				start.FileName = fileInfo.FullName;
				// Do you want to show a console window?
				start.WindowStyle = ProcessWindowStyle.Hidden;
				start.CreateNoWindow = true;
				int exitCode;


				if (!skipCapture)
				{
					// Run the external process & wait for it to finish
					using (Process proc = Process.Start(start))
					{
						proc.WaitForExit();

						// Retrieve the app's exit code
						exitCode = proc.ExitCode;
						if (exitCode == 0)
						{
							LoadPointCloud(scale, colorLimit);
						}
					}
				}
				else
				{
					Console.WriteLine("[INFO] SkipCapture enabled! Load previous points cloud...");
					LoadPointCloud(scale, colorLimit);
				}
			}
			else
			{
				throw new FileNotFoundException("[ERROR] MeshSampler/MeshSampler.exe not found!" + fullPath);
			}
		}

		private void LoadPointCloud(float scale, int colorLimit)
		{
			string appRoot = AppDomain.CurrentDomain.BaseDirectory;
			string resultFile = Path.Combine(appRoot, "MeshSampler/PMesh.ply");
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

		public override Schematic WriteSchematic()
		{
			return mPlyToSchematic.WriteSchematic();
		}
	}
}
