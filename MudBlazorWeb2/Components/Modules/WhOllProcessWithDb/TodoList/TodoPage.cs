//TodoPage.cs

using MudBlazorWeb2.Components.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using MudBlazorWeb2.Components.EntityFrameworkCore.SqliteModel;

namespace MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.TodoList
{
    public class DatabaseConnection
    { 
        public static async Task<(bool, string)> Test(TodoItem todo)
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

    public class TodoListBase : ComponentBase
    {
        [Inject]
        public SqliteDbContext Sqlite { get; set; }

        public async Task<List<TodoItem>> LoadTodos()
        {
            return await Sqlite.TodoItems.ToListAsync();
        }

        public async Task UpdateTodo(TodoItem todo)
        {
            Sqlite.TodoItems.Update(todo);
            await Sqlite.SaveChangesAsync();
        }

        public async Task DeleteTodo(TodoItem todo)
        {
            Sqlite.TodoItems.Remove(todo);
            await Sqlite.SaveChangesAsync();
        }
        public async Task AddTodo(TodoItem todo)
        {
            await Sqlite.TodoItems.AddAsync(todo);
            await Sqlite.SaveChangesAsync();
        }

    }
}
