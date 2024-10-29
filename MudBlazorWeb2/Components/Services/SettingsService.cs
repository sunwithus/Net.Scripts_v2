// SettingsService.cs
using Newtonsoft.Json;

public class SettingsService
{
    private readonly string _settingsFilePath;

    public SettingsService()
    {
        _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "settingsApp.json");
    }

    public async Task SaveSettingAsync(string key, string value)
    {
        var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(_settingsFilePath));
        settings[key] = value;
        await File.WriteAllTextAsync(_settingsFilePath, JsonConvert.SerializeObject(settings));
        //Console.WriteLine($"Settings saved {key} => {value}.");
    }

    public string GetSetting(string key)
    {
        var settings = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(_settingsFilePath));
        //Console.WriteLine($"Settings loaded {key} => {settings[key]}.");
        return settings[key];
    }
}