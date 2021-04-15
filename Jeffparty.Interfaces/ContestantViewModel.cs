using System;

namespace Jeffparty.Interfaces
{
    public sealed class ContestantViewModel : Notifier
    {
        public ContestantViewModel()
        {
            PlayerName = "Player 1";
        }

        public string PlayerName { get; set; }

        public int Score { get; set; }

        public int? Wager { get; set; }

        public string? FinalJeopardyAnswer { get; set; }

        public Guid Guid { get; set; }

        public bool IsBuzzed { get; set; }

        public int ScoreOverride { get; set; }

        public override string ToString()
        {
            return
                $"PlayerName = \"{PlayerName}\", Score = \"{Score}\", Wager = \"{Wager}\", FinalJeopardyAnswer = \"{FinalJeopardyAnswer}\", Guid = \"{Guid}\", IsBuzzed = \"{IsBuzzed}\", ScoreOverride = \"{ScoreOverride}\"";
        }
    }
}