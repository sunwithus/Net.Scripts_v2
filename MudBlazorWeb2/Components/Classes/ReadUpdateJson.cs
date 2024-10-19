//===========================================================
// 
//===========================================================
/*
namespace MudBlazorWeb2.Components.Classes
{
    public class ReadUpdateJson
    {

        public Settings ReadSettings(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("File not found", filePath);
            }

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Settings>(json);
        }

        public void UpdateSettings(string filePath, Settings settings)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(filePath, json);
        }

    }
}*/