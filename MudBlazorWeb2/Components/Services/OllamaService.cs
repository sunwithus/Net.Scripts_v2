using Microsoft.AspNetCore.Authentication;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MudBlazorWeb2.Components.Methods;
using Microsoft.Extensions.Configuration;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb;
using System.Configuration;

public class OllamaService
{
    private readonly HttpClient _httpClient;

    public OllamaService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public class ResponseData
    {
        public string response { get; set; }
        public long total_duration { get; set; }
    }

    public async Task<(string, int)> OllamaResponse(string preText, string recognizedText, string modelName, IConfiguration Configuration)
    {
        if (string.IsNullOrEmpty(recognizedText)) return ("Аудио не транскрибировано", -1);
        try
        {
            (string text, int durationOllama) = await SendTextForAnalysisAsync(preText, recognizedText, modelName, Configuration);
            ConsoleCol.WriteLine("\nOllamaResponse => Длительность выполнения: " + durationOllama + " sec.", ConsoleColor.DarkBlue);

            text = await Text.DeleteUnnecessary(text);
            return (text, durationOllama);
        }
        catch (Exception ex)
        {
            Console.WriteLine("ошибка в методе OllamaResponse: " + ex.Message);
            return ("ошибка в методе OllamaResponse: " + ex.Message, -1);
        }
    }

    public async Task<(string, int)> OllamaTranslate(string recognizedText, string modelName, string languageCode, string detectedLanguage, IConfiguration Configuration, SettingsService SettingsService)
    {
        string preTextToTranslate = SettingsService.GetSettings().PreTextTranslate;
        (string translatedText, int durationOllama) = await SendTextForAnalysisAsync(preTextToTranslate, recognizedText, modelName, Configuration);
        Console.WriteLine();
        ConsoleCol.WriteLine("OllamaTranslate => Длительность выполнения: " + durationOllama + " sec.", ConsoleColor.Blue);
        translatedText = await Text.DeleteUnnecessary(translatedText);
        translatedText = $"Перевод с {detectedLanguage.ToUpper()} языка: \n" + translatedText;

        return (translatedText, durationOllama);
    }

