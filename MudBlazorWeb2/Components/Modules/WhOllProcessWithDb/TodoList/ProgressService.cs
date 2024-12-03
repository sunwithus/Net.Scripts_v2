namespace MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.TodoList
{
    public class ProgressService
    {
        public event EventHandler<ProgressUpdateEventArgs> OnProgressUpdated;

        public void UpdateProgress(int processedKeys, int totalKeys)
        {
            OnProgressUpdated?.Invoke(this, new ProgressUpdateEventArgs(processedKeys, totalKeys));
        }
    }

    public class ProgressUpdateEventArgs : EventArgs
    {
        public int ProcessedKeys { get; set; }
        public int TotalKeys { get; set; }

        public ProgressUpdateEventArgs(int processedKeys, int totalKeys)
        {
            ProcessedKeys = processedKeys;
            TotalKeys = totalKeys;
        }
    }
}
