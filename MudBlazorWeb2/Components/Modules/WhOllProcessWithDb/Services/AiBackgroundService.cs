//AiBackGroundService.cs

using Microsoft.AspNetCore.Components;
using MudBlazorWeb2.Components.EntityFrameworkCore;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.TodoList;
using MudBlazorWeb2.Components.Pages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorWeb2.Components.Modules._Shared;
using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.Services;
using System.Configuration;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb;

public class AiBackgroundService : BackgroundService
{
    private readonly ILogger<AiBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly SettingsService _settingsService;
    private readonly WhisperService _whisper;
    private readonly OllamaService _ollama;
    private readonly IDbContextFactory<SqliteDbContext> _sqliteDbContext; 
    private readonly IHubContext<TodoHub> _hubContext;



    public AiBackgroundService(ILogger<AiBackgroundService> logger, IConfiguration configuration, SettingsService settingsService, WhisperService whisperService, OllamaService ollamaService, IDbContextFactory<SqliteDbContext> sqliteDbContext, IHubContext<TodoHub> hubContext)
    {
        _logger = logger;
        _configuration = configuration;
        _settingsService = settingsService;
        _whisper = whisperService;
        _ollama = ollamaService;
        _sqliteDbContext = sqliteDbContext;
        _hubContext = hubContext;
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
            await Task.Delay(5000, stoppingToken); // delay
        }
    }

    private async Task AiProcessDatabaseAsync(CancellationToken stoppingToken)
    {
        using var sqlite = _sqliteDbContext.CreateDbContext();
        var todoItems = await sqlite.TodoItems.ToListAsync(stoppingToken);

        foreach (var item in todoItems)
        {
            if(item.IsRunPressed)
            {
                bool isConnected = true;
                //(bool isConnected, _) = await DatabaseConnection.Test(item);
                if (isConnected)
                {
                    item.IsRunning = true;
                    item.ProcessingMessage = "Обработка выполняется...";
                    sqlite.TodoItems.Update(item);
                    await sqlite.SaveChangesAsync();

                    string conStringDBA = $"User Id={item.User};Password={item.Password};Data Source={item.ServerAddress};";
                    using (var db = new OracleDbContext(new DbContextOptionsBuilder<OracleDbContext>().UseOracle(conStringDBA).Options))
                    {
                        await db.Database.OpenConnectionAsync();
                        //await db.Database.ExecuteSqlRawAsync($"ALTER SESSION SET CURRENT_SCHEMA = {item.Scheme}", stoppingToken);
                        await db.Database.ExecuteSqlAsync($"ALTER SESSION SET CURRENT_SCHEMA = {item.Scheme}", stoppingToken);
                        ConsoleCol.WriteLine($"AiProcessDatabaseAsync => Выбранная схема: {item.Scheme}", ConsoleColor.DarkCyan);

                        var ignoreRecordTypes = _configuration.GetSection("AudioConverter:IgnoreRecordTypes").Get<List<string>>();
                        var audioList = await EFCoreQuery.GetSpeechRecords(item.StartDateTime, item.EndDateTime, item.DurationString, db, ignoreRecordTypes);
                        item.TotalKeys = audioList.Count;

                        foreach (var entity in audioList)
                        {
                            // Остановить процесс, если нажата кнопка
                            if (item.IsStopPressed)
                            {

                                item.ProcessingMessage = $"{DateTime.Now} Выполнено: {item.CompletedKeys} / {item.TotalKeys}";
                                sqlite.TodoItems.Update(item);
                                await sqlite.SaveChangesAsync();
                                break;
                            }
                        }
                    }

                        await Task.Delay(1500);
                    // процесс запущен

                    item.CompletedKeys++;
                    sqlite.TodoItems.Update(item);
                    await sqlite.SaveChangesAsync();

                    // Отправка обновления через хаб
                    await _hubContext.Clients.All.SendAsync("UpdateTodos", item, stoppingToken);

                    await Task.Delay(1500);
                    ConsoleCol.WriteLine(item.Title, ConsoleColor.DarkCyan);




                    sqlite.TodoItems.Update(item);
                    await sqlite.SaveChangesAsync();
                    await _hubContext.Clients.All.SendAsync("UpdateTodos", item, stoppingToken);

                }
            }
        }

    }

}

