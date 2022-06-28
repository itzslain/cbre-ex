using Pastel;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace CBRE.Updater
{
    public enum LogSeverity
    {
        MESSAGE,
        WARNING,
        ERROR
    }

    public class Program
    {
        static void Main(string[] args)
        {
            string TargetDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string CurrentFilename = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            string friendlyCbreProcess = args[1].Replace(".exe", "");

            Console.Title = "CBRE-EX Updater";
            if (Environment.OSVersion.Version.Major < 10) ConsoleExtensions.Disable();

            Log($"Waiting until {"CBRE-EX".Pastel(Color.LimeGreen)} shuts down...", LogSeverity.MESSAGE);

            while (true)
            {
                Process[] cbre = Process.GetProcessesByName(friendlyCbreProcess);

                if (cbre.Length > 0) Thread.Sleep(100);
                else break;
            }

            Log($"Installing {"CBRE-EX".Pastel(Color.LimeGreen)} {$"v{args[0]}".Pastel(Color.Lime)}", LogSeverity.MESSAGE);

            try
            {
                if (!File.Exists("Update.zip")) throw new FileNotFoundException("The update package was not found. Did you execute this updater manually?");

                Directory.CreateDirectory("Temp");

                Log($"Extracting {"Update.zip".Pastel(Color.LimeGreen)} to Temp directory...", LogSeverity.MESSAGE);
                if (Directory.Exists("Temp")) Directory.Delete("Temp", true);
                ZipFile.ExtractToDirectory("Update.zip", "Temp");

                DirectoryInfo tempDir = new DirectoryInfo("Temp");
                DirectoryInfo[] tempDirs = tempDir.GetDirectories();

                foreach (DirectoryInfo dir in tempDirs)
                {
                    if (dir.Name == "Entities" || dir.Name == "Error Logs") continue;

                    Log($"Copying updated directory \"{dir.Name.Pastel(Color.Lime)}\" to existing install...", LogSeverity.MESSAGE);
                    CopyDirectory(dir.FullName, Path.Combine(TargetDirectory, dir.Name), true);
                }

                FileInfo[] tempDirFiles = tempDir.GetFiles();
                foreach (FileInfo file in tempDirFiles)
                {
                    if (file.Name == "Settings.vdf" || file.Name == CurrentFilename || file.Name == "Pastel.dll") continue;

                    Log($"Copying updated file \"{file.Name.Pastel(Color.Lime)}\" to existing install...", LogSeverity.MESSAGE);
                    file.CopyTo(Path.Combine(TargetDirectory, file.Name), true);
                }

                Log($"Cleaning up leftover files...", LogSeverity.MESSAGE);

                Directory.Delete("Temp", true);
                File.Delete("Update.zip");
                File.Delete("CHECKSUM.txt");

                Log($"Done! Starting CBRE-EX...", LogSeverity.MESSAGE);

                ProcessStartInfo editorProcess = new ProcessStartInfo(args[1]);
                editorProcess.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                editorProcess.UseShellExecute = true;
                Process.Start(editorProcess);

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Log($"Error! {ex.Message.Pastel(Color.IndianRed)}", LogSeverity.ERROR);

                Thread.Sleep(Timeout.Infinite);
            }
        }

        static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDir);
            DirectoryInfo[] dirs = dir.GetDirectories();

            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }

        static void Log(string message, LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.MESSAGE:
                    Console.WriteLine($"[{"MSG".Pastel(Color.CadetBlue)}] {message}");
                    break;
                case LogSeverity.WARNING:
                    Console.WriteLine($"[{"WRN".Pastel(Color.Yellow)}] {message}");
                    break;
                case LogSeverity.ERROR:
                    Console.WriteLine($"[{"ERR".Pastel(Color.Red)}] {message}");
                    break;
            }
        }
    }
}
