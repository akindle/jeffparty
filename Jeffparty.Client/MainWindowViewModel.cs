using Jeffparty.Interfaces;

namespace Jeffparty.Client
{
    public class MainWindowViewModel : Notifier
    {
        public string Title => IsHost ? $"Jeffparty - Host - {ConnectionId}" : $"Jeffparty - {ConnectionId}";

        public ContestantsViewModel ContestantsViewModel
        {
            get;
            set;
        }

        public HostViewModel HostViewModel
        {
            get;
            set;
        }

        public PlayerViewModel PlayerViewModel
        {
            get;
            set;
        }

        public bool IsHost
        {
            get => isHost;
            set
            {
                isHost = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPlayer));
                OnPropertyChanged(nameof(Title));
            }
        }

        public bool IsPlayer => !IsHost;
        private bool isHost;
        private string connectionId = "Connecting";

        public string ConnectionId
        {
            get => connectionId;
            set
            {
                connectionId = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public PersistedSettings PersistedSettings{get;set;}

        public MainWindowViewModel(IMessageHub dhub, PersistedSettings settings)
        {
            IsHost = settings.IsHost;
            PersistedSettings = settings;
            HostViewModel = new HostViewModel(dhub);
            PlayerViewModel = new PlayerViewModel(settings);
            ContestantsViewModel = new ContestantsViewModel();
        }
    }
}