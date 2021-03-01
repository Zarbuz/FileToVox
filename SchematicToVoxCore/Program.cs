using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FileToVox.Converter;
using FileToVox.Converter.Image;
using FileToVox.Converter.Json;
using FileToVox.Converter.PointCloud;
using FileToVox.Schematics;
using FileToVox.Vox;
using NDesk.Options;

namespace FileToVox
{
	class Program
	{
		private static string mInputPath;
		private static string mOutputPath;
		private static string mInputColorFile;
		private static string mInputPaletteFile;

		private static bool mShowHelp;
		private static bool mVerbose;
		private static bool mExcavate;
		private static bool mColor;
		private static bool mTop;
		private static bool mFlood;
		private static bool mHoles;
		private static bool mLonely;
		private static bool mSlice;

		private static float mSlow;

		private static int mIgnoreMinY = -1;
		private static int mIgnoreMaxY = 256;
		private static float mScale = 1;
		private static int mHeightMap = 1;
		private static int mGridSize = 126;
		private static int mColorLimit = 256;
		private static int mChunkSize = 128;

		

		public static void Main(string[] args)
		{
			OptionSet options = new OptionSet()
			{
				{"i|input=", "input path", v => mInputPath = v},
				{"o|output=", "output path", v => mOutputPath = v},
				{"c|color", "enable color when generating heightmap", v => mColor = v != null},
				{"cm|color-from-file=", "load colors from file", v => mInputColorFile = v },
				{"cl|color-limit=", "set the maximal number of colors for the palette", (int v) => mColorLimit =v },
				{"cs|chunk-size=", "set the chunk size", (int v) => mChunkSize = v},
				{"e|excavate", "delete all voxels which doesn't have at least one face connected with air",  v => mExcavate = v != null },
				{"fl|flood", "fill all invisible voxels", v => mFlood = v != null },
				{"flo|fix-lonely", "delete all voxels where all connected voxels are air", v => mLonely = v != null },
				{"fh|fix-holes", "fix holes", v => mHoles = v != null },
				{"gs|grid-size=", "set the grid size", (int v) => mGridSize = v },
				{"h|help", "help informations", v => mShowHelp = v != null},
				{"hm|heightmap=", "create voxels terrain from heightmap (only for PNG file)", (int v) => mHeightMap = v},
				{"iminy|ignore-min-y=", "ignore voxels below the specified layer", (int v) => mIgnoreMinY = v},
				{"imaxy|ignore-max-y=", "ignore voxels above the specified layer", (int v) => mIgnoreMaxY = v},
				{"p|palette=", "set the palette", v => mInputPaletteFile = v },
				{"sc|scale=", "set the scale", (float v) => mScale = v},
				{"si|slice", "indicate that each picture is a slice", v => mSlice = v != null},
				{"sl|slow=", "use a slower algorithm (use all cores) to generate voxels from OBJ but best result (value should be enter 0.0 and 1.0 (0.5 is recommended)", (float v) => mSlow = v },
				{"t|top", "create voxels only at the top of the heightmap", v => mTop = v != null},
				{"v|verbose", "enable the verbose mode", v => mVerbose = v != null},
			};

			try
			{
				List<string> extra = (args.Length > 0) ? options.Parse(args) : options.Parse(CheckArgumentsFile());
				CheckHelp(options);
				CheckArguments();
				DisplayArguments();

				if (mInputPath != null)
					ProcessFile();
				CheckVerbose();
				Console.WriteLine("[LOG] Done.");
				if (mVerbose)
				{
					Console.ReadKey();
				}
			}
			catch (Exception e)
			{
				Console.Write("FileToVox: ");
				Console.WriteLine(e.Message);
				Console.WriteLine("Try `FileToVox --help` for more informations.");
				Console.ReadLine();
			}
		}

		private static string[] CheckArgumentsFile()
		{
			if (!File.Exists("settings.ini"))
			{
				File.Create("settings.ini");
			}

			Console.WriteLine("[INFO] Reading arguments from settings.ini");
			string[] args = new string[0];
			using (StreamReader file = new StreamReader("settings.ini"))
			{
				string line;
				while ((line = file.ReadLine()) != null)
				{
					if (line.Contains("#"))
					{
						continue;
					}

					Console.WriteLine($"[INFO] {line}");
					args = line.Split(" ");
				}
			}

			return args;
		}

