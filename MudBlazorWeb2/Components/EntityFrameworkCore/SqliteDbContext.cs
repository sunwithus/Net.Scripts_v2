//OracleDbContext.cs

using Microsoft.EntityFrameworkCore;
using MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.TodoList;
using MudBlazorWeb2.Components.Pages;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace MudBlazorWeb2.Components.EntityFrameworkCore
{
    // Определение контекста базы данных для работы с Entity Framework Core
    public class SqliteDbContext : DbContext
    {
        public DbSet<TodoItem> TodoItems => Set<TodoItem>();

        public SqliteDbContext() => Database.EnsureCreated();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string pathToSqlite = Path.Combine(AppContext.BaseDirectory, "todos.db");
            optionsBuilder.UseSqlite($"Data Source={pathToSqlite}");
        }

    }
}
