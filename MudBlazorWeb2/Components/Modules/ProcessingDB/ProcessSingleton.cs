using MudBlazorWeb2.Components.Modules.Replicator;
using System.ComponentModel;

public class ProcessSingleton
{
    private static readonly Lazy<ProcessSingleton> instance = new Lazy<ProcessSingleton>(() => new ProcessSingleton());

    private ProcessSingleton()
    {
    }

    public static ProcessSingleton Instance
    {
        get { return instance.Value; }
    }

    private int cycleInterval = 1;
    private int processedKeys = 0;
    private int totalKeys = 0;
    private string processingMessage = "";
    private bool isCycle = true;

    private bool isStoped2 = true;
    private bool isPlayingNow = false;
    private bool isStopPressed = true;

    private DateTime startDate = DateTime.Now.AddMonths(-1);
    private DateTime endDate = DateTime.Now.AddMonths(1).AddYears(1);

    public bool IsStoped2
    {
        get { return isStoped2; }
        set
        {
            isStoped2 = value;
            OnPropertyChanged(nameof(IsStoped2));
        }
    }
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

    public event PropertyChangedEventHandler PropertyChanged2;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged2?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}