using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Jeffparty.Interfaces
{
    public abstract class Notifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}