using Jeffparty.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jeffparty.Client
{
    public class MainWindowViewModel : Notifier
    {
        public string Title => IsHost ? $"Jeffparty - Host - {ConnectionState}" : $"Jeffparty - {ConnectionState}";

        public ContestantsViewModel ContestantsViewModel { get; set; }

        public HostViewModel HostViewModel { get; set; }

        public PlayerViewModel PlayerViewModel { get; set; }

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
        private string _connectionState = "Connecting";

        public string ConnectionState
        {
            get => _connectionState;
            set
            {
                _connectionState = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Title));
            }
        }

        public IMessageHub Server { get; }
        public PersistedSettings PersistedSettings { get; set; }

        public MainWindowViewModel(IMessageHub server, PersistedSettings settings, ILoggerFactory loggerFactory)
        {
            IsHost = settings.IsHost;
            Server = server;
            PersistedSettings = settings;
            settings.MainWindow = this;

            ContestantsViewModel = new ContestantsViewModel {ShowWagerColumn = IsHost};
            PlayerViewModel = new PlayerViewModel(settings, server, ContestantsViewModel);
            HostViewModel = new HostViewModel(server, ContestantsViewModel, loggerFactory);
        }
    }
}