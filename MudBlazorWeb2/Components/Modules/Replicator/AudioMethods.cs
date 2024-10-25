//OraSettings.cs
using FFMpegCore.Pipes;
using FFMpegCore;
using System.Diagnostics;

namespace MudBlazorWeb2.Components.Modules.Replicator
{
    public class AudioMethods
    {
        // Caller ; Talker
        public static (DateTime Timestamp, string IMEI, string Caller, string Talker, int Calltype) ParseFileName(string filePath)
        {
            var fileExt = Path.GetExtension(filePath);
            var fileNameNoExt = Path.GetFileNameWithoutExtension(filePath);
            var parts = fileNameNoExt.Split('_');
            try
            {
                //01012016_000759_35000000000000_79046283999_79046283999.wav
                if (parts.Length == 5)
                {
                    string timestampString = parts[0].Insert(2, "-").Insert(5, "-") + " " + parts[1].Insert(2, ":").Insert(5, ":");
                    DateTime timestamp = DateTime.ParseExact(timestampString, "dd-MM-yyyy HH:mm:ss", null);

                    return (timestamp, parts[2], parts[3], parts[4], 2); // Calltype = 2 - неизвестно, 0 - входящий, 1 - исходящий
                }
                //79841944120_79242505061_Call_In_2023-11-23_16_15_36.mp3
                //_89841537912_Call_Out_2024-07-16_14_53_35.mp3
                else if (parts.Length == 8)
                {
                    string timestampString = parts[4] + " " + parts[5].Substring(0, 2) + ":" + parts[6].Substring(0, 2) + ":" + parts[7].Substring(0, 2);
                    DateTime timestamp = DateTime.ParseExact(timestampString, "yyyy-MM-dd HH:mm:ss", null);
                    int calltype = (parts[3] == "In") ? 0 : (parts[3] == "Out") ? 1 : 2; //тип вызова 0-входящий, 1-исходящий, 2-неизвестный...
                    return (timestamp, "", parts[0], parts[1], calltype); //parts[0] - Caller; 
                }
                else
                {
                    throw new InvalidOperationException("Invalid file name format.");
                }
            }
            catch
            {
                Console.WriteLine("Не удалось получить данные из названия файла: " + fileNameNoExt);
                return (DateTime.Now, "", "", "", 2);
            }
            throw new InvalidOperationException("Unsupported file extension: " + fileExt);
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

        private async Task RunFFmpeg(string inputFileName)
        {
            var mediaInfo = await FFProbe.AnalyseAsync(inputFileName);
            //durationOfWav = Convert.ToInt32(mediaInfo.PrimaryAudioStream?.Duration.TotalSeconds);
            //Console.WriteLine("durationOfWav:" + durationOfWav);

            string ffmpegCommandParams;

            if (mediaInfo.PrimaryAudioStream?.Channels >= 2)
            {
                ffmpegCommandParams = $"-i {inputFileName} -filter_complex \"[0:0]pan=1|c0=c0[left];[0:0]pan=1|c0=c1[right]\" ";
                ffmpegCommandParams += $"-map \"[left]\" -c:a pcm_alaw -b:a 128k -ar 8000 {inputFileName}_left.wav ";
                ffmpegCommandParams += $"-map \"[right]\" -c:a pcm_alaw -b:a 128k -ar 8000 {inputFileName}_right.wav ";
            }
            else
            {
                ffmpegCommandParams = $"-i {inputFileName} -codec:a pcm_alaw -b:a 128k -ac 1 -ar 8000 {inputFileName}_mono.wav";
            }
            //await RunCmdCommand(ffmpegExePath, ffmpegCommandParams);

        }

        private async Task RunCmdCommand(string executablePath, string command)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    /*if (!string.IsNullOrEmpty(e.Data))
                    {
                        Console.WriteLine(e.Data);
                        InvokeAsync(() =>
                        {
                            string beforeTime = "->";
                            int indexOfTimePosition = e.Data.IndexOf(beforeTime);
                            if (indexOfTimePosition != -1)
                            {
                                string time = e.Data.Substring(indexOfTimePosition + beforeTime.Length, 8);
                                TimeSpan timeSpan = TimeSpan.Parse(time);
                                currentProgress = (int)(100 * timeSpan.TotalSeconds / durationOfWav);
                                currentProgress = currentProgress > 100 ? 99 : currentProgress;
                                Console.WriteLine($"Прогресс: {currentProgress}");
                            }
                            StateHasChanged();
                        });
                    }*/
                };

                process.Start();

                using (StreamWriter sw = process.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                    {
                        sw.WriteLine($"c: cd");
                        sw.WriteLine($"cd {Path.GetDirectoryName(executablePath)}");
                        sw.WriteLine($"{Path.GetFileName(executablePath)} {command}");
                        sw.WriteLine("exit");
                    }
                }
                process.BeginOutputReadLine();
                await Task.Run(() => process.WaitForExit());
            }
        }
    }

}