using Pastel;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace CBRE.Updater
{
	public enum LogSeverity
	{
		Message,
		Warning,
		Error
	}

	public class Program
	{
		//Arg 0: New version
		//Arg 1: CBRE-EX process name
		//Arg 2: Package filename
		static void Main(string[] args)
		{
			if (args.Length < 3) return;

			string targetDirectory = AppDomain.CurrentDomain.BaseDirectory;
			string currentFilename = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);

			string newVersion = args[0];
			string friendlyCbreProcess = args[1].Replace(".exe", "");
			string packageFilename = args[2];

			Console.Title = "CBRE-EX Updater";

			if (Environment.OSVersion.Version.Major < 10) 
				ConsoleExtensions.Disable();

			Log($"Waiting until {"CBRE-EX".Pastel(Color.LimeGreen)} shuts down...", LogSeverity.Message);

			while (true)
			{
				Process[] cbreProcess = Process.GetProcessesByName(friendlyCbreProcess);

				if (cbreProcess.Length > 0) 
					Thread.Sleep(100);
				else 
					break;
			}

			Log($"Installing {"CBRE-EX".Pastel(Color.LimeGreen)} {$"v{newVersion}".Pastel(Color.Lime)}", LogSeverity.Message);

			try
			{
				if (!File.Exists(packageFilename)) 
					throw new FileNotFoundException($"The update package was not found. Expected a file called \"{packageFilename}\" in this directory.");

				Log($"Extracting {packageFilename.Pastel(Color.LimeGreen)} to Temp directory...", LogSeverity.Message);

				if (Directory.Exists("Temp")) 
					Directory.Delete("Temp", true);

				ZipFile.ExtractToDirectory(packageFilename, "Temp");

				DirectoryInfo tempDir = new DirectoryInfo("Temp");
				DirectoryInfo[] tempSubdirs = tempDir.GetDirectories();

				foreach (DirectoryInfo dir in tempSubdirs)
				{
					Log($"Copying updated directory \"{dir.Name.Pastel(Color.Lime)}\" and its contents to existing install...", LogSeverity.Message);
					CopyDirectory(dir.FullName, Path.Combine(targetDirectory, dir.Name), true);
				}

				string[] whitelistedFiles = 
				{
					currentFilename,
					currentFilename + ".config",
					"Pastel.dll",
					Path.GetFileNameWithoutExtension(currentFilename) + ".pdb",
					"System.Memory.dll",
					"System.Numerics.Vectors.dll",
					"System.Runtime.CompilerServices.Unsafe.dll"
				};

				FileInfo[] tempDirFiles = tempDir.GetFiles();
				foreach (FileInfo file in tempDirFiles)
				{
					if (whitelistedFiles.Contains(file.Name)) 
						continue;

					Log($"Copying updated file \"{file.Name.Pastel(Color.Lime)}\" to existing install...", LogSeverity.Message);
					file.CopyTo(Path.Combine(targetDirectory, file.Name), true);
				}

				Log($"Cleaning up left over files...", LogSeverity.Message);

				foreach(FileInfo file in tempDirFiles)
				{
					if(whitelistedFiles.Contains(file.Name))
						continue;

					File.Delete(file.FullName);
				}

				DirectoryInfo[] tempDirDirs = tempDir.GetDirectories();
				foreach (DirectoryInfo dir in tempDirDirs)
				{
					Directory.Delete(dir.FullName, true);
				}

				File.Delete(packageFilename);

				Log($"Done! Starting CBRE-EX...", LogSeverity.Message);

				ProcessStartInfo editorProcess = new ProcessStartInfo(args[1]);
				editorProcess.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
				editorProcess.UseShellExecute = true;
				Process.Start(editorProcess);

				Environment.Exit(0);
			}
			catch (Exception ex)
			{
				Log($"Error! {ex.Message.Pastel(Color.IndianRed)}", LogSeverity.Error);

				if(!Console.IsInputRedirected)
				{
					Console.WriteLine("Press any key to exit...");

					Console.ReadKey(true);
				}
			}
		}

		static void CopyDirectory(string source, string destination, bool recursive)
		{
			DirectoryInfo dir = new DirectoryInfo(source);
			DirectoryInfo[] subdirs = dir.GetDirectories();

			Directory.CreateDirectory(destination);

			foreach (FileInfo file in dir.GetFiles())
			{
				string targetPath = Path.Combine(destination, file.Name);
				file.CopyTo(targetPath, true);
			}

			if (recursive)
			{
				foreach (DirectoryInfo subdir in subdirs)
				{
					string targetPath = Path.Combine(destination, subdir.Name);
					CopyDirectory(subdir.FullName, targetPath, true);
				}
			}
		}

		static void Log(string message, LogSeverity severity)
		{
			switch (severity)
			{
				case LogSeverity.Message:
					Console.WriteLine($"[{"MSG".Pastel(Color.CadetBlue)}] {message}");
					break;
				case LogSeverity.Warning:
					Console.WriteLine($"[{"WRN".Pastel(Color.Yellow)}] {message}");
					break;
				case LogSeverity.Error:
					Console.WriteLine($"[{"ERR".Pastel(Color.Red)}] {message}");
					break;
			}
		}
	}
}