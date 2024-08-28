using System;
using System.IO;
using System.Text.Json;

public class OracleSettings
{
    public string User { get; set; }
    public string Password { get; set; }
    public string DataSource { get; set; }
    public string Scheme { get; set; }

    public void LoadSettingsFromJson(string filePath)
    {
        if (File.Exists(filePath))
        {
            var json = File.ReadAllText(filePath);
            var settings = JsonSerializer.Deserialize<OracleSettings>(json);
            if (settings != null)
            {
                User = settings.User;
                Password = settings.Password;
                DataSource = settings.DataSource;
                Scheme = settings.Scheme;
            }
        }
        else
        {
            throw new FileNotFoundException("Файл настроек не найден.", filePath);
        }
    }

    public void SaveSettingsToJson(string filePath)
    {
        try
        {
            var json = JsonSerializer.Serialize(this);
            File.WriteAllText(filePath, json);
            Console.WriteLine("Настройки успешно сохранены.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при сохранении настроек: {ex.Message}");
        }
    }
}