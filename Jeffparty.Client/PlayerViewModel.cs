using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Jeffparty.Client.Commands;
using Jeffparty.Interfaces;

namespace Jeffparty.Client
{
    public class PlayerViewModel : Notifier
    {
        private readonly ContestantsViewModel _contestantsViewModel;

        public string ActiveQuestion { get; set; }

        public List<PlayerCategoryViewModel> GameboardCategories { get; set; }

        public TimeSpan QuestionTimeRemaining { get; set; }

        public bool IsWagerVisible { get; set; }

        public bool IsFinalJeopardy { get; set; }
        public string FinalJeopardyCategory { get; set; }

        public PersistedSettings Settings { get; set; }

        public BuzzIn BuzzInCommand { get; }

        public bool IsBuzzedIn { get; set; }

        public SubmitWager SubmitWager { get; set; }

        public uint Wager { get; set; }

        public PlayerViewModel(PersistedSettings settings, IMessageHub Server,
            ContestantsViewModel contestantsViewModel)
        {
            _contestantsViewModel = contestantsViewModel;
            Settings = settings;

            GameboardCategories = new List<PlayerCategoryViewModel>
            {
                new("Placeholder", 200),
                new("Placeholder", 200),
                new("Placeholder", 200),
                new("Placeholder", 200),
                new("Placeholder", 200),
                new("Placeholder", 200)
            };

            ActiveQuestion = "No question selected";
            FinalJeopardyCategory = string.Empty;
            BuzzInCommand = new BuzzIn(settings.Guid, Server);
            SubmitWager = new SubmitWager(this, Server);
        }

        public void Update(GameState newState)
        {
            ActiveQuestion = newState.CurrentQuestion;
            QuestionTimeRemaining = TimeSpan.FromSeconds(newState.QuestionTimeRemainingSeconds);
            IsWagerVisible = newState.PlayerWithDailyDouble == Settings.Guid || newState.IsFinalJeopardy;
            IsFinalJeopardy = newState.IsFinalJeopardy;
            FinalJeopardyCategory = newState.FinalJeopardyCategory ?? string.Empty;
            var newCategories = new List<PlayerCategoryViewModel>();
            foreach (var (target, source) in GameboardCategories.Zip(newState.Categories))
            {
                target.CategoryTitle = source.CategoryTitle;
                var i = 1;
                foreach (var (tq, sq) in target.CategoryValues.Zip(source.AvailableQuestions))
                {
                    tq.Available = sq;
                    tq.Value = (newState.IsDoubleJeopardy ? 400 : 200) * i;
                    i++;
                }

                newCategories.Add(target);
            }

            foreach (var source in newState.Contestants)
            {
                if (_contestantsViewModel.Contestants.FirstOrDefault(contestant => contestant.Guid == source.Guid) is
                    { } target)
                {
                    target.Score = source.Score;
                    target.PlayerName = source.Name;
                }
            }

            IsBuzzedIn = Settings.Guid == newState.BuzzedInPlayerId;
            GameboardCategories = newCategories;
            BuzzInCommand.CanBuzzIn = newState.CanBuzzIn;
        }
    }
}