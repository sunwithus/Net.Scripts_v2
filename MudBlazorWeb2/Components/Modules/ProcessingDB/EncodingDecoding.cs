//EncodingDecoding.cs
using FFMpegCore.Pipes;
using FFMpegCore;
using System.Diagnostics;

namespace MudBlazorWeb2.Components.Modules.ProcessingDB
{
    public class EncodingDecoding
    {
        public static readonly List<string> _codecs = new List<string>()
        {
            "UMTS_AMR", "EVRC", "GSM"/*
            ,"WAVE_FILE", "RPE-LTP", "DAMPS",
            "PCM-128", "QCELP-8", "QCELP-13", "ADPCM", "AMBE.HR_IRIDIUM", "A-LAW", "AMBE_INMARSAT_MM", "APC.HR_INMARSAT_B", "IMBE_INMARSAT_M",
            "AME", "ACELP_TETRA", "GSM.EFR_ABIS", "GSM.HR_ABIS", "GSM.AMR_ABIS", "GSM_ABIS", "LD-CELP", "E-QCELP", "ATC", "PSI-CELP", "AMBE.GMR1", "AMBE.GMR2", "AMBE.INMARSAT_BGAN", "ADM.UAV",
            "PCMA", "PCMU", "IPCMA", "IPCMU", "L8", "IL8", "L16", "IL16", "G.723.1", "G.726-32", "G.728", "G.729", "GSM.0610", "ILBC-13", "ILBC-15", "PDC.FR", "PDC.EFR", "PDC.HR",
            "IDEN.FR", "APCO-25", "RP-CELP", "IDEN.HR"*/
        };

        public static readonly List<string> _ignoreRecordType = new List<string>()
        {
            "FAXDATA_GSM","DATA_GSM", "BINARY", "FAXDATA_CDMA", "Paging Response", "DMR"
        };

        public static readonly List<string> _tempIgnoreRecordType = new List<string>()
        {
            "UMTS.AMR-WB","EVS"
        };

        public static async Task ConvertToWavAsyncStream(byte[] audioDataLeft, byte[] audioDataRight, string outputFilePath)
        {
            using var streamLeft = new MemoryStream(audioDataLeft);
            using var streamRight = audioDataRight != null ? new MemoryStream(audioDataRight) : null;
            var ffmpegArgs = FFMpegArguments.FromPipeInput(new StreamPipeSource(streamLeft));
            string rightArgument = "";


            Console.WriteLine("ConvertToWavAsyncStream => outputFilePath: " + outputFilePath);
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
            Console.WriteLine("ConvertToWavAsyncStream success!!!");
        }

        public static async Task ConvertToWavAsyncFile(byte[] audioDataLeft, byte[] audioDataRight, string outputFilePath)
        {
            var ramdomFileName = Path.GetRandomFileName();
            var ramdomFileNameWithPath = Path.Combine(Path.GetDirectoryName(outputFilePath), ramdomFileName);
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

        public static async Task ConvertToWavUsingDecoder(byte[] audioDataLeft, byte[] audioDataRight, string outputFilePath, string recordType)
        {
            var ramdomFileName = Path.GetRandomFileName();
            string? directoryName = Path.GetDirectoryName(outputFilePath);
            directoryName += "_for_decoder";
            if(!Directory.Exists(directoryName)) Directory.CreateDirectory(directoryName);
            var ramdomFileNameWithPath = Path.Combine(directoryName, ramdomFileName);
            string fileNameLeft = ramdomFileNameWithPath + "_left"; // имя входного файла
            string fileNameRight = ramdomFileNameWithPath + "_right";
            string fileNameLeftWav = fileNameLeft + ".wav"; // имя выходного файла
            string fileNameRightWav = fileNameRight + ".wav";
            string rightArgument = "-filter_complex amix=inputs=2:duration=first:dropout_transition=2"; // для ffmpeg

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

                Console.WriteLine();
                Console.WriteLine($"Файл успешно сохранён: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"Error ConvertToWavUsingDecoder: {ex.Message}");
                Console.WriteLine();
            }
            finally
            {
                // Ensure files are deleted
                Console.WriteLine();
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

        private static async Task RunCmdCommand(string executablePath, string command)
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



}