using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.Xml.Serialization;
using Jeffparty.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using Microsoft.Extensions.Logging.EventLog;
using NLog.Extensions.Logging;
using SignalR.Strong;

namespace Jeffparty.Client
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMessageSpoke
    {
        private readonly ILogger<MainWindow> _logger;
        private readonly ServerWrapper hub;
        private readonly MainWindowViewModel viewModel;
        private PersistedSettings settings;

        private PersistedSettings TryLoadSettings()
        {
            try
            {
                var settingsPath = $"{Directory.GetCurrentDirectory()}\\settings.xml";
                _logger.LogInformation($"Attempting to load settings from {settingsPath}");
                var xml = new XmlSerializer(typeof(PersistedSettings));
                using var stream = File.OpenRead(settingsPath);
                var newSettings = ((PersistedSettings)xml.Deserialize(stream))!;
                newSettings.ConfigureEventing();
                _logger.LogInformation("Loaded settings");
                return newSettings;
            }
            catch(Exception e)
            {
                _logger.LogWarning($"Settings load failed: {e}");
                _logger.LogInformation("Generating default settings");
                var newSettings = new PersistedSettings(Guid.NewGuid(), "New Player", "https://jeffparty.alexkindle.com/ChatHub");
                newSettings.SaveSettings();
                return newSettings;
            }
        }

        public MainWindow()
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("Jeffparty", LogLevel.Trace)
                    .AddProvider(new DebugLoggerProvider())
                    .AddProvider(new EventLogLoggerProvider(new EventLogSettings
                    {
                        Filter = (_, level) => level switch
                        {
                            LogLevel.Trace => false,
                            LogLevel.Debug => false,
                            LogLevel.Information => true,
                            LogLevel.Warning => true,
                            LogLevel.Error => true,
                            LogLevel.Critical => true,
                            LogLevel.None => true,
                            _ => throw new ArgumentOutOfRangeException(nameof(level), level, null)
                        }
                    }))
                    .AddProvider(new NLogLoggerProvider());
            });
            _logger = loggerFactory.CreateLogger<MainWindow>();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
        
            settings = TryLoadSettings();
            hub = new ServerWrapper(this, loggerFactory);
            viewModel = new MainWindowViewModel(hub, settings, loggerFactory);
            DataContext = viewModel;
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = (Exception) e.ExceptionObject;
            _logger.LogCritical("MyHandler caught : " + ex);
            _logger.LogCritical($"Runtime terminating: {e.IsTerminating}");
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await hub.ChangeHostUrl(settings.HostUrl);
        }

        public async Task UpdateGameState(GameState state)
        {
            _logger.Trace();
            await Dispatcher.InvokeAsync(() => viewModel.PlayerViewModel.Update(state));
        }

        public async Task OnConnected()
        {
            _logger.Trace();
            if (viewModel.IsPlayer)
            {
                await hub.NotifyPlayerJoined(settings.Guid, settings.PlayerName);
            }
            else
            {
                await hub.QueryConnectedPlayers();
            }

            viewModel.ConnectionState = "Connected";
        }

        public async Task NotifyPlayerJoined(ContestantViewModel joiner)
        {
            _logger.Trace();
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
            _logger.Trace();
            if (!viewModel.IsHost)
            {
                return;
            }

            var reconnector = viewModel.ContestantsViewModel.Contestants.FirstOrDefault(contestant => contestant.Guid == joiner);
            if (reconnector == null)
            {
                reconnector = new ContestantViewModel { PlayerName = playerName, Guid = joiner, Score = 0 };
                viewModel.ContestantsViewModel.Contestants.Add(reconnector);
            }
            else
            {
                reconnector.PlayerName = playerName;
            }

            foreach (var player in viewModel.ContestantsViewModel.Contestants)
            {
                await hub.FoundJoiningPlayer(player);
            }
            
            await viewModel.HostViewModel.GameManager.PropagateGameState();
        }

        public async Task NotifyPlayerBuzzed(Guid buzzingPlayer, double timerSecondsAtBuzz)
        {
            _logger.Trace();
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
            _logger.Trace();
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