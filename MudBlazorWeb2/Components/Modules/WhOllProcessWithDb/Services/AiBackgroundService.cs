//AiBackGroundService.cs

using Microsoft.AspNetCore.Components;
using MudBlazorWeb2.Components.EntityFrameworkCore;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.TodoList;
using MudBlazorWeb2.Components.Pages;
using Microsoft.AspNetCore.SignalR;
using SQLitePCL;
using MudBlazorWeb2.Components.Modules._Shared;

public class AiBackgroundService : BackgroundService
{
    private readonly ILogger<AiBackgroundService> _logger;
    private readonly IConfiguration _configuration;
    private readonly SettingsService _settingsService;
    private readonly WhisperService _whisperService;
    private readonly OllamaService _ollamaService;
    private readonly IHubContext<TodoHub> _hubContext;
    //private readonly SqliteDbContext _sqlite;
    

    public AiBackgroundService(ILogger<AiBackgroundService> logger, IConfiguration configuration, SettingsService settingsService, WhisperService whisperService, OllamaService ollamaService, IHubContext<TodoHub> hubContext/*, SqliteDbContext sqlite*/)
    {
        _logger = logger;
        _configuration = configuration;
        _settingsService = settingsService;
        _whisperService = whisperService;
        _ollamaService = ollamaService;
        _hubContext = hubContext;
        //_sqlite = sqlite;
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
        //TodoListBase todoList = new();
        //List<TodoItem> items = await todoList.LoadTodos();

        //var todos = _sqlite.TodoItems.ToList();
        /*foreach (var item in items)
        {
            if(item.IsRunPressed)
            {
                (bool isConnected, _) = await TodoPage.TestDatabaseConnection(item);
                if (isConnected)
                {
                    // процесс запущен
                    item.IsRunning = true;
                    
                    //await todoList.SaveTodos(item);

                    item.Title = item.Title + "Qq";
                    //_sqlite.TodoItems.Update(item);
        */
                    // Отправка обновления через хаб
                    //await _hubContext.Clients.All.SendAsync("UpdateTodos", item, stoppingToken);
                    //_progressService.UpdateProgress(55, 555);

                    await Task.Delay(2000);
                    ConsoleCol.WriteLine("item.Title", ConsoleColor.DarkCyan);



                //}
            //}
        //}

    }

}