		private static void CheckHelp(OptionSet options)
		{
			if (mShowHelp)
			{
				ShowHelp(options);
				Environment.Exit(0);
			}
		}

		private static void CheckArguments()
		{
			if (mInputPath == null)
				throw new ArgumentNullException("[ERROR] Missing required option: --i");
			if (mOutputPath == null)
				throw new ArgumentNullException("[ERROR] Missing required option: --o");
			if (mIgnoreMinY < -1)
				throw new ArgumentException("[ERROR] --ignore-min-y argument must be positive");
			if (mIgnoreMaxY > 256)
				throw new ArgumentException("[ERROR] --ignore-max-y argument must be lower than 256");
			if (mScale <= 0)
				throw new ArgumentException("[ERROR] --scale argument must be positive");
			if (mHeightMap < 1)
				throw new ArgumentException("[ERROR] --heightmap argument must be positive");
			if (mColorLimit < 0)
				throw new ArgumentException("[ERROR] --color-limit argument must be positive");
			if (mColorLimit > 256)
				throw new ArgumentException("[ERROR] --color-limit argument must be lower than 256");
			if (mChunkSize <= 10 || mChunkSize > 257)
				throw new ArgumentException("[ERROR] --chunk-size argument must be lower than 257 and greater than 10");
		}

		private static void DisplayArguments()
		{
			if (mInputPath != null)
				Console.WriteLine("[INFO] Specified input path: " + mInputPath);
			if (mOutputPath != null)
				Console.WriteLine("[INFO] Specified output path: " + mOutputPath);
			if (mInputColorFile != null)
				Console.WriteLine("[INFO] Specified input color file: " + mInputColorFile);
			if (mInputPaletteFile != null)
				Console.WriteLine("[INFO] Specified palette file: " + mInputPaletteFile);
			if (mIgnoreMinY != -1)
				Console.WriteLine("[INFO] Specified min Y layer : " + mIgnoreMinY);
			if (mIgnoreMaxY != 256)
				Console.WriteLine("[INFO] Specified max Y layer : " + mIgnoreMaxY);
			if (mColorLimit != 256)
				Console.WriteLine("[INFO] Specified color limit: " + mColorLimit);
			if (mScale != 0)
				Console.WriteLine("[INFO] Specified increase size: " + mScale);
			if (mGridSize != 126)
				Console.WriteLine("[INFO] Specified grid size: " + mGridSize);
			if (mChunkSize != 125)
				Console.WriteLine("[INFO] Specified chunk size: " + mChunkSize);
			if (mSlow != 0)
				Console.WriteLine("[INFO] Specified winding_number: " + mSlow);
			if (mExcavate)
				Console.WriteLine("[INFO] Enabled option: excavate");
			if (mColor)
				Console.WriteLine("[INFO] Enabled option: color");
			if (mHeightMap != 1)
				Console.WriteLine("[INFO] Enabled option: heightmap (value=" + mHeightMap + ")");
			if (mTop)
				Console.WriteLine("[INFO] Enabled option: top");
			if (mFlood)
				Console.WriteLine("[INFO] Enabled option: flood");
			if (mHoles)
				Console.WriteLine("[INFO] Enabled option: fix-holes");
			if (mLonely)
				Console.WriteLine("[INFO] Enabled option: fix-lonely");
			if (mSlice)
				Console.WriteLine("[INFO] Enabled option: slice");

			Console.WriteLine("[INFO] Specified output path: " + Path.GetFullPath(mOutputPath));
		}

