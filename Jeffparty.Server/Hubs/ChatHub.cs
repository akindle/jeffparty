using System;
using System.Threading.Tasks;
using Jeffparty.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Jeffparty.Server.Hubs
{
    public class ChatHub : Hub<IMessageSpoke>, IMessageHub
    {

        public async Task<int> PropagateGameState(GameState state)
        {
            await Clients.Others.UpdateGameState(state);
            return 5;
        }

        public Task<bool> BuzzIn()
        {
            return Task.FromResult(false);
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

        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            await Clients.Caller.OnConnected();
        }
    }

}