using System;
using System.Collections.Generic;

namespace Jeffparty.Interfaces
{
    public class GameState : Notifier
    {
        public string CurrentQuestion
        {
            get;
            set;
        }

        public List<PlayerCategoryViewModel> Categories
        {
            get;
            set;
        }

        public ContestantsViewModel Contestants
        {
            get;
            set;
        }

        public TimeSpan AnswerTimeRemaining
        {
            get;
            set;
        }

        public TimeSpan QuestionTimeRemaining
        {
            get;
            set;
        }

        public bool CanBuzzIn
        {
            get;
            set;
        }

        public Guid PlayerWithDailyDouble
        {
            get;
            set;
        }

        public bool IsFinalJeopardy
        {
            get;
            set;
        }

        public string? FinalJeopardyCategory
        {
            get;
            set;
        }
    }
}