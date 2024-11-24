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

public class AudioReplicationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly SettingsService _settingsService;

    public AudioReplicationService(IServiceScopeFactory scopeFactory, IConfiguration configuration, SettingsService settingsService)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _settingsService = settingsService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Выполнение задачи с периодическим интервалом
            try
            {
                await ProcessAudioFiles(stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в AudioReplicationService: {ex.Message}");
            }

            // Задержка между циклами
            //await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Пауза 5 минут
            await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken); // Пауза 5 минут
        }
    }

    private async Task ProcessAudioFiles(CancellationToken cancellationToken)
    {
        string pathToAudio = _configuration["AudioPathForReplicator"];

        if (!Directory.Exists(pathToAudio))
        {
            Console.WriteLine("Директория для обработки аудиофайлов отсутствует.");
            return;
        }

        //TODO расширить форматы аудио
        var files = Directory.GetFiles(pathToAudio, "*.wav");
        if (!files.Any())
        {
            Console.WriteLine("Нет файлов для обработки.");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OracleDbContext>();

        var settingsDB = _settingsService.GetSettings().SettingsReplicator.OraItems;
        string connectionString = $"User Id={settingsDB.User};Password={settingsDB.Password};Data Source={settingsDB.DataSource};";
        string scheme = settingsDB.Scheme;

        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        await dbContext.Database.ExecuteSqlRawAsync($"ALTER SESSION SET CURRENT_SCHEMA = {scheme}", cancellationToken);

        foreach (var filePath in files)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                // Обработка файла
                await ProcessFile(dbContext, filePath);
                Console.WriteLine($"Файл обработан: {filePath}");

                // Удаление файла после обработки
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке файла {filePath}: {ex.Message}");
            }
        }
    }

    private async Task ProcessFile(OracleDbContext dbContext, string filePath)
    {
        // Пример логики обработки файла
        // (добавьте вашу логику обработки аудиофайлов)
        string codec = "PCMA";
        long maxKey = await dbContext.SprSpeechTable.MaxAsync(x => (long?)x.Id) ?? 0;

        var (durationOfWav, audioDataLeft, audioDataRight) = await AudioToDbConverter.FFmpegStream(filePath, _configuration["PathToFFmpegExeForReplicator"]);

        Parse.ParsedIdenties fileData = Parse.FormFileName(filePath); //если не удалось, возвращает {DateTime.Now, "", "", "", 2}

        // Создание талиц записи
        var speechTableEntity = CreateSpeechTableEntity(fileData, durationOfWav, codec, maxKey);

        var dataEntry = new SPR_SP_DATA_1_TABLE
        {
            Id = maxKey + 1,
            Order = 1,
            Recordtype = codec,
            Fspeech = audioDataLeft,
            Rspeech = audioDataRight
        };

        using var transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            dbContext.SprSpeechTable.Add(speechTableEntity);
            dbContext.SprSpData1Table.Add(dataEntry);
            await dbContext.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private SPR_SPEECH_TABLE CreateSpeechTableEntity(Parse.ParsedIdenties fileData, int durationOfWav, string codec, long maxKey)
    {
        return new SPR_SPEECH_TABLE
        {
            Id = maxKey + 1,
            Type = 0,
            Prelooked = 0,
            Deviceid = "MEDIUM_R",
            Datetime = fileData.Timestamp,
            Duration = string.Format("+00 {0:D2}:{1:D2}:{2:D2}.000000", durationOfWav / 3600, (durationOfWav % 3600) / 60, durationOfWav % 60),
            Sysnumber3 = fileData.IMEI,
            Sourcename = "sourceName",
            Talker = fileData.Talker,
            Usernumber = fileData.Caller,
            Calltype = fileData.Calltype,
            Eventcode = codec
        };
    }
}
