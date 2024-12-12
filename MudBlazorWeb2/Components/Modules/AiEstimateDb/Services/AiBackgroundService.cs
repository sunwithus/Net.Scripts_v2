//AiBackGroundService.cs

using MudBlazorWeb2.Components.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorWeb2.Components.Modules._Shared;
using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.Modules.AiEstimateDb;
using MudBlazorWeb2.Components.EntityFrameworkCore.SqliteModel;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.AspNetCore.Components;
using System.Configuration;
using MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;
using static MudBlazor.CategoryTypes;
using Npgsql;
using Oracle.EntityFrameworkCore;
using Oracle.ManagedDataAccess.Client;
using MudBlazorWeb2.Components.Pages;

public class AiBackgroundService : BackgroundService
{
    private readonly ILogger<AiBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly WhisperService _whisper;
    private readonly OllamaService _ollama;
    private readonly IHubContext<TodoHub> _hubContext;

    private readonly IDbContextFactory<SqliteDbContext> _sqliteDbContext;
    private readonly IDbContextFactory _dbContextFactory;

    private List<string> IgnoreRecordTypes;

    public AiBackgroundService(ILogger<AiBackgroundService> logger, IConfiguration configuration, WhisperService whisperService, OllamaService ollamaService, IDbContextFactory<SqliteDbContext> sqliteDbContext, IDbContextFactory dbContextFactory, IHubContext<TodoHub> hubContext)
    {
        _logger = logger;
        _configuration = configuration;
        _whisper = whisperService;
        _ollama = ollamaService;
        _hubContext = hubContext;

        _sqliteDbContext = sqliteDbContext;
        _dbContextFactory = dbContextFactory;

        // Инициализация переменных в конструкторе
        IgnoreRecordTypes = _configuration.GetSection("AudioConverter:IgnoreRecordTypes").Get<List<string>>();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AiProcessDatabaseAsync(stoppingToken);
                
                Console.WriteLine("AiBackgroundService...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing database");
            }
            await Task.Delay(8000, stoppingToken); // delay
        }
    }

    private async Task UpdateTodoItemStateAsync(TodoItem item, string message, CancellationToken _stoppingToken)
    {
        item.ProcessingMessage = message;
        await _sqliteDbContext.CreateDbContext().UpdateTodo(item);
        await _sqliteDbContext.CreateDbContext().SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("UpdateTodos", item, _stoppingToken);
    }
    private async Task StopProcessingAsync(TodoItem item, string message, CancellationToken _stoppingToken)
    {
        item.IsRunPressed = false;
        item.IsRunning = false;
        item.ProcessingMessage = message;
        await _sqliteDbContext.CreateDbContext().UpdateTodo(item);
        await _sqliteDbContext.CreateDbContext().SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("UpdateTodos", item, _stoppingToken);
    }
    private async Task HandleExceptionAsync(TodoItem item, Exception ex, CancellationToken _stoppingToken)
    {
        item.ProcessingMessage = $"{DateTime.Now} Error: {ex.Message}";
        item.IsStopPressed = true;
        item.IsRunPressed = false;
        await _sqliteDbContext.CreateDbContext().UpdateTodo(item);
        await _sqliteDbContext.CreateDbContext().SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("UpdateTodos", item, _stoppingToken);
    }
    private async Task AiProcessDatabaseAsync(CancellationToken stoppingToken)
    {
        // получение списка TODO задач для обработки
        using var sqlite = _sqliteDbContext.CreateDbContext();
        var todoItems = await sqlite.TodoItems.ToListAsync();

        foreach (var item in todoItems)
        {
            Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

            if (!item.IsRunPressed && !item.IsRunning)
            {
                await UpdateTodoItemStateAsync(item, "Готово к запуску. 💤", stoppingToken);
            }
            if(item.IsRunPressed)
            {
                item.IsRunning = true;
                item.CompletedKeys = 0;
                item.TotalKeys = 0;
                await UpdateTodoItemStateAsync(item, "Идёт выполнение... ⌛", stoppingToken);
                Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                try
                {
                    string conStringDBA = SelectDb.ConStringDBA(item);
                    using var Context = await _dbContextFactory.CreateDbContext(item.DbType, conStringDBA, item.Scheme);

                    Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                    //получение записей
                    List<SprSpeechTable> AudioList = await EFCoreQuery.GetSpeechRecords(item.StartDateTime, item.EndDateTime, item.MoreThenDuration, Context, IgnoreRecordTypes);
                    item.TotalKeys = AudioList.Count;

                    //если записи отсутствуют => к следующему TODO json
                    if (item.TotalKeys <= 0)
                    {
                        await Task.Delay(1500);
                        if (item.IsCyclic)
                        {
                            await UpdateTodoItemStateAsync(item, $"Обработано {item.CompletedKeys}/{item.TotalKeys}. Ожидание повторного запуска.", stoppingToken);
                        }
                        else
                        {
                            item.IsRunPressed = false;
                            await UpdateTodoItemStateAsync(item, $"Обработано {item.CompletedKeys}/{item.TotalKeys}.", stoppingToken);
                        }
                        continue; //к следующей итерации todoItems
                    }
                    Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                    //если записи есть => действие с записями
                    int ProcessedWhisper = 0; //выполнено Whisper = 0
                    foreach (var entity in AudioList)
                    {
                        // Остановить процесс, если нажата кнопка
                        TodoItem tempItem = await _sqliteDbContext.CreateDbContext().LoadTodoItem(item.Id);
                        item.IsStopPressed = tempItem.IsStopPressed;
                        if (item.IsStopPressed)
                        {
                            await StopProcessingAsync(item, $"{DateTime.Now} Остановлено: {item.CompletedKeys} / {item.TotalKeys}", stoppingToken);
                            break;
                        }
                        Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                        // Обработка записей
                        //await ProcessEntityAsync(entity, item, Context, stoppingToken);

                        item.ProcessingMessage = $"Выполнено: {item.CompletedKeys} / {item.TotalKeys}";
                        sqlite.TodoItems.Update(item);
                        await sqlite.SaveChangesAsync();
                        await _hubContext.Clients.All.SendAsync("UpdateTodos", item, stoppingToken);

                        tempItem = await _sqliteDbContext.CreateDbContext().LoadTodoItem(item.Id);
                        item.IsStopPressed = tempItem.IsStopPressed;
                        Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                        // PreText => get PreText for operator or PreTextDefault
                        string preText = await Params.GetPreTextAsync(entity.SSourcename);

                        // Db => get audio (left, right, recordType)
                        var (audioDataLeft, audioDataRight, recordType) = await EFCoreQuery.GetAudioDataAsync(entity.SInckey, Context);
                        Console.WriteLine($"Audio data for key {entity.SInckey} loaded successfully. recordType = " + recordType);

                        tempItem = await _sqliteDbContext.CreateDbContext().LoadTodoItem(item.Id);
                        item.IsStopPressed = tempItem.IsStopPressed;
                        Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                        // FFMpeg or Decoder => audio to folder
                        string audioFilePath = Path.Combine(_configuration["AudioPathForProcessing"], $"{entity.SInckey}.wav");
                        bool result = await DbToAudioConverter.FFMpegDecoder(audioDataLeft, audioDataRight, recordType, audioFilePath, _configuration);
                        if (!result) continue;

                        tempItem = await _sqliteDbContext.CreateDbContext().LoadTodoItem(item.Id);
                        item.IsStopPressed = tempItem.IsStopPressed;
                        Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                        // WHISPER
                        Task<string> _recognizedText = _whisper.RecognizeSpeechAsync(audioFilePath, _configuration); //асинхронно, не ждём
                        (string languageCode, string detectedLanguage) = await _whisper.DetectLanguageAsync(audioFilePath, _configuration);
                        string recognizedText = await _recognizedText; //дожидаемся _recognizedText...

                        tempItem = await _sqliteDbContext.CreateDbContext().LoadTodoItem(item.Id);
                        item.IsStopPressed = tempItem.IsStopPressed;
                        Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                        // Остановить процесс, если нажата кнопка
                        if (item.IsStopPressed)
                        {
                            await StopProcessingAsync(item, $"{DateTime.Now} Остановлено: {item.CompletedKeys} / {item.TotalKeys}", stoppingToken);
                            break; //выход из foreach (var entity in AudioList)
                        }

                        Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                        // Temprorary push string to Notice to aviod repeated process with entity
                        entity.SNotice = "TempRecord";
                        Context.SprSpeechTables.Update(entity);
                        await Context.SaveChangesAsync();

                        Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                        _logger.LogInformation("entity.SNotice = \"TempRecord\"");

                        // Delete earlier created file
                        Files.DeleteFilesByPath(audioFilePath);

                        Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                        // OLLAMA + ORACLE => Run task !!!_WITHOUT await
                        using var NewContext = await _dbContextFactory.CreateDbContext(item.DbType, conStringDBA, item.Scheme);
                        _logger.LogInformation("NewContext");
                        await ProcessOllamaAndUpdateEntityAsync(entity.SInckey, recognizedText, languageCode, detectedLanguage, preText, _configuration["OllamaModelName"], _configuration, entity, NewContext, sqlite, item);
                        _logger.LogInformation("ProcessOllamaAndUpdateEntityAsync");

                        Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                        // TODO
                        // разрешить "вырываться вперёд не более чем на N раз" и ProcessedAi
                        while (ProcessedWhisper - 2 > item.CompletedKeys)
                        {
                            await Task.Delay(5000);
                            ConsoleCol.WriteLine("Delay", ConsoleColor.Yellow);
                            ConsoleCol.WriteLine("ProcessedOllama / ProcessedWhisper => " + item.CompletedKeys + "/" + ProcessedWhisper, ConsoleColor.Yellow);
                        }
                        ProcessedWhisper++;
                        _logger.LogInformation("ProcessedOllama / ProcessedWhisper => " + item.CompletedKeys + "/" + ProcessedWhisper);

                        Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                        item.ProcessingMessage = $"Идёт выполнение: {item.CompletedKeys} / {item.TotalKeys}";
                        sqlite.TodoItems.Update(item);
                        await sqlite.SaveChangesAsync();
                        await _hubContext.Clients.All.SendAsync("UpdateTodos", item, stoppingToken);

                        Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");
                    }
                }
                catch (OracleException ex)
                {
                    await HandleExceptionAsync(item, ex, stoppingToken);
                }
                catch (NpgsqlException ex)
                {
                    await HandleExceptionAsync(item, ex, stoppingToken);
                }
                catch (Exception ex)
                {
                    await HandleExceptionAsync(item, ex, stoppingToken);
                }
                finally
                {
                    await _hubContext.Clients.All.SendAsync("UpdateTodos", item, stoppingToken);
                }

            }


        }
    }

    private async Task ProcessOllamaAndUpdateEntityAsync(long? entityId, string recognizedText, string languageCode, string detectedLanguage, string preText, string modelName, IConfiguration Configuration, SprSpeechTable entity, BaseDbContext db, SqliteDbContext sqlite, TodoItem item)
    {
        // OLLAMA
        try
        {
            (string responseOllamaText, int lagTime) = await _ollama.OllamaResponse(preText, recognizedText, Configuration);
            if (languageCode != "ru" && languageCode != "uk" && !string.IsNullOrEmpty(languageCode))
            {
                (recognizedText, lagTime) = await _ollama.OllamaTranslate(recognizedText, languageCode, detectedLanguage, Configuration);
            }
            await EFCoreQuery.InsertCommentAsync(entityId, recognizedText, detectedLanguage, responseOllamaText, Configuration["OllamaModelName"], db, item.BackLight);
            item.CompletedKeys++;
            sqlite.TodoItems.Update(item);
            await sqlite.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // EFCoreQuery - "обнуление" Notice при ошибке
            await EFCoreQuery.UpdateNoticeValueAsync(entityId, db, null);
            Console.WriteLine("Ошибка при обработке Ollama и обновлении сущности EFCore: " + ex.Message);

            // TODO if (MistakesCount > 10)
            //StateService.IsStopPressed = true;
        }

    }

}

