//AiBackGroundService.cs

using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore;

using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb;
using static MudBlazorWeb2.Components.Pages.Todo;
using MudBlazorWeb2.Components.Methods;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.TodoList;

public class AiBackgroundService : BackgroundService
{
    private readonly ILogger<AiBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly SettingsService _settingsService;
    private readonly WhisperService _whisperService;
    private readonly OllamaService _ollamaService;


    public AiBackgroundService(ILogger<AiBackgroundService> logger, IConfiguration configuration, SettingsService settingsService, WhisperService whisperService, OllamaService ollamaService)
    {
        _logger = logger;
        _configuration = configuration;
        _settingsService = settingsService;
        _whisperService = whisperService;
        _ollamaService = ollamaService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AiProcessDatabaseAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing database");
            }
            await Task.Delay(10000, stoppingToken); // Adjust the delay as needed
        }
    }

    private async Task AiProcessDatabaseAsync(CancellationToken stoppingToken)
    {
        string FilePath = Path.Combine(AppContext.BaseDirectory, "todoitems.json");
        var JsonTodoItems = new SimpleJson<TodoItem>(FilePath);
        await JsonTodoItems.LoadItemsAsync();

        var items = JsonTodoItems.GetItems();
        foreach (var item in items)
        {
            ConsoleCol.WriteLine(item.Title, ConsoleColor.DarkCyan);
        }


        /*
                // This is a simplified example based on your existing code
                var settingsDB = _settingsService.GetSettings();
                var schemeName = settingsDB.SettingsSputnik.OraItems.Scheme;
                var conStringDBA = $"User Id={settingsDB.SettingsSputnik.OraItems.User};Password={settingsDB.SettingsSputnik.OraItems.Password};Data Source={settingsDB.SettingsSputnik.OraItems.DataSource};";

                using (var db = new OracleDbContext(new DbContextOptionsBuilder<OracleDbContext>().UseOracle(conStringDBA).Options))
                {
                    await db.Database.OpenConnectionAsync();
                    await db.Database.ExecuteSqlRawAsync($"ALTER SESSION SET CURRENT_SCHEMA = {schemeName}");

                    var ignoreRecordTypes = _configuration.GetSection("AudioConverter:IgnoreRecordTypes").Get<List<string>>();
                    var audioList = await EFCoreQuery.GetSpeechRecords(db, ignoreRecordTypes);

                    int processedKeys = 0;
                    int totalKeys = audioList.Count;

                    foreach (var entity in audioList)
                    {
                        if (stoppingToken.IsCancellationRequested)
                        {
                            break;
                        }

                        // Update Progress
                        // Here you can use a message queue or a shared state to update the progress in your Blazor component
                        // For simplicity, let's assume you have a method to update the progress
                        UpdateProgress(processedKeys, totalKeys);

                        // ORACLE => get audio (left, right, recordType)
                        var (audioDataLeft, audioDataRight, recordType) = await EFCoreQuery.GetAudioDataAsync(entity.Id, db);

                        // PreText => get PreText for operator or PreTextDefault
                        string preText = await GetPreTextForOperatorAsync(entity.Sourcename);

                        // FFMpeg or Decoder => audio to folder
                        string audioFilePath = Path.Combine(_configuration["AudioPathForProcessing"], $"{entity.Id}.wav");
                        bool result = await DbToAudioConverter.FFMpegDecoder(audioDataLeft, audioDataRight, recordType, audioFilePath, _configuration);
                        if (!result) continue;

                        // WHISPER
                        Task<string> _recognizedText = _whisperService.RecognizeSpeechAsync(audioFilePath, _configuration);
                        (string languageCode, string detectedLanguage) = await _whisperService.DetectLanguageAsync(audioFilePath, _configuration);
                        string recognizedText = await _recognizedText;

                        // OLLAMA + ORACLE => Run task !!!_WITHOUT await
                        await ProcessOllamaAndUpdateEntityAsync(entity.Id, recognizedText, languageCode, detectedLanguage, preText, _configuration["OllamaModelName"], schemeName, conStringDBA, _configuration, _settingsService, entity, db);

                        processedKeys++;
                    }
                    await db.Database.CloseConnectionAsync();
                }
            }
            private void UpdateProgress(int processed, int total)
            {
                // Implement a way to update the progress in your Blazor component
                // For example, you could use a shared service or a message queue
                // Here, we'll just log it for demonstration purposes
                _logger.LogInformation($"Processed {processed} out of {total}");
                // Update your progress in your Blazor component ???
                ProgressService.UpdateProgress(processed, total);
            }

            private async Task<string> GetPreTextForOperatorAsync(string operatorName)
            {
                // Implement getting pre-text logic here
                // For simplicity, let's return a default value
                return "Default PreText";
            }

            private async Task ProcessOllamaAndUpdateEntityAsync(long? entityId, string recognizedText, string languageCode, string detectedLanguage, string preText, string modelName, string schemeName, string conStringDBA, IConfiguration configuration, SettingsService settingsService, SPR_SPEECH_TABLE entity, OracleDbContext db)
            {
                // Implement OLLAMA and ORACLE update logic here
                // For simplicity, let's just log it
                _logger.LogInformation($"Processing OLLAMA for entity {entityId}");
            }
        */
    }

}