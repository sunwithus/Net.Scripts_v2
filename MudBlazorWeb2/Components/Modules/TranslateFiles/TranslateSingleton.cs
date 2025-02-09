//ReplSingleton.cs

using System.ComponentModel;

namespace MudBlazorWeb2.Components.Modules.TranslateFiles;

public class TranslateSingleton
{
    private static TranslateSingleton instance;
    private static readonly object padlock = new object();

    private TranslateSingleton()
    {
    }

    public static TranslateSingleton TranslateInstance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new TranslateSingleton();
                }
                return instance;
            }
        }
    }

    private bool isStopTranlation = false;
    public bool IsStopTranlation
    {
        get { return isStopTranlation; }
        set
        {
            isStopTranlation = value;
            OnPropertyChanged(nameof(IsStopTranlation));
        }
    }

    private string processTranlation = "";
    public string ProcessTranlation
    {
        get { return processTranlation; }
        set
        {
            processTranlation = value;
            OnPropertyChanged(nameof(ProcessTranlation));
        }
    }

    public event PropertyChangedEventHandler TranslatePropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        TranslatePropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
