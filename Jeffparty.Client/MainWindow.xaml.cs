using System;
using System.Collections.Generic;
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
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
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
            #nullable disable
            try
            {
                var settingsPath = $"{Directory.GetCurrentDirectory()}\\settings.xml";
                _logger.LogInformation($"Attempting to load settings from {settingsPath}");
                var xml = new XmlSerializer(typeof(PersistedSettings));
                using var stream = File.OpenRead(settingsPath);
                var newSettings = ((PersistedSettings) xml.Deserialize(stream))!;
                newSettings.ConfigureEventing();
                _logger.LogInformation("Loaded settings");
                return newSettings;
            }
            #nullable restore
            catch (Exception e)
            {
                _logger.LogWarning($"Settings load failed: {e}");
                _logger.LogInformation("Generating default settings");
                var newSettings = new PersistedSettings(Guid.NewGuid(), "New Player",
                    "https://jeffparty.alexkindle.com/ChatHub");
                newSettings.SaveSettings();
                return newSettings;
            }
        }

        public static ILoggerFactory LogFactory { get; } = LoggerFactory.Create(builder =>
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

        public MainWindow()
        {
            _logger = LogFactory.CreateLogger<MainWindow>();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            settings = TryLoadSettings();
            hub = new ServerWrapper(this, LogFactory);
            viewModel = new MainWindowViewModel(hub, settings, LogFactory);
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
            await Dispatcher.InvokeAsync(() => viewModel.PlayerViewModel?.Update(state));
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
            if (!viewModel.IsHost || viewModel.HostViewModel == null)
            {
                return;
            }

            var reconnector =
                viewModel.ContestantsViewModel.Contestants.FirstOrDefault(contestant => contestant.Guid == joiner);
            if (reconnector == null)
            {
                reconnector = new ContestantViewModel {PlayerName = playerName, Guid = joiner, Score = 0};
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
            _logger.Trace(buzzingPlayer.ToString());
            if (viewModel.ContestantsViewModel.Contestants.Any(contestant => contestant.IsBuzzed))
            {
                return;
            }
            
            var p =
                viewModel.ContestantsViewModel.Contestants.FirstOrDefault(
                    contestant => contestant.Guid == buzzingPlayer);
            _logger.Trace(p?.ToString() ?? "player wasn't found");
            if (p != null && viewModel.IsHost && viewModel.HostViewModel != null)
            {
                await Dispatcher.InvokeAsync(async () => await viewModel.HostViewModel.GameManager.PlayerBuzzed(p, timerSecondsAtBuzz));
            }
        }

        public async Task NotifyPlayerWagered(Guid settingsGuid, int playerViewWager)
        {
            _logger.Trace();
            if (!viewModel.IsHost || viewModel.HostViewModel == null)
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

        public async Task NotifyFinalJeopardyAnswer(Guid settingsGuid, string playerFinalJeopardyAnswer)
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
                await Dispatcher.InvokeAsync(() => { p.FinalJeopardyAnswer = playerFinalJeopardyAnswer; });
            }
        }

        public Task DoPlayAudio(AudioClips clip)
        {
            var soundPath = clip switch
            {
                AudioClips.Buzz => "./Sounds/buzz.mp3",
                AudioClips.Ding => "./Sounds/ding.mp3",
                AudioClips.Airhorn => "./Sounds/mlg-airhorn.mp3",
                AudioClips.Timeout => "./Sounds/timeout.mp3",
                AudioClips.Wrong => "./Sounds/wrong.mp3",
                _ => string.Empty
            };

            AudioPlaybackEngine.Instance.PlaySound(soundPath);
            return Task.CompletedTask;
        }
    }

    class AudioPlaybackEngine : IDisposable
    {
        private readonly IWavePlayer outputDevice;
        private readonly MixingSampleProvider mixer;

        public AudioPlaybackEngine(int sampleRate = 44100, int channelCount = 2)
        {
            _logger = MainWindow.LogFactory.CreateLogger<AudioPlaybackEngine>();
            outputDevice = new WaveOutEvent();
            mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, channelCount));
            mixer.ReadFully = true;
            outputDevice.Init(mixer);
            outputDevice.Play();
        }

        public void PlaySound(string fileName)
        {
            _logger.LogDebug($"Playing file {fileName}");
            try
            {
                var input = new AudioFileReader(fileName);
                AddMixerInput(new AutoDisposeFileReader(input));
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Failed to play audio file {fileName} with exception {e}");
            }
        }

        private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
        {
            if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
            {
                return input;
            }

            if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
            {
                return new MonoToStereoSampleProvider(input);
            }

            throw new NotImplementedException("Not yet implemented this channel count conversion");
        }

        public void PlaySound(CachedSound sound)
        {
            AddMixerInput(new CachedSoundSampleProvider(sound));
        }

        private void AddMixerInput(ISampleProvider input)
        {
            mixer.AddMixerInput(ConvertToRightChannelCount(input));
        }

        public void Dispose()
        {
            outputDevice.Dispose();
        }

        public static readonly AudioPlaybackEngine Instance = new AudioPlaybackEngine(44100, 2);
        private ILogger<AudioPlaybackEngine> _logger;
    }

    class CachedSound
    {
        public float[] AudioData { get; private set; }
        public WaveFormat WaveFormat { get; private set; }

        public CachedSound(string audioFileName)
        {
            using (var audioFileReader = new AudioFileReader(audioFileName))
            {
                // TODO: could add resampling in here if required
                WaveFormat = audioFileReader.WaveFormat;
                var wholeFile = new List<float>((int) (audioFileReader.Length / 4));
                var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];
                int samplesRead;
                while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
                {
                    wholeFile.AddRange(readBuffer.Take(samplesRead));
                }

                AudioData = wholeFile.ToArray();
            }
        }
    }

    class CachedSoundSampleProvider : ISampleProvider
    {
        private readonly CachedSound cachedSound;
        private long position;

        public CachedSoundSampleProvider(CachedSound cachedSound)
        {
            this.cachedSound = cachedSound;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            var availableSamples = cachedSound.AudioData.Length - position;
            var samplesToCopy = Math.Min(availableSamples, count);
            Array.Copy(cachedSound.AudioData, position, buffer, offset, samplesToCopy);
            position += samplesToCopy;
            return (int) samplesToCopy;
        }

        public WaveFormat WaveFormat
        {
            get { return cachedSound.WaveFormat; }
        }
    }

    class AutoDisposeFileReader : ISampleProvider
    {
        private readonly AudioFileReader reader;
        private bool isDisposed;

        public AutoDisposeFileReader(AudioFileReader reader)
        {
            this.reader = reader;
            this.WaveFormat = reader.WaveFormat;
        }

        public int Read(float[] buffer, int offset, int count)
        {
            if (isDisposed)
                return 0;
            int read = reader.Read(buffer, offset, count);
            if (read == 0)
            {
                reader.Dispose();
                isDisposed = true;
            }

            return read;
        }

        public WaveFormat WaveFormat { get; private set; }
    }
}