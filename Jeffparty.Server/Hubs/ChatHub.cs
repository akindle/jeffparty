using System.Threading.Tasks;
using Jeffparty.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Jeffparty.Server.Hubs
{
    public class ChatHub : Hub<IMessageSpoke>, IMessageHub
    {
        public async Task<bool> SendMessage(string user, string message)
        {
            await Clients.All.ReceiveMessage(user, message);
            return true;
        }

        public async Task<bool> PropagateGameState(GameState state)
        {
            await Clients.Others.UpdateGameState(state);
            return true;
        }
    }

}