    public async Task<(string, int)> SendTextForAnalysisAsync(string preText, string recognizedText, string modelName, IConfiguration configuration)
    {
        var ollamaOptions = new
        {
            temperature = configuration.GetSection("OllamaModelOptions").GetValue<double>("temperature"),
            num_predict = configuration.GetSection("OllamaModelOptions").GetValue<int>("num_predict"),
            num_ctx = configuration.GetSection("OllamaModelOptions").GetValue<int>("num_ctx"),
            top_k = configuration.GetSection("OllamaModelOptions").GetValue<int>("top_k"),
            top_p = configuration.GetSection("OllamaModelOptions").GetValue<double>("top_p"),
            repeat_penalty = configuration.GetSection("OllamaModelOptions").GetValue<double>("repeat_penalty"),
            presence_penalty = configuration.GetSection("OllamaModelOptions").GetValue<double>("presence_penalty"),
            frequency_penalty = configuration.GetSection("OllamaModelOptions").GetValue<double>("frequency_penalty"),
        };
        string ollamaIP = configuration["OllamaIP"];

        // Check for negative values
        if (ollamaOptions.num_predict < 0 || ollamaOptions.num_ctx < 0 || ollamaOptions.top_k < 0)
        {
            throw new ArgumentException("One or more parameters have negative values.");
        }

        var content = new StringContent(JsonSerializer.Serialize(new { 
            model = modelName, 
            prompt = $"{preText} => {recognizedText}. Ответ должен быть только на русском языке.",
            stream = false,
            options = new
            {
                temperature = ollamaOptions.temperature,
                num_predict = ollamaOptions.num_predict,
                num_ctx = ollamaOptions.num_ctx,
                top_k = ollamaOptions.top_k,
                top_p = ollamaOptions.top_p,
                repeat_penalty = ollamaOptions.repeat_penalty,
                presence_penalty = ollamaOptions.presence_penalty,
                frequency_penalty = ollamaOptions.frequency_penalty,
            }

    }), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync($"{ollamaIP}/api/generate", content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var responseWithBody = JsonSerializer.Deserialize<ResponseData>(responseBody);
        
        return (responseWithBody?.response ?? "response is empty or null", (int)(responseWithBody?.total_duration / 1000 / 1000 / 1000));
    }
}

/*

            options = new
            {
                // Температура (temperature) контролирует степень случайности генерируемого текста.
                // Значение 0.2 означает, что модель будет генерировать более предсказуемые и менее случайные ответы.
                temperature = 0.3,

                // Количество токенов, которые необходимо предсказать (num_predict).
                // Значение 2000 указывает, что модель будет генерировать текст из 2000 токенов.
                num_predict = 2000,

                // Длина контекста, которую модель будет учитывать (num_ctx).
                // Значение 4096 указывает, что модель будет использовать контекст длиной в 4096 токенов.
                num_ctx = 4096,

                // Количество наиболее вероятных токенов, которые будут рассматриваться при генерации текста (top_k).
                // Значение 40 указывает, что модель будет учитывать 40 наиболее вероятных токенов.
                top_k = 40,

                // Пороговое значение для вероятности токенов (top_p).
                // Значение 0.95 указывает, что токены с вероятностью ниже 95% будут игнорироваться.
                top_p = 0.95,

                // Штраф за повторение токенов (repeat_penalty).
                // Значение 1.5 указывает, что повторение токенов будет штрафоваться более сильно, что помогает избежать повторений в тексте.
                repeat_penalty = 1.5,

                // Штраф за присутствие определенных токенов (presence_penalty).
                // Значение 1.2 указывает, что присутствие некоторых токенов будет штрафоваться, что помогает избежать их избыточного использования.
                presence_penalty = 1.2,

                // Штраф за частоту использования токенов (frequency_penalty).
                // Значение 1.1 указывает, что частое использование одних и тех же токенов будет штрафоваться, что помогает поддерживать разнообразие текста.
                frequency_penalty = 1,
            }
 */

/*
Temperature
Низкие значения temperature (например, 0.6) делают ответы более детерминированными и предсказуемыми. Это может ускорить процесс генерации текста, поскольку модель менее часто будет экспериментировать с разными вариантами ответов. Высокие значения температуры могут привести к более творческим, но также более ресурсоемким и медленным ответам.
Num Thread
Параметр num_thread определяет количество потоков, используемых для генерации текста. Увеличение этого значения (например, до 16) может существенно ускорить процесс генерации текста, поскольку задача распределяется между несколькими потоками, что позволяет использовать параллельную обработку и снижает общее время выполнения запроса.
Num Predict
Параметр num_predict определяет количество токенов, которые модель будет генерировать в одном ответе. Установка этого значения в 200 означает, что модель остановится после генерации 200 токенов, даже если это происходит посередине предложения. Более низкие значения num_predict могут ускорить выполнение запроса, поскольку модель будет генерировать меньше текста, но это также может ограничить детализацию ответа.
Num Ctx
Параметр num_ctx определяет размер контекстного окна. Более крупное контекстное окно (например, 4096) позволяет модели обрабатывать длинные тексты, но это также может увеличить время выполнения запроса, поскольку модель будет обрабатывать больше информации. Слишком малое контекстное окно может ускорить выполнение, но это может ухудшить качество ответов, поскольку модель будет иметь меньше контекста для принятия решений.
*/

/*
answer:
{"model":"gemma2",
"created_at":"2024-11-02T13:46:43.712067888Z",
"response":"успех \n",
"done":true,
"done_reason":"stop",
"context":[106,1645,108,38780,2607,2241,1416,8168,40939,76897,235292,1236,6550,107,108,106,2516,108,235355,15268,235401,235248,108],
"total_duration":346978478654,
"load_duration":50799110582,
"prompt_eval_count":19,
"prompt_eval_duration":42270653000,
"eval_count":6,
"eval_duration":211462844000}


 * 
Параметры
model: (обязательно) название модели
prompt: запрос на создание ответа для
suffix: текст после ответа модели
images: (необязательно) список изображений в кодировке base64 (для мультимодальных моделей, таких как llava)
Расширенные параметры (опционально):

format: формат для возврата ответа. В настоящее время единственным допустимым значением является json
options: дополнительные параметры модели, перечисленные в документации к файлу модели, такие как temperature
system: системное сообщение в (переопределяет то, что определено в Modelfile)
template: шаблон приглашения для использования (переопределяет то, что определено в Modelfile)
context: параметр контекста, возвращенный из предыдущего запроса к , это может быть использовано для сохранения короткой памяти разговора/generate
stream: если ответ будет возвращен в виде одного объекта ответа, а не потока объектовfalse
raw: если форматирование не будет применено к приглашению. Вы можете использовать этот параметр, если указываете полный шаблон запроса в своем запросе к APItrueraw
keep_alive: определяет, как долго модель будет оставаться загруженной в память после запроса (по умолчанию: 5m)


Итоговый ответ в потоке также включает в себя дополнительные данные о поколении:

total_duration: время, затраченное на генерацию ответа
load_duration: время, затраченное на загрузку модели в наносекундах
prompt_eval_count: количество токенов в командной строке
prompt_eval_duration: время, затраченное в наносекундах на оценку запроса
eval_count: количество токенов в ответе
eval_duration: время в наносекундах, затраченное на генерацию ответа
context: кодировка разговора, используемая в этом ответе, может быть отправлена в следующем запросе для сохранения памяти разговора
response: пусто, если ответ был передан в потоковом режиме, если не был передан в потоковом режиме, то будет содержать полный ответ

 
Сгенерировать запрос (с опциями)
Если вы хотите задать пользовательские параметры для модели во время выполнения, а не в файле модели, вы можете сделать это с помощью параметра. В этом примере задаются все доступные параметры, но вы можете задать любой из них по отдельности и опустить те, которые не хотите переопределять.options

Просьба
curl http://localhost:11434/api/generate -d '{
  "model": "llama3.2",
  "prompt": "Why is the sky blue?",
  "stream": false,
  "options": {
    "num_keep": 5, //Количество ответов, которые необходимо сохранить. В данном случае сохраняются 5 наиболее вероятных ответов.
    "seed": 42, //Случайное число, которое используется для обеспечения воспроизводимости результатов генерации. Это помогает в повторении экспериментов и сравнении результатов.
    "num_predict": 100, //Количество токенов, которые необходимо предсказать. В данном случае модель предсказывает 100 токенов.
    "top_k": 20, //Количество наиболее вероятных токенов, которые будут рассматриваться при генерации текста. Это помогает в уменьшении количества вариантов и улучшении качества ответа.
    "top_p": 0.9, //Пороговое значение для вероятности токенов. Токены с вероятностью ниже этого значения будут игнорироваться. В данном случае 90% вероятность токенов будет учитываться.
    "min_p": 0.0, //Минимальная вероятность токена, которая будет учитываться. В данном случае минимальная вероятность равна 0%.
    "tfs_z": 0.5, //Параметр, который регулирует величину токен-фрейм-сенсоризации (TF-Sens). Этот параметр используется для управления чувствительностью модели к токенам.
    "typical_p": 0.7, //Пороговое значение для типичной вероятности токенов. Токены с вероятностью ниже этого значения будут игнорироваться. В данном случае 70% вероятность токенов будет учитываться.
    "repeat_last_n": 33, //Количество последних токенов, которые будут повторены при генерации текста. В данном случае последние 33 токена будут повторены.
    
    "temperature": 0.8, //Параметр, который регулирует степень случайности генерации текста. Более низкие значения температуры приводят к более предсказуемому поведению модели, в то время как более высокие значения увеличивают степень случайности.
    "repeat_penalty": 1.2, //
    "presence_penalty": 1.5, //
    "frequency_penalty": 1.0, //
    "mirostat": 1, //
    "mirostat_tau": 0.8, //
    "mirostat_eta": 0.6, //
    "penalize_newline": true, //
    "stop": ["\n", "user:"], //
    "numa": false, //
    "num_ctx": 1024, //
    "num_batch": 2, //
    "num_gpu": 1, //Количество GPU, которые будут использованы для генерации текста. В данном случае будет использоваться одна GPU.
    "main_gpu": 0, //Индекс основной GPU, которая будет использоваться для генерации текста. В данном случае основной GPU будет номер 0.
    "low_vram": false, //Флаг, указывающий, следует ли использовать малую видеопамять. В данном случае малая видеопамять не будет использоваться.
    "f16_kv": true, //Флаг, указывающий, следует ли использовать 16-битные ключевые значения. В данном случае 16-битные ключевые значения будут использованы.
    "vocab_only": false, //
    "use_mmap": true, //Флаг, указывающий, следует ли использовать маппинг памяти. В данном случае маппинг памяти будет использован.
    "use_mlock": false, //Флаг, указывающий, следует ли использовать блокировку памяти. В данном случае блокировка памяти не будет использована.
    "num_thread": 8 //Количество потоков, которые будут использованы для генерации текста. В данном случае потоков будет 8.
  }
}'
 */