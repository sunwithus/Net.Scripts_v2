//AiBackGroundService.cs

using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore;

using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb;
using static MudBlazorWeb2.Components.Pages.Todo;
using MudBlazorWeb2.Components.Methods;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.TodoList;
using MudBlazorWeb2.Components.Pages;
using static MudBlazor.Colors;
using System.Diagnostics;
using System.Threading;
using Microsoft.AspNetCore.Components;

public class AiBackgroundService : BackgroundService
{
    private readonly ILogger<AiBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly SettingsService _settingsService;
    private readonly WhisperService _whisperService;
    private readonly OllamaService _ollamaService;
    private readonly ProgressService _progressService;


    public AiBackgroundService(ILogger<AiBackgroundService> logger, IConfiguration configuration, SettingsService settingsService, WhisperService whisperService, OllamaService ollamaService, ProgressService progressService)
    {
        _logger = logger;
        _configuration = configuration;
        _settingsService = settingsService;
        _whisperService = whisperService;
        _ollamaService = ollamaService;
        _progressService = progressService;
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
        string FilePath = Path.Combine(AppContext.BaseDirectory, "todoitems.json");
        var JsonTodoItems = new SimpleJson<TodoItem>(FilePath);
        await JsonTodoItems.LoadItemsAsync();

        var items = JsonTodoItems.GetItems();
        foreach (var item in items)
        {
            
            if(item.IsRunPressed)
            {
                (bool isConnected, _) = await TodoPage.TestDatabaseConnection(item);
                if (isConnected)
                {

                    // процесс запущен
                    item.IsRunning = true;
                    //await JsonTodoItems.UpdateItemAsync(item, x => x.Id == item.Id);
                    //await JsonTodoItems.SaveItemsAsync();

                    //_progressService.UpdateProgress(55, 555);

                    await Task.Delay(2000);
                    ConsoleCol.WriteLine(item.Title, ConsoleColor.DarkCyan);



                }
            }
        }

    }

}

