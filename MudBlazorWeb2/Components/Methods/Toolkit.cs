using System.Diagnostics;

namespace MudBlazorWeb2.Components.Methods
{
    public class ConsoleCol
    {
        private static readonly object consoleLock = new object();

        public static void WriteLine(string text, ConsoleColor color)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = color;
                Console.WriteLine(text);
                Console.ResetColor();
            }
        }

        public static void Write(string text, ConsoleColor color)
        {
            lock (consoleLock)
            {
                Console.ForegroundColor = color;
                Console.Write(text);
                Console.ResetColor();
            }
        }
    }

    public class Files
    {
        public static void DeleteFilesByPath(params string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }
        public static void CreateDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }
        public static void DeleteDirectory(string directoryPath)
        {
            DirectoryInfo dir = new DirectoryInfo(directoryPath);
            if (dir.Exists)
            {
                dir.Delete(true);
            }
        }
    }
    public class Cmd
    {
        public static async Task RunProcess(string executablePath, string arguments)
        {
            try
            {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = executablePath;
                    process.StartInfo.Arguments = arguments;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.WorkingDirectory = Path.GetDirectoryName(executablePath);

                    process.Start();
                    await process.WaitForExitAsync();

                    var output = await process.StandardOutput.ReadToEndAsync();
                    var error = await process.StandardError.ReadToEndAsync();

                    if (process.ExitCode != 0)
                    {
                        throw new Exception($"Decoder failed with exit code {process.ExitCode}:\n{error}");
                    }

                    Console.WriteLine("Cmd.RunProcess => Output: " + output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running decoder: {ex.Message}");
                throw;
            }
        }

    }

    public class FileLogger
    {
        private readonly string _filePath;

        public FileLogger(string filePath)
        {
            _filePath = filePath;
            Files.CreateDirectory(Path.GetDirectoryName(_filePath));
        }

        public void Log(string message)
        {
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}";
            var maxLines = 800; // Максимальное количество строк
            var lines = File.ReadAllLines(_filePath);
            var newLines = new List<string>(lines);

            if (newLines.Count >= maxLines)
            {
                newLines.RemoveAt(0); // Удалить самую старую строку
            }

            newLines.Add(logEntry);
            File.WriteAllLines(_filePath, newLines);
        }
    }
}
