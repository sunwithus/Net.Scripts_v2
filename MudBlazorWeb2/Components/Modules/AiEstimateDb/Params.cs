//Params.cs

using MudBlazorWeb2.Components.Modules.UserSettings;

namespace MudBlazorWeb2.Components.Modules.AiEstimateDb
{
    public static class Params
    {
        private static string FilePretextDefault = Path.Combine(AppContext.BaseDirectory, "pretextDefault.ini");

        public static async Task<string> GetPreTextAsync(string operatorName)
        {
            ConsoleCol.WriteLine("operatorName: " + operatorName, ConsoleColor.DarkYellow);
            string? preText = "";

            try
            {
                preText = await SourceName.ReadItemValueByKey(operatorName);
                if (string.IsNullOrEmpty(preText))
                {
                    preText = await IniFile.ReadFile(FilePretextDefault);
                    ConsoleCol.WriteLine("preText = PretextDefault", ConsoleColor.DarkYellow);
                }

            }
            catch (Exception ex)
            {
                ConsoleCol.WriteLine("Error getting Operator: " + ex, ConsoleColor.Red);
                preText = await IniFile.ReadFile(FilePretextDefault);
            }
            finally
            {
                //ConsoleCol.WriteLine("preText: " + preText, ConsoleColor.DarkYellow);
            }
            return preText;
        }

    }
}


