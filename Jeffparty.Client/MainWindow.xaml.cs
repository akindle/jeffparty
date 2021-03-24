using System;
using System.Diagnostics;
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
                var newSettings = new PersistedSettings(Guid.NewGuid(), "New Player", "https://jeffparty.alexkindle.com/ChatHub");
                newSettings.SaveSettings();
                return newSettings;
            }
        }

        public MainWindow()
        {
            try
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
            }
            catch (Exception e)
            {
                Debug.Fail(e.ToString());
            }

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
            if (viewModel.IsPlayer)
            {
                await hub.NotifyPlayerJoined(settings.Guid, settings.PlayerName);
            }

            viewModel.ConnectionId = "Connected";
        }

        public async Task NotifyPlayerJoined(ContestantViewModel joiner)
        {
            await Dispatcher.InvokeAsync(() =>
            {
                var possibleReconnect =
                    viewModel.ContestantsViewModel.Contestants.FirstOrDefault(contestant =>
                        contestant.Guid == joiner.Guid);
                if (possibleReconnect != null)
                {
                    possibleReconnect.PlayerName = joiner.PlayerName;
                }
                else
                {
                    viewModel.ContestantsViewModel.Contestants.Add(joiner);
                }
            });
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
            await viewModel.HostViewModel.GameManager.PropagateGameState();
        }

        public async Task NotifyPlayerBuzzed(Guid buzzingPlayer, double timerSecondsAtBuzz)
        {
            var p =
                viewModel.ContestantsViewModel.Contestants.FirstOrDefault(
                    contestant => contestant.Guid == buzzingPlayer);
            if (p != null)
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    p.IsBuzzed = true;
                    if (viewModel.IsHost)
                    {
                        await viewModel.HostViewModel.GameManager.PlayerBuzzed(p, timerSecondsAtBuzz);
                    }
                });
            }
        }

        public async Task NotifyPlayerWagered(Guid settingsGuid, int playerViewWager)
        {
            if (!viewModel.IsHost)
            {
                return;
            }
            
            var p =
                viewModel.ContestantsViewModel.Contestants.FirstOrDefault(
                    contestant => contestant.Guid == settingsGuid);
            if (p != null)
            {
                await Dispatcher.InvokeAsync(async () =>
                {
                    await viewModel.HostViewModel.GameManager.PlayerWagered(p, playerViewWager);
                });
            }
        }
    }
}