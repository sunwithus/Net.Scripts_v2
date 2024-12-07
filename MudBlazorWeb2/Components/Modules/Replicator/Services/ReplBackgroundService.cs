//ReplBackGroundService.cs

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MudBlazorWeb2.Components.EntityFrameworkCore;
using MudBlazorWeb2.Components.Modules.Replicator;
using MudBlazorWeb2.Components.Methods;

using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorWeb2.Components.Modules._Shared;
using MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;

public class ReplBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly SettingsService _settingsService;
    private FileLogger _fileLogger;

    private readonly IHubContext<ReplicatorHub> _hubContext;

    public ReplBackgroundService(IServiceScopeFactory scopeFactory, IConfiguration configuration, SettingsService settingsService, IHubContext<ReplicatorHub> hubContext)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _settingsService = settingsService;
        _hubContext = hubContext;
        _fileLogger=new FileLogger(Path.Combine(AppContext.BaseDirectory, "Logs/replicator.log"));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Выполнение задачи с периодическим интервалом
            try
            {
                await CheckFilesToReplicate(stoppingToken); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в ReplBackgroundService: {ex.Message}");
                _fileLogger.Log($"Ошибка в ReplBackgroundService: {ex.Message}");
            }

            // Задержка между циклами
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task CheckFilesToReplicate(CancellationToken cancellationToken) 
    {
        string pathToAudio = _configuration["AudioPathForReplicator"];

        if (!Directory.Exists(pathToAudio))
        {
            Console.WriteLine("Директория для обработки аудиофайлов отсутствует.");
            return;
        }

        var JsonFiles = Directory.EnumerateFiles(pathToAudio, "*.json");
        if (!JsonFiles.Any())
        {
            Console.WriteLine("Нет json файлов для обработки.");
            return;
        }

        foreach (var file in JsonFiles) 
        {
            var json = await File.ReadAllTextAsync(file);
            DataForBackgroungService JsonData = JsonSerializer.Deserialize<DataForBackgroungService>(json);

            await ReplicateAudioFromDirectory(JsonData, cancellationToken);
            await Task.Delay(500);

            Files.DeleteDirectory(JsonData.PathToSaveTempAudio);
            Files.DeleteFilesByPath(file);
        }
    }

    private async Task ReplicateAudioFromDirectory(DataForBackgroungService JsonData, CancellationToken cancellationToken)
    {
        var optionsBuilder = OracleDbContext.ConfigureOptionsBuilder(JsonData.DbConnectionSettings);
        using var context = new OracleDbContext(optionsBuilder.Options);

        string scheme = JsonData.DbScheme;
        await context.Database.OpenConnectionAsync();
        await context.Database.ExecuteSqlRawAsync($"ALTER SESSION SET CURRENT_SCHEMA = {scheme}");
        
        var filesAudio = Directory.EnumerateFiles(JsonData.PathToSaveTempAudio);
        if (!filesAudio.Any())
        {
            Console.WriteLine("Нет аудио файлов для репликации.");
            return;
        }

        foreach (var filePath in filesAudio)
        {
            try
            {
                await ProcessSingleAudio(context, filePath, JsonData.SourceName);
                Console.WriteLine($"Файл обработан: {filePath}");
                //File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке файла {filePath}: {ex.Message}");
                await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"❌ Ошибка при обработке файла {filePath}: {ex.Message}", cancellationToken);
                throw;
            }
        }
        _fileLogger.Log($"Выполнено. Источник: {JsonData.SourceName}, схема БД: {JsonData.DbScheme} Записано {filesAudio.Count()} файлов.");
    }

    private async Task ProcessSingleAudio(OracleDbContext context, string filePath, string sourceName)
    {
        string codec = "PCMA";
        var _maxKey = context.SprSpeechTables.MaxAsync(x => (long?)x.SInckey);

        var (durationOfWav, audioDataLeft, audioDataRight) = await AudioToDbConverter.FFmpegStream(filePath, _configuration["PathToFFmpegExeForReplicator"]);
        long maxKey = await _maxKey ?? 0;
        Parse.ParsedIdenties fileData = Parse.FormFileName(filePath); //если не удалось, возвращает {DateTime.Now, "", "", "", 2}
        string isIdentificators = (fileData.Talker == "" && fileData.Caller == "" && fileData.IMEI == "") ? "✔️ без идентификаторов" : "✅ с идентификаторами";
        // Создание талиц записи
        var speechTableEntity = CreateSpeechTableEntity(fileData, durationOfWav, codec, maxKey, sourceName);
        var data1TableEntity = CreateData1TableEntity(audioDataLeft, audioDataRight, codec, maxKey);

        await SaveEntitiesToDatabase(context, speechTableEntity, data1TableEntity);
        await _hubContext.Clients.All.SendAsync("ReceiveMessage", $"Записан {isIdentificators}: {filePath}", CancellationToken.None);

    }

    private SprSpeechTable CreateSpeechTableEntity(Parse.ParsedIdenties fileData, int durationOfWav, string codec, long maxKey, string sourceName)
    {
        return new SprSpeechTable
        {
            SInckey = maxKey + 1,
            SType = 0,
            SPrelooked = 0,
            SDeviceid = "MEDIUM_R",
            SDatetime = fileData.Timestamp,
            SDuration = string.Format("+00 {0:D2}:{1:D2}:{2:D2}.000000", durationOfWav / 3600, (durationOfWav % 3600) / 60, durationOfWav % 60),
            SSysnumber3 = fileData.IMEI,
            SSourcename = sourceName,
            STalker = fileData.Talker,
            SUsernumber = fileData.Caller,
            SCalltype = fileData.Calltype,
            SEventcode = codec
        };
    }

    private SprSpData1Table CreateData1TableEntity(byte[]? audioDataLeft, byte[]? audioDataRight, string codec, long maxKey)
    {
        return new SprSpData1Table
        {
                Id = maxKey + 1,
                SOrder = 1,
                SRecordtype = codec,
                SFspeech = audioDataLeft,
                SRspeech = audioDataRight
        };
    }

    private async Task SaveEntitiesToDatabase(OracleDbContext context, SprSpeechTable speechEntry, SprSpData1Table dataEntry)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            context.SprSpeechTables.Add(speechEntry);
            context.SprSpData1Tables.Add(dataEntry);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
