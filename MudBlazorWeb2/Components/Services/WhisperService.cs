//WhisperService.cs

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

public class WhisperService
{
    private readonly HttpClient _httpClient;

    public WhisperService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string[]> DetectLanguageAsync(string audioFilePath)
    {
        DateTime startTime = DateTime.Now;

        using var form = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(audioFilePath);
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
        form.Add(fileContent, "audio_file", Path.GetFileName(audioFilePath));

        //string requestUrl = "http://localhost:9000/detect-language?encode=true";
        string requestUrl = "http://192.168.2.254:8999/detect-language?encode=true";

        var response = await _httpClient.PostAsync(requestUrl, form);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(jsonResponse);

        DateTime endTime = DateTime.Now;
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("WhisperService(DetectLanguageAsync) => Длительность выполнения запроса: " + (endTime - startTime).TotalSeconds.ToString() + " sec.");
        Console.ResetColor();

        return new string[]
        {
            result.RootElement.GetProperty("language_code").GetString(),
            result.RootElement.GetProperty("detected_language").GetString()
        };
    }

    public async Task<string> RecognizeSpeechAsync(string audioFilePath/*, string languageCode*/)
    {
        DateTime startTime = DateTime.Now;
        
        using var form = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(audioFilePath);
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
        form.Add(fileContent, "audio_file", Path.GetFileName(audioFilePath));

        string requestUrl = $"http://192.168.2.254:9000/asr?encode=true&task=transcribe&word_timestamps=true&output=txt";
        //string requestUrl = $"http://localhost:9000/asr?encode=true&task=transcribe&word_timestamps=true&output=txt";
        /*
        if (languageCode != "ru" && languageCode != "uk" && languageCode != "en")
        {
            requestUrl = $"http://192.168.2.254:9000/asr?encode=true&task=translate&language=ru&word_timestamps=true&output=txt";
        }
        */
        var response = await _httpClient.PostAsync(requestUrl, form);
        response.EnsureSuccessStatusCode();

        // &output=txt
        var responseText = await response.Content.ReadAsStringAsync();

        DateTime endTime = DateTime.Now;
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("WhisperService(RecognizeSpeechAsync) => Длительность выполнения запроса: " + (endTime - startTime).TotalSeconds.ToString() + " sec.");
        Console.ResetColor();
        
        

        return responseText;

        /* &output=json
        var jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(jsonResponse);
        return result.RootElement.GetProperty("text").GetString();
        */
    }
}
