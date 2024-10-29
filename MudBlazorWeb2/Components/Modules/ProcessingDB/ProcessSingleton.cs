// ProcessSingleton.cs

using System.ComponentModel;

namespace MudBlazorWeb2.Components.Modules.ProcessingDB
{
    public class ProcessSingleton
    {
        private static ProcessSingleton instance;
        private static readonly object padlock = new object();

        private ProcessSingleton()
        {
        }

        public static ProcessSingleton Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new ProcessSingleton();
                    }
                    return instance;
                }
            }
        }

        private int cycleInterval = 1;
        private int processedKeys = 0;
        private int totalKeys = 0;
        private bool isStoped = true;
        private string processingMessage = "";
        private bool isCycle = true;
        private bool isPlayed = false;
        private bool isPlayingNow = false;
        private bool isStopPressed = true;
        private DateTime startDate = DateTime.Now.AddMonths(-1);
        private DateTime endDate = DateTime.Now.AddMonths(1).AddYears(1);

        public int CycleInterval
        {
            get { return cycleInterval; }
            set
            {
                cycleInterval = value;
                OnPropertyChanged(nameof(CycleInterval));
            }
        }
        public int ProcessedKeys
        {
            get { return processedKeys; }
            set
            {
                processedKeys = value;
                OnPropertyChanged(nameof(ProcessedKeys));
            }
        }

        public int TotalKeys
        {
            get { return totalKeys; }
            set
            {
                totalKeys = value;
                OnPropertyChanged(nameof(TotalKeys));
            }
        }

        public bool IsStoped
        {
            get { return isStoped; }
            set
            {
                isStoped = value;
                OnPropertyChanged(nameof(IsStoped));
            }
        }

        public string ProcessingMessage
        {
            get { return processingMessage; }
            set
            {
                processingMessage = value;
                OnPropertyChanged(nameof(ProcessingMessage));
            }
        }

        public bool IsCycle
        {
            get { return isCycle; }
            set
            {
                isCycle = value;
                OnPropertyChanged(nameof(IsCycle));
            }
        }

        public bool IsPlayed
        {
            get { return isPlayed; }
            set
            {
                isPlayed = value;
                OnPropertyChanged(nameof(IsPlayed));
            }
        }

        public bool IsPlayingNow
        {
            get { return isPlayingNow; }
            set
            {
                isPlayingNow = value;
                OnPropertyChanged(nameof(IsPlayingNow));
            }
        }

        public bool IsStopPressed
        {
            get { return isStopPressed; }
            set
            {
                isStopPressed = value;
                OnPropertyChanged(nameof(IsStopPressed));
            }
        }
        public DateTime StartDate
        {
            get { return startDate; }
            set
            {
                startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }
        public DateTime EndDate
        {
            get { return endDate; }
            set
            {
                endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
