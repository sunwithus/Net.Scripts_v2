// SettingsService.cs

using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using System.Threading.Tasks;

public class Settings
{
    public string PreText { get; set; }
    public string PreTextDefault { get; set; }
    public SettingsReplicator SettingsReplicator { get; set; }
    public SettingsProcessing SettingsProcessing { get; set; }
    public SettingsMakingWord SettingsMakingWord { get; set; }
}

public class SettingsReplicator
{
    public OraItems OraItems { get; set; }
}

public class SettingsProcessing
{
    public OraItems OraItems { get; set; }
}

public class SettingsMakingWord
{
    public OraItems OraItems { get; set; }
}

public class OraItems
{
    public string User { get; set; }
    public string Password { get; set; }
    public string DataSource { get; set; }
    public string Scheme { get; set; }
}

/// <summary>
/// ///////////////////////// SettingsService
/// </summary>
public class SettingsService
{
    private readonly string _settingsFilePath;

    public SettingsService()
    {
        _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "settingsApp.json");
    }

    public async Task SaveSettingAsync(Settings settings)
    {
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_settingsFilePath, json);
    }

    public Settings GetSettings()
    {
    if (!File.Exists(_settingsFilePath))
    {
        var defaultSettings = new Settings
        {
            SettingsReplicator = new SettingsReplicator
            {
                OraItems = new OraItems()
            },
            SettingsProcessing = new SettingsProcessing
            {
                OraItems = new OraItems()
            }
        };
        return defaultSettings;
    }

        var json = File.ReadAllText(_settingsFilePath);
        return JsonSerializer.Deserialize<Settings>(json);
    }

    public async Task UpdateSettingAsync(string key, object value)
    {
        var settings = GetSettings();

        switch (key)
        {
            case "PreText":
                settings.PreText = (string)value;
                break;
            case "PreTextDefault":
                settings.PreTextDefault = (string)value;
                break;

            // SettingsReplicator
            case "SettingsReplicator.OraItems.User":
                settings.SettingsReplicator.OraItems.User = (string)value;
                break;
            case "SettingsReplicator.OraItems.Password":
                settings.SettingsReplicator.OraItems.Password = (string)value;
                break;
            case "SettingsReplicator.OraItems.DataSource":
                settings.SettingsReplicator.OraItems.DataSource = (string)value;
                break;
            case "SettingsReplicator.OraItems.Scheme":
                settings.SettingsReplicator.OraItems.Scheme = (string)value;
                break;

            // SettingsProcessing
            case "SettingsProcessing.OraItems.User":
                settings.SettingsProcessing.OraItems.User = (string)value;
                break;
            case "SettingsProcessing.OraItems.Password":
                settings.SettingsProcessing.OraItems.Password = (string)value;
                break;
            case "SettingsProcessing.OraItems.DataSource":
                settings.SettingsProcessing.OraItems.DataSource = (string)value;
                break;
            case "SettingsProcessing.OraItems.Scheme":
                settings.SettingsProcessing.OraItems.Scheme = (string)value;
                break;

            // SettingsMakingWord
            case "SettingsMakingWord.OraItems.User":
                settings.SettingsMakingWord.OraItems.User = (string)value;
                break;
            case "SettingsMakingWord.OraItems.Password":
                settings.SettingsMakingWord.OraItems.Password = (string)value;
                break;
            case "SettingsMakingWord.OraItems.DataSource":
                settings.SettingsMakingWord.OraItems.DataSource = (string)value;
                break;
            case "SettingsMakingWord.OraItems.Scheme":
                settings.SettingsMakingWord.OraItems.Scheme = (string)value;
                break;
  
            // Exeption during Update
            default:
                throw new ArgumentException("Invalid key", nameof(key));
        }

        await SaveSettingAsync(settings);
    }
}
