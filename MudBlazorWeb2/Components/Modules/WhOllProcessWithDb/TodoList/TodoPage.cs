using MudBlazorWeb2.Components.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;

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

    public class TodoListBase : ComponentBase
    {
        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public ISnackbar SnackbarService { get; set; }

        [Inject]
        public SqliteDbContext Sqlite { get; set; }

        public MudBlazor.Color colorTag = Color.Info;
        public List<TodoItem> todos = new();
        public string? newTodo;
        public bool IsPanelExpanded = false;

        protected override async Task OnInitializedAsync()
        {
            todos = await Sqlite.TodoItems.ToListAsync();
        }

        protected async Task StartButtonPressed(TodoItem todo)
        {
            todo.IsRunPressed = true;
            todo.IsRunning = true;
            todo.IsStopPressed = false;
            Sqlite.TodoItems.Update(todo);
            await Sqlite.SaveChangesAsync();

            ConsoleCol.WriteLine("StartButtonPressed", ConsoleColor.Blue);
        }

        protected async Task StopButtonPressed(TodoItem todo)
        {
            todo.IsStopPressed = true;
            todo.IsRunPressed = false;
            todo.IsRunning = false;
            Sqlite.TodoItems.Update(todo);
            await Sqlite.SaveChangesAsync();

            ConsoleCol.WriteLine("StopButtonPressed", ConsoleColor.Blue);
        }


        protected async Task TestConnection(TodoItem todo)
        {
            var (result, status) = await TodoPage.TestDatabaseConnection(todo);
            string relultString = result ? "Соединение установлено!" : "Соединение не установлено!";
            SnackbarService.Configuration.PositionClass = Defaults.Classes.Position.BottomRight;
            SnackbarService.Configuration.VisibleStateDuration = 1000;
            SnackbarService.Configuration.PreventDuplicates = true;
            SnackbarService.Add("<span>"+relultString+ "<br />"+status+ "</span>", key:"mudblazor");
        }

        protected async Task DialogDeleteTodoAndCollapse(TodoItem todo)
        {
            var options = new DialogOptions { CloseOnEscapeKey = true, BackgroundClass = "bg-custom-class" };

            var dialog = await DialogService.ShowAsync<Dialog>("ConfirmDeletion Dialog", options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                IsPanelExpanded = false;
                Sqlite.TodoItems.Remove(todo);
                await Sqlite.SaveChangesAsync();
                todos = await Sqlite.TodoItems.ToListAsync();
                StateHasChanged();
            }
        }

        public async Task SaveTodos(TodoItem todo)
        {
            Sqlite.TodoItems.Update(todo);
            await Sqlite.SaveChangesAsync();
            StateHasChanged();
        }

        public async Task<List<TodoItem>> LoadTodos()
        {
            return await Sqlite.TodoItems.ToListAsync();
        }

        public async Task AddTodo()
        {
            if (!string.IsNullOrWhiteSpace(newTodo))
            {
                var newItem = new TodoItem { Title = newTodo };
                await Sqlite.TodoItems.AddAsync(newItem);
                await Sqlite.SaveChangesAsync();
                todos = await Sqlite.TodoItems.ToListAsync();
                newTodo = "";
                StateHasChanged();
            }
        }

    }
}
