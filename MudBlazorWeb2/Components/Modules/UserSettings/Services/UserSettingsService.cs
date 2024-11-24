//OperSettingsService.cs

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MudBlazorWeb2.Components.Modules.SettingsOper.Services
{
    public class UserSettingsService
    {
        private readonly string _settingsFilePath;

        public UserSettingsService()
        {
            _settingsFilePath = Path.Combine(AppContext.BaseDirectory, "settingsOper.json");
        }

        public async Task SaveItemAsync(string key, string value)
        {
            var settings = await ReadAllItemsFromFile();
            settings[key] = value;
            await WriteAllItemsToFile(settings);
        }
        public async Task<string> ReadItemValueByKey(string key)
        {
            var settings = await ReadAllItemsFromFile();
            return settings.TryGetValue(key, out string value) ? value : null;
        }
        public async Task DeleteItemByKey(string key)
        {
            var settings = await ReadAllItemsFromFile();
            if (settings.ContainsKey(key))
            {
                settings.Remove(key);
                await WriteAllItemsToFile(settings);
            }
        }

        public async Task<Dictionary<string, string>> ReadAllItemsFromFile()
        {
            if (!File.Exists(_settingsFilePath))
                return new Dictionary<string, string>();

            var json = await File.ReadAllTextAsync(_settingsFilePath);
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }

        private async Task WriteAllItemsToFile(Dictionary<string, string> settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_settingsFilePath, json);
        }
    }
}