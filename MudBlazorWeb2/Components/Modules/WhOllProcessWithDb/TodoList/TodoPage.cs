using MudBlazorWeb2.Components.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.TodoList
{
    public class TodoPage 
    { 
        public static async Task<(bool, string)> TestDatabaseConnection(TodoItem todo)
        {
            try
            {
                long? maxKey = null;
                string conStringDBA = $"User Id={todo.User};Password={todo.Password};Data Source={todo.ServerAddress};";
                var optionsBuilder = OracleDbContext.ConfigureOptionsBuilder(conStringDBA);
                using (var context = new OracleDbContext(optionsBuilder.Options))
                {
                    await context.Database.OpenConnectionAsync();
                    if (await context.Database.CanConnectAsync())
                    {
                        await context.Database.ExecuteSqlRawAsync($"ALTER SESSION SET CURRENT_SCHEMA = {todo.Scheme}");
                        maxKey = await context.SprSpeechTable.MaxAsync(x => x.Id);
                        ConsoleCol.WriteLine($"maxKey = {maxKey}", ConsoleColor.Green);
                        await context.Database.CloseConnectionAsync();
                    }
                }
                return (true, $"maxKey = {maxKey}");
            }
            catch (Exception ex)
            {
                ConsoleCol.WriteLine($"Connection Error: {ex.Message}", ConsoleColor.Red);
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}
