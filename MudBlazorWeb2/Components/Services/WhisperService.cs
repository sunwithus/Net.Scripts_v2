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
            string whisperIP = conf["WhisperDetectLanguageIP"];

            DateTime startTime = DateTime.Now;
            using var form = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(audioFilePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
            form.Add(fileContent, "audio_file", Path.GetFileName(audioFilePath));

            string requestUrl = $"{whisperIP}/detect-language?encode=true";

            var response = await _httpClient.PostAsync(requestUrl, form);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var result = JsonDocument.Parse(jsonResponse);

            DateTime endTime = DateTime.Now;
            ConsoleCol.WriteLine("\nWhisper (DetectLanguageAsync) => request time: " + ((int)Math.Round((endTime - startTime).TotalSeconds)).ToString() + " sec.", ConsoleColor.DarkGreen);

            return (result.RootElement.GetProperty("language_code").GetString().ToLower(),
                result.RootElement.GetProperty("detected_language").GetString());
        }
        catch (Exception ex)
        {
            ConsoleCol.WriteLine("ошибка в методе WhisperDetectLanguage: " + ex.Message, ConsoleColor.Red);
            return ("", "");
        }
    }

    public async Task<string> RecognizeSpeechAsync(string audioFilePath, IConfiguration conf)
    {
        try
        {
            string whisperIP = conf["WhisperRecogniseIP"];

            DateTime startTime = DateTime.Now;
        
            using var form = new MultipartFormDataContent();
            using var fileStream = File.OpenRead(audioFilePath);
            var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
            form.Add(fileContent, "audio_file", Path.GetFileName(audioFilePath));

            string requestUrl = $"{whisperIP}/asr?encode=true&task=transcribe&word_timestamps=true&output=txt";

            var response = await _httpClient.PostAsync(requestUrl, form);
            response.EnsureSuccessStatusCode();

            var responseText = await response.Content.ReadAsStringAsync();

            DateTime endTime = DateTime.Now;
            ConsoleCol.WriteLine("\nWhisper (RecognizeSpeechAsync) => request time: " + ((int)Math.Round((endTime - startTime).TotalSeconds)).ToString() + " sec.", ConsoleColor.DarkGray);

            return responseText;
        }
        catch (Exception ex) {
            ConsoleCol.WriteLine("ошибка в методе RecognizeSpeechAsync: " + ex.Message, ConsoleColor.Red);
            return ("");
        }
    }
}
