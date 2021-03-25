using System;
using System.Threading.Tasks;
using Jeffparty.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Jeffparty.Server.Hubs
{
    public class ChatHub : Hub<IMessageSpoke>, IMessageHub
    {

        public async Task<bool> PropagateGameState(GameState state)
        {
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

        public async Task<bool> RequestPlayAnswerAudio(bool isCorrect)
        {
            await Clients.All.DoPlayAnswerAudio(isCorrect);
            return true;
        }

        public async Task<bool> RequestPlayTimeoutAudio()
        {
            await Clients.All.DoPlayTimeoutAudio();
            return true;
        }

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Caller.OnConnected();
        }
    }

}