/*using System.Diagnostics;
using Microsoft.AspNetCore.SignalR;

using FFMpegCore;
using FFMpegCore.Pipes;

using Oracle.ManagedDataAccess.Client;

public class AudioProcessor
{
    private readonly WhisperService _whisperService;
    private readonly OllamaService _ollamaService;
    private readonly DatabaseService _databaseService;

    private readonly List<string> _codecs = new List<string>()
        {
            "UMTS_AMR", "EVRC", "GSM"
            ,"WAVE_FILE", "RPE-LTP", "DAMPS",
            "PCM-128", "QCELP-8", "QCELP-13", "ADPCM", "AMBE.HR_IRIDIUM", "A-LAW", "AMBE_INMARSAT_MM", "APC.HR_INMARSAT_B", "IMBE_INMARSAT_M",
            "AME", "ACELP_TETRA", "GSM.EFR_ABIS", "GSM.HR_ABIS", "GSM.AMR_ABIS", "GSM_ABIS", "LD-CELP", "E-QCELP", "ATC", "PSI-CELP", "AMBE.GMR1", "AMBE.GMR2", "AMBE.INMARSAT_BGAN", "ADM.UAV",
            "PCMA", "PCMU", "IPCMA", "IPCMU", "L8", "IL8", "L16", "IL16", "G.723.1", "G.726-32", "G.728", "G.729", "GSM.0610", "ILBC-13", "ILBC-15", "PDC.FR", "PDC.EFR", "PDC.HR",
            "IDEN.FR", "APCO-25", "RP-CELP", "IDEN.HR"
        };

    private readonly List<string> _doNotWorkWith = new List<string>()
    {
        "FAXDATA_GSM","DATA_GSM", "BINARY", "FAXDATA_CDMA", "Paging Response", "DMR"
    };

    private bool isProcessing = false;
    private bool isHardStopPressed = false;

    public AudioProcessor(WhisperService whisperService, OllamaService ollamaService, DatabaseService databaseService)
    {
        _whisperService = whisperService;
        _ollamaService = ollamaService;
        _databaseService = databaseService;
    }

    public async Task ProcessAudioFilesAsync(DateTime startDate, DateTime endDate, string schemeName, Action<int, int> updateProgress, string modelName, string preText)
    {
        isProcessing = true;
        isHardStopPressed = false;

        var inkKeys = await _databaseService.GetInkKeysAsync(startDate, endDate, schemeName);
        int totalKeys = inkKeys.Count;
        int processedKeys = 0;

        updateProgress(processedKeys, totalKeys);


            foreach (int key in inkKeys)
            {
                if (isHardStopPressed)
                {
                    Console.WriteLine("Hard stop pressed. Stopping processing.");
                    break;
                }
                if (!isProcessing) break;

                var transaction = await _databaseService.BeginTransactionAsync();
                try
                {
                    int seconds = 0;
                    int secondsTotal = 0;
                    DateTime startTime = DateTime.Now;

                    // Максимальное количество попыток
                    const int maxRetries = 2;
                    int currentAttempt = 0;
                    bool success = false;

                    var (audioDataLeft, audioDataRight, recordType) = await _databaseService.GetAudioDataAsync(key, schemeName);
                    string audioFilePath = Path.Combine("C:\\temp\\4", $"{key}.wav");

                    if (recordType == "UMTS.AMR-WB" || recordType == "EVS" || _doNotWorkWith.Contains(recordType)) continue; // пока с этими кодеком не работаем, а также BINARY, FAX...
                
                    if (recordType != null && _codecs.Contains(recordType))
                    {
                        Console.WriteLine("using decoder!!!");
                        await ConvertToWavUsingDecoder(audioDataLeft, audioDataRight, audioFilePath, recordType);
                        await Task.Delay(200); //возможно файл не успевает сохраниться, поэтому пауза
                    }
                    else
                    {
                        Console.WriteLine("using ffmpeg!!!");
                        Console.WriteLine("recordType is " + recordType);
                        await ConvertToWavAsyncStream(audioDataLeft, audioDataRight, audioFilePath);
                        //await ConvertToWavAsyncFile(audioDataLeft, audioDataRight, audioFilePath);
                        await Task.Delay(200); //возможно файл не успевает сохраниться, поэтому пауза
                    }
             
                    //Произошла ошибка на определении языка: Error while copying content to a stream.
                    //при повторном запуске в этом же месте ошибка может не произойти
                    //поэтому выполняем несколько попыток
                    string[] recognizedLanguage = new string[] {"", ""};
                    string recognizedText = "";
                    string languageCode = "";
                    string detectedLanguage = "";

                    while (currentAttempt < maxRetries && !success)
                    {
                        try
                        {
                            recognizedLanguage = await _whisperService.DetectLanguageAsync(audioFilePath);
                            languageCode = recognizedLanguage[0].ToLower();
                            detectedLanguage = recognizedLanguage[1];

                            recognizedText = await _whisperService.RecognizeSpeechAsync(audioFilePath, languageCode);

                            success = true;
                        }
                        catch (Exception ex)
                        {
                            currentAttempt++;
                            if (currentAttempt >= maxRetries)
                            {
                                // Логируем ошибку, если все попытки не удались
                                Console.WriteLine($"Ошибка при обработке ключа {key}: {ex.Message}");
                            }
                            else
                            {
                                // Ждем перед повторной попыткой
                                await Task.Delay(300);
                            }
                        }
                    }



                Console.WriteLine();
                    Console.WriteLine("recognizedLanguage: " + recognizedLanguage[0]);
                    Console.WriteLine("languageCode: " + languageCode);

                    Console.WriteLine();
                    Console.WriteLine("recognizedText: " + recognizedText);

                    seconds = (int)(DateTime.Now - startTime).TotalSeconds;
                    secondsTotal += seconds;
                    Console.WriteLine($"_whisperService.RecognizeSpeechAsync за: {seconds} сек.");
                    startTime = DateTime.Now;

                    string responseOllamaText = "Аудио не транскрибировано";
                    if (recognizedText != "")
                    {
                        responseOllamaText = await _ollamaService.SendTextForAnalysisAsync(preText, recognizedText, modelName);
                        if (languageCode != "ru" && languageCode != "uk")
                        {
                            string preTextToTranslate = "Сделай перевод текста на русский язык, ответом должен должен быть только перевод, без дополнительных фраз. Вот текст: ";
                            recognizedText = await _ollamaService.SendTextForAnalysisAsync(preTextToTranslate, recognizedText, modelName);
                            recognizedText = $"Перевод с {detectedLanguage.ToUpper()} языка: \n" + recognizedText;
                        }
                    }
                
                    seconds = (int)(DateTime.Now - startTime).TotalSeconds;
                    secondsTotal += seconds;
                    Console.WriteLine($"_ollamaService.SendTextForAnalysisAsync за: {seconds} сек.");
                    startTime = DateTime.Now;

                    await _databaseService.InsertCommentAsync(key, recognizedText, detectedLanguage, responseOllamaText, transaction, schemeName, modelName);
                    File.Delete(audioFilePath);
                    secondsTotal += seconds;
                    Console.WriteLine($"Всего прошло времени: {secondsTotal} сек.");


                    processedKeys++;
                    updateProgress(processedKeys, totalKeys);

                    Console.WriteLine($"Обработка в процессе: Processed: {processedKeys}, Total: {totalKeys}");
                
                    if (!isProcessing) break;

                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
     
        }
    }
    public async Task StopProcessingAsync()
    {
        isProcessing = false;
        isHardStopPressed = true;
        await Task.Delay(1);
    }

    public static async Task ConvertToWavAsyncStream(byte[] audioDataLeft, byte[] audioDataRight, string outputFilePath)
    {
        using var streamLeft = new MemoryStream(audioDataLeft);
        using var streamRight = audioDataRight != null ? new MemoryStream(audioDataRight) : null;
        var ffmpegArgs = FFMpegArguments.FromPipeInput(new StreamPipeSource(streamLeft));
        string rightArgument = "";

        if (streamRight != null)
        {
            ffmpegArgs = ffmpegArgs.AddPipeInput(new StreamPipeSource(streamRight));
            rightArgument = "-filter_complex amix=inputs=2:duration=first:dropout_transition=2";
        }
        await ffmpegArgs
        .OutputToFile(outputFilePath, true, options => options
            .ForceFormat("wav")
            .WithCustomArgument("-codec:a pcm_s16le -b:a 128k -ar 16000 -ac 1")
            .WithCustomArgument($"{rightArgument}")
        )
        .ProcessAsynchronously();
    }

    public static async Task ConvertToWavAsyncFile(byte[] audioDataLeft, byte[] audioDataRight, string outputFilePath)
    {
        var ramdomFileName = Path.GetRandomFileName();
        var ramdomFileNameWithPath = Path.Combine(@"C:\temp\4\", ramdomFileName);
        string fileNameLeft = ramdomFileNameWithPath + "_left";
        string fileNameRight = ramdomFileNameWithPath + "_right";
        string rightArgument = "";

        await File.WriteAllBytesAsync(fileNameLeft, audioDataLeft);
        var ffmpegArgs = FFMpegArguments.FromFileInput(fileNameLeft);

        if (audioDataRight != null)
        {
            await File.WriteAllBytesAsync(fileNameRight, audioDataRight);
            ffmpegArgs.AddFileInput(fileNameRight);
            rightArgument = "-filter_complex amix=inputs=2:duration=first:dropout_transition=2";
        }

        await ffmpegArgs
            .OutputToFile(outputFilePath, true, options => options
            .ForceFormat("wav")
            .WithCustomArgument("-codec:a pcm_s16le -b:a 128k -ar 16000 -ac 1")
            .WithCustomArgument(rightArgument)
            )
            .ProcessAsynchronously();

        if (File.Exists(fileNameLeft)) File.Delete(fileNameLeft);
        if (File.Exists(fileNameRight)) File.Delete(fileNameRight);

    }

    public async Task ConvertToWavUsingDecoder(byte[] audioDataLeft, byte[] audioDataRight, string outputFilePath, string recordType)
    {
        var ramdomFileName = Path.GetRandomFileName();
        var ramdomFileNameWithPath = Path.Combine(@"C:\temp\4\", ramdomFileName);
        string fileNameLeft = ramdomFileNameWithPath + "_left";
        string fileNameRight = ramdomFileNameWithPath + "_right";
        string fileNameLeftWav = fileNameLeft + ".wav";
        string fileNameRightWav = fileNameRight + ".wav";
        string rightArgument = "";
        rightArgument = "-filter_complex amix=inputs=2:duration=first:dropout_transition=2";

        string pathToDecoder = "C:\\dotnet\\decoder\\decoder.exe";
        string pathToEncoder = "C:\\dotnet\\decoder\\suppdll";

        try
        {
            await File.WriteAllBytesAsync(fileNameLeft, audioDataLeft);

            if (audioDataRight != null)
            {
                await File.WriteAllBytesAsync(fileNameRight, audioDataRight);
            }
            else
            {
                await File.WriteAllBytesAsync(fileNameRight, audioDataLeft);
            }

            string decoderCommandParams = $" -c_dir \"{pathToEncoder}\" -c \"{recordType}\" -f \"{fileNameLeft}\" \"{fileNameLeftWav}\" -r \"{fileNameRight}\" \"{fileNameRightWav}\"";
            Console.WriteLine("decoderCommandParams: " + decoderCommandParams);

            await RunCmdCommand(pathToDecoder, decoderCommandParams);

            var ffmpegArgs = FFMpegArguments.FromFileInput(fileNameLeftWav);
            await ffmpegArgs
                .AddFileInput(fileNameRightWav)
                .OutputToFile(outputFilePath, true, options => options
                .ForceFormat("wav")
                .WithCustomArgument("-codec:a pcm_s16le -b:a 128k -ar 16000 -ac 1")
                .WithCustomArgument(rightArgument))
                .ProcessAsynchronously();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            // Ensure files are deleted
            Console.WriteLine("RunCmdCommand(pathToDecoder, pathToEncoder)");
            DeleteFiles(fileNameLeft, fileNameRight, fileNameLeftWav, fileNameRightWav);
        }
    }

    private static void DeleteFiles(params string[] fileNames)
    {
        foreach (var fileName in fileNames)
        {
            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }
        }
    }

    private async Task RunCmdCommand(string executablePath, string command)
    {
        try
        {
            using (var process = new Process())
            {
                process.StartInfo.FileName = executablePath;
                process.StartInfo.Arguments = command;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WorkingDirectory = Path.GetDirectoryName(executablePath); // Установите рабочую директорию

                process.Start();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    throw new Exception($"Decoder failed with exit code {process.ExitCode}:\n{error}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error running decoder: {ex.Message}");
            throw;
        }
    }
}
*/