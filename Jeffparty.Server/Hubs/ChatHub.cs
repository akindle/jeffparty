using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Jeffparty.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Jeffparty.Server.Hubs
{
    public class ChatHub : Hub<IMessageSpoke>, IMessageHub
    {
        private static readonly ConcurrentDictionary<string, string> ConnectionLobby = new();
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        public async Task<bool> JoinLobby(string lobbyCode)
        {
            var connectionId = Context.ConnectionId;

            // Remove from previous lobby if switching
            if (ConnectionLobby.TryGetValue(connectionId, out var previousLobby))
            {
                await Groups.RemoveFromGroupAsync(connectionId, previousLobby);
            }

            ConnectionLobby[connectionId] = lobbyCode;
            await Groups.AddToGroupAsync(connectionId, lobbyCode);
            _logger.LogInformation("Client {ConnectionId} joined lobby {Lobby}", connectionId, lobbyCode);
            return true;
        }

        public async Task<bool> PropagateGameState(GameState state)
        {
            _logger.LogDebug("PropagateGameState: CanBuzzIn={CanBuzzIn}, ShouldShowQuestion={Show}, Question={Q}",
                state.CanBuzzIn, state.ShouldShowQuestion, state.CurrentQuestion?[..Math.Min(30, state.CurrentQuestion?.Length ?? 0)]);
            if (TryGetLobby(out var lobby))
                await Clients.OthersInGroup(lobby).UpdateGameState(state);
            return true;
        }

        public async Task<bool> NotifyPlayerJoined(Guid joiner, string playerName)
        {
            if (TryGetLobby(out var lobby))
                await Clients.Group(lobby).FindOrCreatePlayerData(joiner, playerName);
            return true;
        }

        public async Task<bool> FoundJoiningPlayer(ContestantViewModel contestant)
        {
            if (TryGetLobby(out var lobby))
                await Clients.Group(lobby).NotifyPlayerJoined(contestant);
            return true;
        }

        public async Task<bool> BuzzIn(Guid buzzingPlayer, double timerSecondsAtBuzz)
        {
            _logger.LogInformation("BuzzIn from {Player} at {Timer}s", buzzingPlayer, timerSecondsAtBuzz);
            if (TryGetLobby(out var lobby))
                await Clients.Group(lobby).NotifyPlayerBuzzed(buzzingPlayer, timerSecondsAtBuzz);
            return true;
        }

        public async Task<bool> SubmitWager(Guid settingsGuid, int playerViewWager)
        {
            if (TryGetLobby(out var lobby))
                await Clients.Group(lobby).NotifyPlayerWagered(settingsGuid, playerViewWager);
            return true;
        }

        public async Task<bool> QueryConnectedPlayers()
        {
            if (TryGetLobby(out var lobby))
                await Clients.OthersInGroup(lobby).OnConnected();
            return true;
        }

        public async Task<bool> SubmitFinalJeopardyAnswer(Guid settingsGuid, string playerFinalJeopardyAnswer)
        {
            if (TryGetLobby(out var lobby))
                await Clients.Group(lobby).NotifyFinalJeopardyAnswer(settingsGuid, playerFinalJeopardyAnswer);
            return true;
        }

        public async Task<bool> RequestPlayAudio(AudioClips clip)
        {
            if (TryGetLobby(out var lobby))
                await Clients.Group(lobby).DoPlayAudio(clip);
            return true;
        }

        public async Task<bool> KickPlayer(Guid playerGuid)
        {
            _logger.LogInformation("KickPlayer: {PlayerGuid}", playerGuid);
            if (TryGetLobby(out var lobby))
                await Clients.Group(lobby).NotifyPlayerKicked(playerGuid);
            return true;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
            await base.OnConnectedAsync();
            await Clients.Caller.OnConnected();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation("Client disconnected: {ConnectionId}, Exception: {Exception}",
                Context.ConnectionId, exception?.Message);
            ConnectionLobby.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        private bool TryGetLobby(out string lobby)
        {
            if (ConnectionLobby.TryGetValue(Context.ConnectionId, out lobby!))
                return true;

            _logger.LogWarning("Client {ConnectionId} sent a message without joining a lobby", Context.ConnectionId);
            return false;
        }
    }

}