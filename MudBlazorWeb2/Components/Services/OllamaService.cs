using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class OllamaService
{
    private readonly HttpClient _httpClient;

    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromMinutes(7);
    }

    public class ResponseData
    {
        public string response { get; set; } = string.Empty;
    }
    public async Task<string> SendTextForAnalysisAsync(string preText, string recognizedText, string modelName)
    {
        var content = new StringContent(JsonSerializer.Serialize(new { model = modelName, prompt = preText + "=>" + recognizedText, stream = false }), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("http://192.168.2.253:11434/api/generate", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine("responseBody:");
        Console.WriteLine(responseBody);

        var responseWithotBody = JsonSerializer.Deserialize<ResponseData>(responseBody);
        return responseWithotBody?.response ?? "response is empty or null";

    }
}
