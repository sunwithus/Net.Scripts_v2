using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MudBlazorWeb2.Components.Modules.WhOllProcessWithDb.Services
{
    public class StateService
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1).AddYears(1);
        public string TimeInterval { get; set; } = "00:00:10";
        public bool IsPlayingNow { get; set; } = false;
        public bool IsStoped { get; set; } = true;
        public bool IsStopPressed { get; set; } = true;
        public bool IsCycle { get; set; } = true;
        public int CycleInterval { get; set; } = 3;
        public int ProcessedKeys { get; set; } = 0;
        public int TotalKeys { get; set; } = 0;
        public string ProcessingMessage { get; set; } = "";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void ChangeState(bool isPlayingNow, bool isStoped, string processingMessage, int processedKeys = -1, int totalKeys = -1)
        {
            if (processedKeys >= 0 && totalKeys >= 0)
            {
                ProcessedKeys = processedKeys;
                TotalKeys = totalKeys;
            }
            ProcessingMessage = processingMessage;
            IsStoped = isStoped;
            IsPlayingNow = isPlayingNow;
            OnPropertyChanged();
        }
    }
}