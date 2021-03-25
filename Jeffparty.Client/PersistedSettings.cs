using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using Jeffparty.Interfaces;

namespace Jeffparty.Client
{
    [Serializable]
    public class PersistedSettings : Notifier
    {
        public Guid Guid{get;set;}

        public string PlayerName{get;set;} = string.Empty;

        public string HostUrl
        {
            get;
            set;
        } = string.Empty;

        public bool IsHost{get;set;}
        
        [XmlIgnore]
        public MainWindowViewModel? MainWindow { get; set; }

        public PersistedSettings(Guid guid, string playerName, string hostUrl)
        {
            Guid = guid;
            PlayerName = playerName;
            HostUrl = hostUrl;
            IsHost = false;
            ConfigureEventing();
        }

        public void ConfigureEventing()
        {
            PropertyChanged += OnPropertyChangedEventHandler;
        }

        private async void OnPropertyChangedEventHandler(object? _, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(PlayerName) || e.PropertyName == nameof(Guid))
            {
                if (MainWindow != null)
                {
                    await MainWindow.Server.NotifyPlayerJoined(Guid, PlayerName);
                }
            }
            else if (e.PropertyName == nameof(HostUrl) && MainWindow?.Server is ServerWrapper hub)
            {
                await hub.ChangeHostUrl(HostUrl);
            }

            if (e.PropertyName == nameof(PlayerName) || e.PropertyName == nameof(Guid) || e.PropertyName == nameof(HostUrl))
            {
                SaveSettings();
            }
        }

        private PersistedSettings()
        {
        }

        public void SaveSettings()
        {
            try
            {
                var settingsPath = $"{Directory.GetCurrentDirectory()}\\settings.xml";
                var xml = new XmlSerializer(typeof(PersistedSettings));
                using var stream = File.OpenWrite(settingsPath);
                xml.Serialize(stream, this);
            }
            catch 
            {
            }
        }
    }
}