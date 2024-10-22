//OraSingleton.cs
/*
using System.ComponentModel;

namespace MudBlazorWeb2.Components.Modules.Replicator
{
    public class OraSingleton
    {
        private static OraSingleton instance;
        private static readonly object padlock = new object();

        private OraSingleton()
        {
        }

        public static OraSingleton Instance
        {
            get
            {
                lock (padlock)
                {
                    if (instance == null)
                    {
                        instance = new OraSingleton();
                    }
                    return instance;
                }
            }
        }

        private bool isStoped = true;
        public bool IsStoped
        {
            get { return isStoped; }
            set
            {
                isStoped = value;
                OnPropertyChanged(nameof(IsStoped));
            }
        }

        private int progressExec = 0;
        public int ProgressExec
        {
            get { return progressExec; }
            set
            {
                progressExec = value;
                OnPropertyChanged(nameof(ProgressExec));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
*/