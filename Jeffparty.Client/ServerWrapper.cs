using System;
using System.Threading.Tasks;
using Jeffparty.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;

namespace Jeffparty.Client
{
    public class ServerWrapper : IMessageHub
    {
        private readonly IMessageSpoke _spoke;
        private HubConnection? _underlyingConnection;
        public string? LobbyCode { get; set; }

        public async Task ChangeHostUrl(string? newHost)
        {
            try
            {
                if (_hostUrl != newHost)
                {
                    _logger.LogInformation($"Changing host from {_hostUrl} to {newHost}");
                    if (_underlyingConnection != null) await _underlyingConnection.StopAsync();
                    _hostUrl = newHost;
                    if (_hostUrl != null)
                    {
                        _underlyingConnection =
                            new HubConnectionBuilder().WithUrl(_hostUrl).WithAutomaticReconnect().Build();
                        RegisterSpokeHandlers(_underlyingConnection, _spoke);
                        await _underlyingConnection.StartAsync();
                        if (!string.IsNullOrEmpty(LobbyCode))
                            await JoinLobby(LobbyCode);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to bind to {_hostUrl}: {e}");
            }
        }

        private static void RegisterSpokeHandlers(HubConnection connection, IMessageSpoke spoke)
        {
            connection.On<GameState>(nameof(IMessageSpoke.UpdateGameState), spoke.UpdateGameState);
            connection.On(nameof(IMessageSpoke.OnConnected), spoke.OnConnected);
            connection.On<ContestantViewModel>(nameof(IMessageSpoke.NotifyPlayerJoined), spoke.NotifyPlayerJoined);
            connection.On<Guid, string>(nameof(IMessageSpoke.FindOrCreatePlayerData), spoke.FindOrCreatePlayerData);
            connection.On<Guid, double>(nameof(IMessageSpoke.NotifyPlayerBuzzed), spoke.NotifyPlayerBuzzed);
            connection.On<Guid, int>(nameof(IMessageSpoke.NotifyPlayerWagered), spoke.NotifyPlayerWagered);
            connection.On<Guid, string>(nameof(IMessageSpoke.NotifyFinalJeopardyAnswer), spoke.NotifyFinalJeopardyAnswer);
            connection.On<AudioClips>(nameof(IMessageSpoke.DoPlayAudio), spoke.DoPlayAudio);
            connection.On<Guid>(nameof(IMessageSpoke.NotifyPlayerKicked), spoke.NotifyPlayerKicked);
        }

        private string? _hostUrl;
        private readonly ILogger<ServerWrapper> _logger;

        public ServerWrapper(IMessageSpoke spoke, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ServerWrapper>();
            _spoke = spoke;
        }
        
        public async Task<bool> PropagateGameState(GameState state)
        {
            _logger.Trace();
            if (_underlyingConnection == null) return false;
            return await _underlyingConnection.InvokeAsync<bool>(nameof(IMessageHub.PropagateGameState), state);
        }

        public async Task<bool> NotifyPlayerJoined(Guid joiner, string playerName)
        {
            _logger.Trace();
            if (_underlyingConnection == null) return false;
            return await _underlyingConnection.InvokeAsync<bool>(nameof(IMessageHub.NotifyPlayerJoined), joiner, playerName);
        }

        public async Task<bool> FoundJoiningPlayer(ContestantViewModel contestant)
        {
            _logger.Trace();
            if (_underlyingConnection == null) return false;
            return await _underlyingConnection.InvokeAsync<bool>(nameof(IMessageHub.FoundJoiningPlayer), contestant);
        }

        public async Task<bool> BuzzIn(Guid buzzingPlayer, double timerSecondsAtBuzz)
        {
            _logger.Trace();
            if (_underlyingConnection == null) return false;
            return await _underlyingConnection.InvokeAsync<bool>(nameof(IMessageHub.BuzzIn), buzzingPlayer, timerSecondsAtBuzz);
        }

        public async Task<bool> SubmitWager(Guid settingsGuid, int playerViewWager)
        {
            _logger.Trace();
            if (_underlyingConnection == null) return false;
            return await _underlyingConnection.InvokeAsync<bool>(nameof(IMessageHub.SubmitWager), settingsGuid, playerViewWager);
        }

        public async Task<bool> QueryConnectedPlayers()
        {
            _logger.Trace();
            if (_underlyingConnection == null) return false;
            return await _underlyingConnection.InvokeAsync<bool>(nameof(IMessageHub.QueryConnectedPlayers));
        }

        public async Task<bool> SubmitFinalJeopardyAnswer(Guid settingsGuid, string playerFinalJeopardyAnswer)
        {
            _logger.Trace();
            if (_underlyingConnection == null) return false;
            return await _underlyingConnection.InvokeAsync<bool>(nameof(IMessageHub.SubmitFinalJeopardyAnswer), settingsGuid, playerFinalJeopardyAnswer);
        }

        public async Task<bool> RequestPlayAudio(AudioClips clip)
        {
            _logger.Trace();
            if (_underlyingConnection == null) return false;
            return await _underlyingConnection.InvokeAsync<bool>(nameof(IMessageHub.RequestPlayAudio), clip);
        }

        public async Task<bool> KickPlayer(Guid playerGuid)
        {
            _logger.Trace();
            if (_underlyingConnection == null) return false;
            return await _underlyingConnection.InvokeAsync<bool>(nameof(IMessageHub.KickPlayer), playerGuid);
        }

        public async Task<bool> JoinLobby(string lobbyCode)
        {
            _logger.Trace();
            if (_underlyingConnection == null) return false;
            return await _underlyingConnection.InvokeAsync<bool>(nameof(IMessageHub.JoinLobby), lobbyCode);
        }
    }
}