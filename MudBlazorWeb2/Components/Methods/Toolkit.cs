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
}
