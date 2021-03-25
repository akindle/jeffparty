using System;
using System.Collections.Generic;

namespace Jeffparty.Interfaces
{
    public readonly struct Category
    {
        public string CategoryTitle { get; init; }

        public List<bool> AvailableQuestions { get; init; }
    }

    public readonly struct Contestant
    {
        public string Name { get; init; }

        public int Score { get; init; }

        public Guid Guid { get; init; }
    }

    public readonly struct GameState
    {
        public string CurrentQuestion { get; init; }

        public List<Category> Categories { get; init; }

        public List<Contestant> Contestants { get; init; }

        public double QuestionTimeRemainingSeconds { get; init; }

        public bool CanBuzzIn { get; init; }

        public Guid PlayerWithDailyDouble { get; init; }

        public bool IsDoubleJeopardy { get; init; }

        public bool IsFinalJeopardy { get; init; }

        public string? FinalJeopardyCategory { get; init; }
        public Guid BuzzedInPlayerId { get; init; }
        public bool ShouldShowQuestion { get; init; }
    }
}