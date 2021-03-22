using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Jeffparty.Interfaces
{
    public interface IMessageSpoke
    {
        Task ReceiveMessage(string user, string message);

        Task UpdateGameState(GameState state);
    }

    public interface IMessageHub
    {
        Task<bool> SendMessage(string user, string message);
    }

    public struct GameState
    {
    }
}
