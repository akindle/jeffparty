using System;
using System.Threading.Tasks;
using Jeffparty.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace Jeffparty.Server.Hubs
{
    public class ChatHub : Hub<IMessageSpoke>, IMessageHub
    {
        private readonly ILogger<ChatHub> _logger;

        public ChatHub(ILogger<ChatHub> logger)
        {
            _logger = logger;
        }

        public async Task<bool> PropagateGameState(GameState state)
        {
            _logger.LogDebug("PropagateGameState: CanBuzzIn={CanBuzzIn}, ShouldShowQuestion={Show}, Question={Q}",
                state.CanBuzzIn, state.ShouldShowQuestion, state.CurrentQuestion?[..Math.Min(30, state.CurrentQuestion?.Length ?? 0)]);
            await Clients.Others.UpdateGameState(state);
            return true;
        }

        public async Task<bool> NotifyPlayerJoined(Guid joiner, string playerName)
        {
            await Clients.All.FindOrCreatePlayerData(joiner, playerName);
            return true;
        }

        public async Task<bool> FoundJoiningPlayer(ContestantViewModel contestant)
        {
            await Clients.All.NotifyPlayerJoined(contestant);
            return true;
        }

        public async Task<bool> BuzzIn(Guid buzzingPlayer, double timerSecondsAtBuzz)
        {
            _logger.LogInformation("BuzzIn from {Player} at {Timer}s", buzzingPlayer, timerSecondsAtBuzz);
            await Clients.All.NotifyPlayerBuzzed(buzzingPlayer, timerSecondsAtBuzz);
            return true;
        }

        public async Task<bool> SubmitWager(Guid settingsGuid, int playerViewWager)
        {
            await Clients.All.NotifyPlayerWagered(settingsGuid, playerViewWager);
            return true;
        }

        public async Task<bool> QueryConnectedPlayers()
        {
            await Clients.Others.OnConnected();
            return true;
        }

        public async Task<bool> SubmitFinalJeopardyAnswer(Guid settingsGuid, string playerFinalJeopardyAnswer)
        {
            await Clients.All.NotifyFinalJeopardyAnswer(settingsGuid, playerFinalJeopardyAnswer);
            return true;
        }

        public async Task<bool> RequestPlayAudio(AudioClips clip)
        {
            await Clients.All.DoPlayAudio(clip);
            return true;
        }

        public async Task<bool> KickPlayer(Guid playerGuid)
        {
            _logger.LogInformation("KickPlayer: {PlayerGuid}", playerGuid);
            await Clients.All.NotifyPlayerKicked(playerGuid);
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
            return base.OnDisconnectedAsync(exception);
        }
    }

}