﻿// TodoItem.cs

namespace MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.TodoList
{
    public class TodoItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? Title { get; set; }
        public bool IsDone { get; set; } = false;
        public bool IsRunning { get; set; } = false;
        public bool IsRunPressed { get; set; } = false;
        public bool IsStopPressed { get; set; } = false;
        public int CompletedKeys { get; set; } = 0;
        public int TotalKeys { get; set; } = 0;
        public string? ProcessingMessage { get; set; } = "";
        public bool IsCyclic { get; set; } = true;
        public int CycleInterval { get; set; } = 1;
        public DateTime StartDateTime { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime EndDateTime { get; set; } = DateTime.Now.AddMonths(1).AddYears(1);
        public string? DurationString {get; set; } = "00:00:10";

        public string? DbType { get; set; } = "PostgreSql"; //Oracle //InterBase //Sqlite
        public string? User { get; set; } = "SYSDBA";
        public string? Password { get; set; } = "masterkey";
        public string? ServerAddress { get; set; } = "127.0.0.1 / sprutora";
        public string? Scheme { get; set; } = "";

    }
}