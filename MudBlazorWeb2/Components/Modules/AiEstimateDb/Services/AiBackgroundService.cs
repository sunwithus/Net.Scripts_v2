//AiBackGroundService.cs

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorWeb2.Components.Modules._Shared;

using MudBlazorWeb2.Components.Modules.AiEstimateDb;
using MudBlazorWeb2.Components.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore.SqliteModel;
using MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;
using Npgsql;
using Oracle.ManagedDataAccess.Client;

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
                //Console.WriteLine("AiBackgroundService...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing database");
            }
            await Task.Delay(3000, stoppingToken); // delay
        }
    }

    private async Task UpdateTodoItemStateAsync(TodoItem item, string message, CancellationToken _stoppingToken)
    {
        using var context = _sqliteDbContext.CreateDbContext();
        try
        {
            var todoItemFromDb = await context.TodoItems.FindAsync(item.Id);
            if (todoItemFromDb != null)
            {
                todoItemFromDb.CompletedKeys = item.CompletedKeys;
                todoItemFromDb.TotalKeys = item.TotalKeys;
                todoItemFromDb.ProcessingMessage = message;
                await context.SaveChangesAsync();
            }
            await _hubContext.Clients.All.SendAsync("UpdateTodos", todoItemFromDb, _stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating TodoItem");
        }
    }
    private async Task StopProcessingAsync(TodoItem item, string message, CancellationToken _stoppingToken)
    {
        using var context = _sqliteDbContext.CreateDbContext();
        var todoItemFromDb = await context.TodoItems.FindAsync(item.Id);
        if (todoItemFromDb != null)
        {
            todoItemFromDb.CompletedKeys = item.CompletedKeys;
            todoItemFromDb.TotalKeys = item.TotalKeys;
            todoItemFromDb.IsRunPressed = false;
            todoItemFromDb.IsStopPressed = true;
            todoItemFromDb.ProcessingMessage = message;
            await context.SaveChangesAsync();
        }
        await _hubContext.Clients.All.SendAsync("UpdateTodos", todoItemFromDb, _stoppingToken);
        Console.WriteLine($"Процесс остановлен, нажата кнопкаIsStopPressed => {item.IsStopPressed}");
    }
    private async Task HandleExceptionAsync(TodoItem item, Exception ex, CancellationToken _stoppingToken)
    {
        item.ProcessingMessage = $"{DateTime.Now} Error: {ex.Message}";
        item.IsStopPressed = true;
        item.IsRunPressed = false;
        await _sqliteDbContext.CreateDbContext().UpdateTodo(item);
        await _sqliteDbContext.CreateDbContext().SaveChangesAsync();
        await _hubContext.Clients.All.SendAsync("UpdateTodos", item, _stoppingToken);
        Console.WriteLine("HandleExceptionAsync:" + ex.Message);
    }

    private async Task<bool> ReloadIsStopPressedByItemId(int Id)
    {
        var ReloadedTodoItemById = await _sqliteDbContext.CreateDbContext().LoadTodoItem(Id);
        return ReloadedTodoItemById.IsStopPressed;
    }
    private async Task<bool> ReloadIsRunPressedByItemId(int Id)
    {
        var ReloadedTodoItemById = await _sqliteDbContext.CreateDbContext().LoadTodoItem(Id);
        return ReloadedTodoItemById.IsRunPressed;
    }
    private async Task AiProcessDatabaseAsync(CancellationToken stoppingToken)
    {
        // получение списка TODO задач для обработки
        using var sqlite = _sqliteDbContext.CreateDbContext();
        var todoItems = await sqlite.TodoItems.ToListAsync();

        foreach (var item in todoItems)
        {
            if (!await ReloadIsRunPressedByItemId(item.Id))
            {
                await StopProcessingAsync(item, "Готово к запуску. 💤", stoppingToken);
            }
            else
            {
                item.CompletedKeys = 0;
                item.TotalKeys = 0;
                await UpdateTodoItemStateAsync(item, "Идёт выполнение... ⌛", stoppingToken);
                await Task.Delay(1200);
                Console.WriteLine("item.IsRunPressed =>" + item.IsRunPressed);

                try
                {
                    string conStringDBA = SelectDb.ConStringDBA(item);
                    using var Context = await _dbContextFactory.CreateDbContext(item.DbType, conStringDBA, item.Scheme);

                    //получение записей
                    List<SprSpeechTable> AudioList = await EFCoreQuery.GetSpeechRecords(item.StartDateTime, item.EndDateTime, item.MoreThenDuration, Context, IgnoreRecordTypes);
                    item.TotalKeys = AudioList.Count;

                    //если записи отсутствуют => к следующему TODO json
                    if (item.TotalKeys <= 0)
                    {
                        if (item.IsCyclic)
                        {
                            await UpdateTodoItemStateAsync(item, $"Обработано {item.CompletedKeys}/{item.TotalKeys}. Ожидание повторного запуска.", stoppingToken);
                        }
                        else
                        {
                            await StopProcessingAsync(item, $"Обработано {item.CompletedKeys}/{item.TotalKeys}.", stoppingToken);
                        }

                        continue; //к следующей итерации todoItems
                    }

                    //если записи есть => действие с записями
                    await UpdateTodoItemStateAsync(item, $"Идёт выполнение: {item.CompletedKeys}/{item.TotalKeys}", stoppingToken);
                    int ProcessedWhisper = 0; //выполнено Whisper = 0
                    foreach (var entity in AudioList)
                    {
                        // Остановить процесс, если нажата кнопка
                        if (await ReloadIsStopPressedByItemId(item.Id))
                        {
                            await StopProcessingAsync(item, $"{DateTime.Now} Остановлено: {item.CompletedKeys} / {item.TotalKeys}", stoppingToken);
                            break;
                        }

                        // PreText => get PreText for operator or PreTextDefault
                        string preText = await Params.GetPreTextAsync(entity.SSourcename);

                        // Db => get audio (left, right, recordType)
                        var (audioDataLeft, audioDataRight, recordType) = await EFCoreQuery.GetAudioDataAsync(entity.SInckey, Context);
                        Console.WriteLine($"Audio data for key {entity.SInckey} loaded successfully. recordType = " + recordType);

                        // FFMpeg or Decoder => audio to folder
                        string audioFilePath = Path.Combine(_configuration["AudioPathForProcessing"], $"{entity.SInckey}.wav");
                        bool result = await DbToAudioConverter.FFMpegDecoder(audioDataLeft, audioDataRight, recordType, audioFilePath, _configuration);
                        if (!result) continue;

                        // WHISPER
                        //Task<string> _recognizedText = _whisper.RecognizeSpeechAsync(audioFilePath, _configuration); //асинхронно, не ждём
                        //(string languageCode, string detectedLanguage) = await _whisper.DetectLanguageAsync(audioFilePath, _configuration);
                        //string recognizedText = await _recognizedText; //дожидаемся _recognizedText...
                        string languageCode = "";
                        string detectedLanguage = "";
                        string recognizedText = "";
                        ProcessedWhisper++;
                        await Task.Delay(3000);


                        // Остановить процесс, если нажата кнопка
                        if (await ReloadIsStopPressedByItemId(item.Id))
                        {
                            await StopProcessingAsync(item, $"{DateTime.Now} Остановлено: {item.CompletedKeys} / {item.TotalKeys}", stoppingToken);
                            break;
                        }



                        // Temprorary push string to Notice to aviod repeated process with entity
                        entity.SNotice = "TempRecord";
                        Context.SprSpeechTables.Update(entity);
                        await Context.SaveChangesAsync();



                        _logger.LogInformation("entity.SNotice = \"TempRecord\"");

                        // Delete earlier created file
                        Files.DeleteFilesByPath(audioFilePath);

                        // OLLAMA + ORACLE => Run task !!!_WITHOUT await
                        using var NewContext = await _dbContextFactory.CreateDbContext(item.DbType, conStringDBA, item.Scheme);
                        _logger.LogInformation("NewContext");
                        //await ProcessOllamaAndUpdateEntityAsync(entity.SInckey, recognizedText, languageCode, detectedLanguage, preText, _configuration["OllamaModelName"], _configuration, entity, NewContext, sqlite, item);
                        languageCode = "1";
                        detectedLanguage = "1";
                        recognizedText = "1";
                        item.CompletedKeys++;
                        await Task.Delay(3000);

                        _logger.LogInformation("ProcessOllamaAndUpdateEntityAsync");


                        // TODO
                        // разрешить "вырываться вперёд не более чем на N раз" и ProcessedAi
                        while (ProcessedWhisper - 2 > item.CompletedKeys)
                        {
                            await Task.Delay(5000);
                            ConsoleCol.WriteLine("Delay", ConsoleColor.Yellow);
                            ConsoleCol.WriteLine("ProcessedOllama / ProcessedWhisper => " + item.CompletedKeys + "/" + ProcessedWhisper, ConsoleColor.Yellow);
                            
                            if (await ReloadIsStopPressedByItemId(item.Id))
                            {
                                await StopProcessingAsync(item, $"{DateTime.Now} Остановлено: {item.CompletedKeys} / {item.TotalKeys}", stoppingToken);
                                break;
                            }
                        }
                        _logger.LogInformation("ProcessedOllama / ProcessedWhisper => " + item.CompletedKeys + "/" + ProcessedWhisper);

                        Console.WriteLine($"IsStopPressed => {item.IsStopPressed}");

                        await UpdateTodoItemStateAsync(item, $"Идёт выполнение: {item.CompletedKeys}/{item.TotalKeys}", stoppingToken);
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
                    if (item.IsCyclic)
                    {
                        await UpdateTodoItemStateAsync(item, $"Выполнено: {item.CompletedKeys}/{item.TotalKeys}. Ожидание повторного запуска.", stoppingToken);
                        
                    }
                    else
                    {
                        await StopProcessingAsync(item, $"Процесс остановлен. Выполнено: {item.CompletedKeys}/{item.TotalKeys}.", stoppingToken);
                    }
                    Console.WriteLine("finally");
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
            ConsoleCol.WriteLine("Ошибка при обработке Ollama и обновлении сущности EFCore: " + ex.Message, ConsoleColor.Red);

            // TODO if (MistakesCount > 10)
            //StateService.IsStopPressed = true;
        }

    }

}