		private static void ProcessFile()
		{
			string path = Path.GetFullPath(mInputPath);
			bool isFolder = false;
			if (!Directory.Exists(path))
			{
				if (!File.Exists(path))
				{
					throw new FileNotFoundException("[ERROR] Input file not found", mInputPath);
				}
			}
			else
			{
				isFolder = true;
			}
			try
			{
				AbstractToSchematic converter;
				if (isFolder)
				{
					if (mSlice)
					{
						converter = new FolderImageToSchematic(path, mExcavate, mInputColorFile, mColorLimit);
						SchematicToVox(converter, mOutputPath);
					}
					else
					{
						string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(t => Path.GetExtension(t).ToLower() != ".vox").ToArray();
						Console.WriteLine("[INFO] Processing folder " + files.Length + " files");
						for (int i = 0; i < files.Length; i++)
						{
							string file = files[i];
							Console.WriteLine("[INFO] Processing file: " + file);
							converter = GetConverter(file);
							if (converter != null)
							{
								SchematicToVox(converter, Path.Combine(mOutputPath, Path.GetFileNameWithoutExtension(file)));
							}
							else
							{
								Console.WriteLine("[ERROR] Unsupported file extension !");
							}
						}
					}
				}
				else
				{
					converter = GetConverter(path);
					if (converter != null)
					{
						SchematicToVox(converter, mOutputPath);
					}
					else
					{
						Console.WriteLine("[ERROR] Unsupported file extension !");
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				Console.ReadLine();
			}

		}

		private static AbstractToSchematic GetConverter(string path)
		{
			switch (Path.GetExtension(path))
			{
				case ".asc":
					return new ASCToSchematic(path);
				case ".binvox":
					return new BinvoxToSchematic(path);
				case ".csv":
					return new CSVToSchematic(path, mScale, mColorLimit, mHoles, mFlood, mLonely);
				case ".obj":
					return new OBJToSchematic(path, mGridSize, mExcavate, mSlow);
				case ".ply":
					return new PLYToSchematic(path, mScale, mColorLimit, mHoles, mFlood, mLonely);
				case ".png":
					return new PNGToSchematic(path, mInputColorFile, mHeightMap, mExcavate, mColor, mTop, mColorLimit);
				case ".qb":
					return new QBToSchematic(path);
				case ".schematic":
					return new SchematicToSchematic(path, mIgnoreMinY, mIgnoreMaxY, mExcavate, mScale);
				case ".tif":
					return new TIFtoSchematic(path, mInputColorFile, mHeightMap, mExcavate, mColor, mTop, mColorLimit);
				case ".xyz":
					return new XYZToSchematic(path, mScale, mColorLimit, mHoles, mFlood, mLonely);
				case ".json":
					return new JsonToSchematic(path);
				default:
					return null;
			}
		}

		private static void SchematicToVox(AbstractToSchematic converter, string outputPath)
		{
			Schematic schematic = converter.WriteSchematic();
			Console.WriteLine($"[INFO] Vox Width: {schematic.Width}");
			Console.WriteLine($"[INFO] Vox Length: {schematic.Length}");
			Console.WriteLine($"[INFO] Vox Height: {schematic.Height}");

			if (schematic.Width > Schematic.MAX_WORLD_WIDTH || schematic.Length > Schematic.MAX_WORLD_LENGTH || schematic.Height > Schematic.MAX_WORLD_HEIGHT)
			{
				Console.WriteLine("[ERROR] Model is too big ! MagicaVoxel can't support model bigger than 2000x2000x1000");
				return;
			}

			VoxWriter writer = new VoxWriter();

			if (mInputPaletteFile != null)
			{
				PaletteSchematicConverter converterPalette = new PaletteSchematicConverter(mInputPaletteFile, mColorLimit);
				schematic = converterPalette.ConvertSchematic(schematic);
				writer.WriteModel(mChunkSize, outputPath + ".vox", converterPalette.GetPalette(), schematic);
			}
			else
			{
				writer.WriteModel(mChunkSize, outputPath + ".vox", null, schematic);
			}
		}

		private static void ShowHelp(OptionSet p)
		{
			Console.WriteLine("Usage: FileToVox --i INPUT --o OUTPUT");
			Console.WriteLine("Options: ");
			p.WriteOptionDescriptions(Console.Out);
		}

		private static void CheckVerbose()
		{
			if (mVerbose)
			{
				VoxReader reader = new VoxReader();
				reader.LoadModel(mOutputPath + ".vox");
			}
		}
	}
}
