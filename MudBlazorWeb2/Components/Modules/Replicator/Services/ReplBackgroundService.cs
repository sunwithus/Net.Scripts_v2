//ReplBackGroundService.cs

using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.EntityFrameworkCore;
using MudBlazorWeb2.Components.Modules.Replicator;

using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazorWeb2.Components.Modules._Shared;
using MudBlazorWeb2.Components.EntityFrameworkCore.Sprutora;
using MudBlazorWeb2.Components.Methods;

public class ReplBackgroundService : BackgroundService
{
    //private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private FileLogger _fileLogger;
    private readonly IHubContext<ReplicatorHub> _hubContext;
    private readonly IDbContextFactory _dbContextFactory;

    public ReplBackgroundService(IDbContextFactory dbContextFactory, IConfiguration configuration, IHubContext<ReplicatorHub> hubContext)
    {
        _dbContextFactory = dbContextFactory;
        _configuration = configuration;
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

        //Test ///////////////////////////////////////////
        /*
        string conStringDBA = $"Host=localhost;Database=test;Username=postgres;Password=postgres;";
        using var context = _dbContextFactory.CreateDbContext("Postgres", conStringDBA, "test");
        var maxKey = context.SprSpData1Tables.Max(x => x.SInckey);
        Console.WriteLine("maxKey: " + maxKey);
        */
        ///////////////////////////////////////////

        var JsonFiles = Directory.EnumerateFiles(pathToAudio, "*.json");
        if (!JsonFiles.Any())
        {
            Console.WriteLine("Нет json файлов для обработки.");
            return;
        }

        foreach (var file in JsonFiles) 
        {
            //var jsonData = new SimpleJson<DataForBackgroungService>(file);
            //await jsonData.LoadItemsAsync();
            //var paramsRepl = jsonData.GetItems().FirstOrDefault();

            var json = await File.ReadAllTextAsync(file);
            DataForBackgroungService JsonData = JsonSerializer.Deserialize<DataForBackgroungService>(json);
            var paramsRepl = JsonData;

            await ReplicateAudioFromDirectory(paramsRepl, cancellationToken);
            await Task.Delay(500);

            Files.DeleteDirectory(paramsRepl.PathToSaveTempAudio);
            Files.DeleteFilesByPath(file);
        }
    }

    private async Task ReplicateAudioFromDirectory(DataForBackgroungService paramsRepl, CancellationToken cancellationToken)
    {
        /*if (conStringDBA != "")
        {
            _context = DbContextFactory.CreateDbContext(SettingsDb.DbType, conStringDBA, SettingsDb.Scheme);
            message += $"Соединение с {SettingsDb.DbType} установлено!\n";
            long? maxKey = await _context.SprSpeechTables.MaxAsync(x => x.SInckey);
            if (maxKey > 0)
                message += "Схема: " + SettingsDb.Scheme + " выбрана! \nМаксимальный идентификатор: " + maxKey + ".";
            await _context.Database.CloseConnectionAsync();
        }*/

        //paramsRepl.DbType
        var optionsBuilder = OracleDbContext.ConfigureOptionsBuilder(paramsRepl.DbConnectionSettings);
        using var context = _dbContextFactory.CreateDbContext(paramsRepl.DbType, paramsRepl.DbConnectionSettings, paramsRepl.Scheme);
        //using var context = new OracleDbContext(optionsBuilder.Options);

        //string scheme = paramsRepl.DbScheme;
        //await context.Database.OpenConnectionAsync();
        //await context.Database.ExecuteSqlRawAsync($"ALTER SESSION SET CURRENT_SCHEMA = {scheme}");
        
        var filesAudio = Directory.EnumerateFiles(paramsRepl.PathToSaveTempAudio);
        if (!filesAudio.Any())
        {
            Console.WriteLine("Нет аудио файлов для репликации.");
            return;
        }

        foreach (var filePath in filesAudio)
        {
            try
            {
                await ProcessSingleAudio(context, filePath, paramsRepl.SourceName);
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
        _fileLogger.Log($"Выполнено. Источник: {paramsRepl.SourceName}, схема БД: {paramsRepl.Scheme} Записано {filesAudio.Count()} файлов.");
    }

    private async Task ProcessSingleAudio(BaseDbContext context, string filePath, string sourceName)
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
        string durationString = string.Format("+00 {0:D2}:{1:D2}:{2:D2}.000000", durationOfWav / 3600, (durationOfWav % 3600) / 60, durationOfWav % 60);
        TimeSpan durationTimeSpan = TimeSpan.FromSeconds(durationOfWav);

        return new SprSpeechTable
        {
            SInckey = maxKey + 1,
            SType = 0,
            SPrelooked = 0,
            SDeviceid = "MEDIUM_R",
            SDatetime = fileData.Timestamp,
            SDuration = durationTimeSpan,
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
                SInckey = maxKey + 1,
                SOrder = 1,
                SRecordtype = codec,
                SFspeech = audioDataLeft,
                SRspeech = audioDataRight
        };
    }

    private async Task SaveEntitiesToDatabase(BaseDbContext context, SprSpeechTable speechEntry, SprSpData1Table dataEntry)
    {
        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            context.SprSpeechTables.Add(speechEntry);
            context.SprSpData1Tables.Add(dataEntry);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch(Exception ex)
        {
            Console.WriteLine("Error SaveEntitiesToDatabase => " + ex);
            await transaction.RollbackAsync();
            throw;
        }
    }
}
