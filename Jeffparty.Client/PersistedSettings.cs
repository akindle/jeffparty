using System;
using System.IO;
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

        public PersistedSettings(Guid guid, string playerName, string hostUrl)
        {
            Guid = guid;
            PlayerName = playerName;
            HostUrl = hostUrl;
            PropertyChanged += (_, __) => SaveSettings();
            IsHost = false;
        }

        private PersistedSettings(){}

        public void SaveSettings()
        {
            try
            {
                var settingsPath = $"{Directory.GetCurrentDirectory()}\\settings.xml";
                var xml = new XmlSerializer(typeof(PersistedSettings));
                using var stream = File.OpenWrite(settingsPath);
                xml.Serialize(stream, this);
            }
            catch{}
        }
    }
}