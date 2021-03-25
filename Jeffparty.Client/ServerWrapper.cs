using System;
using System.Threading.Tasks;
using Jeffparty.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using SignalR.Strong;

namespace Jeffparty.Client
{
    public class ServerWrapper : IMessageHub
    {
        private readonly IMessageSpoke _spoke;
        private IMessageHub? _messageHubImplementation;
        private HubConnection? _underlyingConnection;

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
                        _underlyingConnection.RegisterSpoke(_spoke);
                        _messageHubImplementation = _underlyingConnection.AsDynamicHub<IMessageHub>();
                        await _underlyingConnection.StartAsync();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"Failed to bind to {_hostUrl}: {e}");
            }
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
            return await (_messageHubImplementation?.PropagateGameState(state) ?? Task.FromResult(false));
        }

        public async Task<bool> NotifyPlayerJoined(Guid joiner, string playerName)
        {
            _logger.Trace();
            return await (_messageHubImplementation?.NotifyPlayerJoined(joiner, playerName) ?? Task.FromResult(false));
        }

        public async Task<bool> FoundJoiningPlayer(ContestantViewModel contestant)
        {
            _logger.Trace();
            return await (_messageHubImplementation?.FoundJoiningPlayer(contestant) ?? Task.FromResult(false));
        }

        public async Task<bool> BuzzIn(Guid buzzingPlayer, double timerSecondsAtBuzz)
        {
            _logger.Trace();
            return await (_messageHubImplementation?.BuzzIn(buzzingPlayer, timerSecondsAtBuzz) ?? Task.FromResult(false));
        }

        public async Task<bool> SubmitWager(Guid settingsGuid, int playerViewWager)
        {
            _logger.Trace();
            return await (_messageHubImplementation?.SubmitWager(settingsGuid, playerViewWager) ?? Task.FromResult(false));
        }
    }
}