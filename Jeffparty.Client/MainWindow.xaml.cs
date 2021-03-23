using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Xml.Serialization;
using Jeffparty.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong;

namespace Jeffparty.Client
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMessageSpoke
    {
        private readonly HubConnection connection;
        private readonly IMessageHub hub;
        private readonly MainWindowViewModel viewModel;
        private PersistedSettings settings;

        private PersistedSettings TryLoadSettings()
        {
            try
            {
                var settingsPath = $"{Directory.GetCurrentDirectory()}\\settings.xml";
                var xml = new XmlSerializer(typeof(PersistedSettings));
                using var stream = File.OpenRead(settingsPath);
                return ((PersistedSettings)xml.Deserialize(stream))!;
            }
            catch
            {
                var newSettings = new PersistedSettings(Guid.NewGuid(), "New Player", "https://localhost:44391/ChatHub");
                newSettings.SaveSettings();
                return newSettings;
            }
        }

        public MainWindow()
        {
            settings = TryLoadSettings();
            connection = new HubConnectionBuilder()
                .WithUrl(settings.HostUrl)
                .WithAutomaticReconnect()
                .Build();

            connection.Closed += async error =>
            {
                await Task.Delay(new Random().Next(0, 5) * 1000);
                await connection.StartAsync();
            };
            
            connection.RegisterSpoke<IMessageSpoke>(this);
            hub = connection.AsDynamicHub<IMessageHub>();
            viewModel = new MainWindowViewModel(hub, settings);
            DataContext = viewModel;
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await connection.StartAsync();
        }

        public async Task UpdateGameState(GameState state)
        {
            await Dispatcher.InvokeAsync(() => viewModel.PlayerViewModel.Update(state));
        }

        public async Task OnConnected()
        {
            await hub.NotifyPlayerJoined(settings.Guid, settings.PlayerName);
            viewModel.ConnectionId = "Connected";
        }

        public async Task NotifyPlayerJoined(ContestantViewModel joiner)
        {
            await Dispatcher.InvokeAsync(() => viewModel.ContestantsViewModel.Contestants.Add(joiner));
        }

        public async Task FindOrCreatePlayerData(Guid joiner, string playerName)
        {
            if (!viewModel.IsHost)
            {
                return;
            }

            var reconnector = viewModel.ContestantsViewModel.Contestants.FirstOrDefault(contestant => contestant.Guid == joiner);
            if (reconnector == null)
            {
                reconnector = new ContestantViewModel { PlayerName = playerName, Guid = joiner, Score = 0 };
            }
            else
            {
                reconnector.PlayerName = playerName;
            }

            await hub.FoundJoiningPlayer(reconnector);
        }
    }
}