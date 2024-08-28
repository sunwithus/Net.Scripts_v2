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
        using var form = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(audioFilePath);
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
        form.Add(fileContent, "audio_file", Path.GetFileName(audioFilePath));

        string requestUrl = "http://192.168.2.254:9000/detect-language?encode=true";

        var response = await _httpClient.PostAsync(requestUrl, form);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(jsonResponse);
        return new string[]
        {
            result.RootElement.GetProperty("language_code").GetString(),
            result.RootElement.GetProperty("detected_language").GetString()
        };
    }

    public async Task<string> RecognizeSpeechAsync(string audioFilePath, string languageCode)
    {
        using var form = new MultipartFormDataContent();
        using var fileStream = File.OpenRead(audioFilePath);
        var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("audio/wav");
        form.Add(fileContent, "audio_file", Path.GetFileName(audioFilePath));

        //string requestUrl = $"http://192.168.2.254:9000/asr?output=json&task=translate&word_timestamps=true";
        //string requestUrl = $"http://192.168.2.254:9000/asr?encode=true&task=transcribe&language=ru&word_timestamps=true&output=json";
        string requestUrl = $"http://192.168.2.254:9000/asr?encode=true&task=transcribe&word_timestamps=true&output=json";

        if (languageCode == "ru" || languageCode == "uk")
        {
            requestUrl = $"http://192.168.2.254:9000/asr?encode=true&output=json&task=transcribe&word_timestamps=true";
        }

        var response = await _httpClient.PostAsync(requestUrl, form);
        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        var result = JsonDocument.Parse(jsonResponse);
        return result.RootElement.GetProperty("text").GetString();
    }
}
