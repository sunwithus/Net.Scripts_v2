//ReplSingletonService.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MudBlazorWeb2.Components.Modules.Replicator.Services
{
    public class ReplSingletonService : INotifyPropertyChanged
    {
        private bool _isStoped = true;
        public bool IsStoped
        {
            get => _isStoped;
            set
            {
                if (_isStoped != value)
                {
                    _isStoped = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _progressExec = 0;
        public int ProgressExec
        {
            get => _progressExec;
            set
            {
                if (_progressExec != value)
                {
                    _progressExec = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


}