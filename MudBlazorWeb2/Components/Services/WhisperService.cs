//WhisperService.cs

using System.Configuration;
using System.Net.Http.Headers;
using System.Text.Json;
using MudBlazorWeb2.Components.Methods;

public class WhisperService
{
    private readonly HttpClient _httpClient;

    public WhisperService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(string, string)> DetectLanguageAsync(string audioFilePath, IConfiguration conf)
    {
        try
        {
            var jsonResponse = await SendAudioRequestAsync(audioFilePath, conf["WhisperDetectLanguageIP"]);

            var result = JsonDocument.Parse(jsonResponse);
            return (result.RootElement.GetProperty("language_code").GetString().ToLower(),
                    result.RootElement.GetProperty("detected_language").GetString());
        }
        catch (Exception ex)
        {
            ConsoleCol.WriteLine("Ошибка в методе WhisperDetectLanguage: " + ex.Message, ConsoleColor.Red);
            throw;
        }
    }

    public async Task<string> RecognizeSpeechAsync(string audioFilePath, IConfiguration conf)
    {
        try
        {
            var responseText = await SendAudioRequestAsync(audioFilePath, conf["WhisperRecogniseIP"]);
            return responseText;
        }
        catch (Exception ex)
        {
            ConsoleCol.WriteLine("Ошибка в методе RecognizeSpeechAsync: " + ex.Message, ConsoleColor.Red);
            throw;
        }
    }

    private async Task<string> SendAudioRequestAsync(string audioFilePath, string requestUrl, string contentType = "audio/wav")
    {
        DateTime startTime = DateTime.Now;

        using var form = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(audioFilePath);
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        form.Add(fileContent, "audio_file", Path.GetFileName(audioFilePath));

        var response = await _httpClient.PostAsync(requestUrl, form);
        response.EnsureSuccessStatusCode();

        var responseText = await response.Content.ReadAsStringAsync();

        DateTime endTime = DateTime.Now;
        ConsoleCol.WriteLine($"\n########## {requestUrl} \nВремя выполнения Whisper = {((int)Math.Round((endTime - startTime).TotalSeconds))} sec.", ConsoleColor.DarkYellow);

        return responseText;
    }
}
