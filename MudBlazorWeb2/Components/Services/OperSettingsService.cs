using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

public class OperSettingsService
{
    private readonly string _settingsFilePath;

    public OperSettingsService()
    {
        _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "settingsOper.json");
    }

    public async Task SaveSettingAsync(string key, string value)
    {
        var settings = ReadSettingsFromFile();
        settings[key] = value;
        await WriteSettingsToFile(settings);
    }

    public string GetSetting(string key)
    {
        var settings = ReadSettingsFromFile();
        return settings.TryGetValue(key, out string value) ? value : null;
    }

    private Dictionary<string, string> ReadSettingsFromFile()
    {
        if (!File.Exists(_settingsFilePath))
            return new Dictionary<string, string>();

        var json = File.ReadAllText(_settingsFilePath);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
    }

    private async Task WriteSettingsToFile(Dictionary<string, string> settings)
    {
        var json = JsonSerializer.Serialize(settings);
        await File.WriteAllTextAsync(_settingsFilePath, json);
    }
}

/* usage

        var operSettingsService = new OperSettingsService();
        await operSettingsService.SaveSettingAsync("MyKey", "MyValue");
        var value = operSettingsService.GetSetting("MyKey");
        Console.WriteLine($"Settings loaded MyKey => {value}.");
*/