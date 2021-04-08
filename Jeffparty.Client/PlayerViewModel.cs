using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using Jeffparty.Client.Commands;
using Jeffparty.Interfaces;

namespace Jeffparty.Client
{
    public class WagerValidator : ValidationRule
    {
        private readonly PlayerViewModel _player;

        public WagerValidator(PlayerViewModel player)
        {
            _player = player;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is int i || (value is string str && int.TryParse(str, out i)))
            {
                if (i < 0)
                {
                    return new ValidationResult(false, "Wager is below 0");
                }

                if (_player.IsDoubleJeopardy)
                {
                    if (i < Math.Max(2000, _player.Self?.Score ?? 0))
                    {
                        return ValidationResult.ValidResult;
                    }

                    return new ValidationResult(false, "Wager too high");
                }

                if (_player.IsFinalJeopardy)
                {
                    if (i < Math.Max(0, _player.Self?.Score ?? 0))
                    {
                        return ValidationResult.ValidResult;
                    }

                    return new ValidationResult(false, "Wager too high");
                }

                if (i < Math.Max(1000, _player.Self?.Score ?? 0))
                {
                    return ValidationResult.ValidResult;
                }

                return new ValidationResult(false, "Wager too high");
            }

            return new ValidationResult(false, "Not a number");
        }
    }

    public class PlayerViewModel : Notifier
    {
        private readonly ContestantsViewModel _contestantsViewModel;
        private uint? _wager;
        private string? _buzzedInPlayer;
        private string _finalJeopardyAnswer;
        private bool _isWagerVisible;
        private bool _isFinalJeopardy;

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
                    if (value)
                    {
                        Wager = null;
                    }

                    OnPropertyChanged();
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
                    if (value)
                    {
                        Wager = null;
                    }

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
                if (value == null)
                {
                    throw new ArgumentException("Wager missing");
                }
                
                if (IsDoubleJeopardy && value >= Math.Max(2000, Self?.Score ?? 0))
                {
                    throw new ArgumentException("Wager too high");
                }
                
                if (IsFinalJeopardy && value >= Math.Max(0, Self?.Score ?? 0))
                {
                    throw new ArgumentException("Wager too high");
                }
                
                if (!IsDoubleJeopardy && !IsFinalJeopardy && value >= Math.Max(1000, Self?.Score ?? 0))
                {
                    throw new ArgumentException("Wager too high");
                }

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

        public PlayerViewModel(PersistedSettings settings, IMessageHub Server,
            ContestantsViewModel contestantsViewModel)
        {
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
        }

        public void Update(GameState newState)
        {
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
        }
    }
}