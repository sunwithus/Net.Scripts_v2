using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.TodoList;
using MudBlazorWeb2.Components.Pages;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Text.Json;

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
            if(!File.Exists(_filePath))File.Create(_filePath);
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
    /// <summary>
    /// Usage
    /// string FilePath = Path.Combine(AppContext.BaseDirectory, "todoitems.json");
    /// var JsonTodoItems = new SimpleJson<TodoItem>(FilePath);
    /// await JsonTodoItems.LoadItemsAsync();
    /// </summary>
    public class SimpleJson<T> where T : class, new()
    {
        private readonly string filePath;
        private List<T> items = new();

        public SimpleJson(string filePath)
        {
            this.filePath = filePath;
        }

        public async Task LoadItemsAsync()
        {
            if (File.Exists(filePath))
            {
                var json = await File.ReadAllTextAsync(filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    items = new List<T>();
                    await SaveItemsAsync(); // Сохранить начальную структуру
                }
                else
                {
                    var options = new JsonSerializerOptions
                    {
                        ReadCommentHandling = JsonCommentHandling.Skip
                    };
                    items = JsonSerializer.Deserialize<List<T>>(json, options) ?? new List<T>();
                }
            }
            /*
            else
            {
                items = new List<T>();
                await SaveItemsAsync(); // Создать новый файл
            }
            */
        }

        public async Task SaveItemsAsync()
        {
            var json = JsonSerializer.Serialize(items);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task AddItemAsync(T newItem)
        {
            if (newItem != null)
            {
                items.Add(newItem);
                await SaveItemsAsync();
            }
        }

        public async Task DeleteItemAsync(T item)
        {
            if (items.Contains(item))
            {
                items.Remove(item);
                await SaveItemsAsync();
            }
        }

        public List<T> GetItems()
        {
            return new List<T>(items);
        }

        public async Task UpdateItemAsync(T item, Func<T, bool> predicate)
        {
            var existingItem = items.FirstOrDefault(predicate);
            if (existingItem != null)
            {
                var index = items.IndexOf(existingItem);
                items[index] = item;
                await SaveItemsAsync();
            }
        }

    }
}
