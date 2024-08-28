using MudBlazorWeb2.Components.Common;
using System.Text.Json;

//===========================================================
// 
//===========================================================

namespace MudBlazorWeb2.Components.Data
{
    public class ReadUpdateJson
    {

        private readonly string _stateDataFilePath;

        public ReadUpdateJson()
        {
            _stateDataFilePath = Constants.StateData;//Path.Combine(AppContext.BaseDirectory, "statedata.json");
        }
        //====================================
        public async Task<Parameters> ReadFromJsonAsync()
        {
            if (File.Exists(_stateDataFilePath))
            {
                var json = await File.ReadAllTextAsync(_stateDataFilePath);
                return JsonSerializer.Deserialize<Parameters>(json) ?? new Parameters();
            }
            return new Parameters(); // Возвращаем новый объект, если файл не существует
        }
        //====================================
        public async Task UpdateStateAsync(Parameters parameters)
        {
            var json = JsonSerializer.Serialize(parameters);
            try 
            { 
                await File.WriteAllTextAsync(_stateDataFilePath, json);  
            } 
            catch (Exception e)
            { 
                Console.WriteLine("UpdateStateAsync: " + e);
            } 
        }

    }
}