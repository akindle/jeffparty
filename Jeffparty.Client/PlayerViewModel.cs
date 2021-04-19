using System;
using System.Collections.Generic;
using System.Linq;
using Jeffparty.Client.Commands;
using Jeffparty.Interfaces;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Jeffparty.Client
{
    public class PlayerViewModel : Notifier
    {
        private readonly ContestantsViewModel _contestantsViewModel;
        private readonly ILogger _logger;
        private string? _buzzedInPlayer;
        private string _finalJeopardyAnswer;
        private bool _isFinalJeopardy;
        private bool _isWagerVisible;
        private uint? _wager;

        public PlayerViewModel(PersistedSettings settings, IMessageHub Server,
            ContestantsViewModel contestantsViewModel)
        {
            _logger = MainWindow.LogFactory.CreateLogger<PlayerViewModel>();
            _finalJeopardyAnswer = string.Empty;
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
            SubmitFinalJeopardy = new SubmitFinalJeopardy(this, Server);

            PropertyChanged += (sender, args) =>
            {
                _logger.Trace($"{sender}: PropertyName: \"{args.PropertyName}\"");
            };
        }

        public string ActiveQuestion { get; set; }

        public List<PlayerCategoryViewModel> GameboardCategories { get; set; }

        public TimeSpan QuestionTimeRemaining { get; set; }

        public bool IsWagerVisible
        {
            get => _isWagerVisible;
            set
            {
                if (_isWagerVisible != value)
                {
                    _isWagerVisible = value;
                    if (value) Wager = null;

                    OnPropertyChanged();
                    OnPropertyChanged(nameof(MaximumWager));
                }
            }
        }

        public bool IsFinalJeopardy
        {
            get => _isFinalJeopardy;
            set
            {
                if (_isFinalJeopardy != value)
                {
                    _isFinalJeopardy = value;
                    if (value) Wager = null;

                    OnPropertyChanged();
                }
            }
        }

        public string FinalJeopardyCategory { get; set; }

        public PersistedSettings Settings { get; set; }

        public BuzzIn BuzzInCommand { get; }

        public bool CanBuzzIn { get; set; }

        public bool IsBuzzedIn { get; set; }

        public SubmitWager SubmitWager { get; set; }

        public SubmitFinalJeopardy SubmitFinalJeopardy { get; set; }

        public uint? Wager
        {
            get => _wager;
            set
            {
                SubmitWager.NotifyExecutabilityChanged();
                _wager = value;
            }
        }

        public bool IsQuestionVisible { get; set; }

        public string? BuzzedInPlayer
        {
            get => _buzzedInPlayer;
            set
            {
                _buzzedInPlayer = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(BuzzedInPlayerDisplayString));
            }
        }

        public bool IsDoubleJeopardy { get; set; }

        public ContestantViewModel? Self =>
            _contestantsViewModel.Contestants.FirstOrDefault(c => c.Guid == Settings.Guid);

        public string BuzzedInPlayerDisplayString => BuzzedInPlayer == null
            ? ""
            : $"{BuzzedInPlayer} buzzed in!";

        public string FinalJeopardyAnswer
        {
            get => _finalJeopardyAnswer;
            set
            {
                _finalJeopardyAnswer = value;
                SubmitFinalJeopardy.NotifyExecutabilityChanged();
            }
        }

        public string BoardControllerText
        {
            get;
            set;
        } = "Unknown";

        public uint MaximumWager => (uint)(Math.Abs(Self?.Score ?? 0) + 2000);

        public void Update(GameState newState)
        {
            _logger.Trace(newState.ToString());
            _logger.Trace($"Update start: {this}");
            BoardControllerText = string.IsNullOrEmpty(newState.BoardController) ? "Unknown" : newState.BoardController;
            ActiveQuestion = $"{newState.QuestionCategory}: {newState.CurrentQuestion}";
            QuestionTimeRemaining = TimeSpan.FromSeconds(newState.QuestionTimeRemainingSeconds);

            BuzzedInPlayer =
                _contestantsViewModel.Contestants
                    .FirstOrDefault(contestant => contestant.Guid == newState.BuzzedInPlayerId)?.PlayerName;

            IsWagerVisible = newState.PlayerWithDailyDouble == Settings.Guid || newState.IsFinalJeopardy;
            IsQuestionVisible = newState.ShouldShowQuestion;
            IsDoubleJeopardy = newState.IsDoubleJeopardy;
            IsFinalJeopardy = newState.IsFinalJeopardy;

            FinalJeopardyCategory = newState.FinalJeopardyCategory ?? string.Empty;
            var newCategories = new List<PlayerCategoryViewModel>();
            foreach (var (target, source) in GameboardCategories.Zip(newState.Categories))
            {
                target.CategoryTitle = source.CategoryTitle;
                target.IsActiveCategory = target.CategoryTitle == newState.QuestionCategory;
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
                    target.IsBuzzed = source.IsBuzzedIn;
                }
            }

            IsBuzzedIn = Settings.Guid == newState.BuzzedInPlayerId;
            GameboardCategories = newCategories;
            BuzzInCommand.CanBuzzIn = newState.CanBuzzIn;
            CanBuzzIn = newState.CanBuzzIn;
            _logger.Trace($"Update end: {this}");
        }

        public override string ToString()
        {
            return $"ActiveQuestion = \"{ActiveQuestion}\", GameboardCategories = \"{GameboardCategories}\", QuestionTimeRemaining = \"{QuestionTimeRemaining}\", IsWagerVisible = \"{IsWagerVisible}\", IsFinalJeopardy = \"{IsFinalJeopardy}\", FinalJeopardyCategory = \"{FinalJeopardyCategory}\", Settings = \"{Settings}\", BuzzInCommand = \"{BuzzInCommand}\", CanBuzzIn = \"{CanBuzzIn}\", IsBuzzedIn = \"{IsBuzzedIn}\", SubmitWager = \"{SubmitWager}\", SubmitFinalJeopardy = \"{SubmitFinalJeopardy}\", Wager = \"{Wager}\", IsQuestionVisible = \"{IsQuestionVisible}\", BuzzedInPlayer = \"{BuzzedInPlayer}\", IsDoubleJeopardy = \"{IsDoubleJeopardy}\", Self = \"{Self}\", BuzzedInPlayerDisplayString = \"{BuzzedInPlayerDisplayString}\", FinalJeopardyAnswer = \"{FinalJeopardyAnswer}\"";
        }
    }
}