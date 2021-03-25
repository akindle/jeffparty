using System;
using System.Threading.Tasks;

namespace Jeffparty.Interfaces
{
    public interface IMessageSpoke
    {
        Task UpdateGameState(GameState state);

        Task OnConnected();
        Task NotifyPlayerJoined(ContestantViewModel joiner);
        Task FindOrCreatePlayerData(Guid joiner, string playerName);
        Task NotifyPlayerBuzzed(Guid buzzingPlayer, double timerSecondsAtBuzz);
        Task NotifyPlayerWagered(Guid settingsGuid, int playerViewWager);
    }

    public interface IMessageHub
    {
        Task<bool> PropagateGameState(GameState state);
        Task<bool> NotifyPlayerJoined(Guid joiner, string playerName);
        Task<bool> FoundJoiningPlayer(ContestantViewModel contestant);
        Task<bool> BuzzIn(Guid buzzingPlayer, double timerSecondsAtBuzz);
        Task<bool> SubmitWager(Guid settingsGuid, int playerViewWager);
        Task<bool> QueryConnectedPlayers();
    }
}