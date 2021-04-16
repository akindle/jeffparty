using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using Jeffparty.Client.Commands;
using Jeffparty.Interfaces;

namespace Jeffparty.Client
{
    public class GameManager : Notifier
    {
        private ContestantViewModel? _lastCorrectPlayer;
        private ILogger<GameManager> _logger;
        public string FinalJeopardyCategory { get; set; }
        public string FinalJeopardyQuestion { get; set; }
        public string FinalJeopardyAnswer { get; set; }

        public bool IsFinalJeopardy { get; set; }

        public bool IsDoubleJeopardy { get; set; }

        public Guid PlayerWithDailyDouble { get; set; }

        public QuestionViewModel CurrentQuestion { get; set; }

        public IMessageHub Server { get; }

        public HostViewModel HostViewModel { get; }

        public ContestantsViewModel ContestantsViewModel { get; }

        public DispatcherTimer QuestionTimer { get; }

        public DateTime LastQuestionFiring { get; set; }

        public DateTime LastAnswerFiring { get; set; }

        public TimeSpan QuestionTimeRemaining { get; set; }

        public bool CanBuzzIn { get; set; }

        public AskQuestion AskQuestionCommand { get; }

        public AcceptOrRejectAnswer AnswerCommand { get; }

        public ContestantViewModel? BuzzedInPlayer { get; set; }

        public ContestantViewModel? LastCorrectPlayer
        {
            get { return _lastCorrectPlayer ??= ContestantsViewModel.Contestants.FirstOrDefault(); }
            set => _lastCorrectPlayer = value;
        }


        public ReplaceCategory ReplaceCategory { get; }

        public GameManager(IMessageHub server, HostViewModel hostViewModel, ContestantsViewModel contestants,
            ILoggerFactory loggerFactory)
        {
            FinalJeopardyCategory = string.Empty;
            FinalJeopardyQuestion = string.Empty;
            FinalJeopardyAnswer = string.Empty;
            _logger = loggerFactory.CreateLogger<GameManager>();
            ContestantsViewModel = contestants;
            Server = server;
            HostViewModel = hostViewModel;
            AskQuestionCommand = new AskQuestion(this);
            AnswerCommand = new AcceptOrRejectAnswer(this, loggerFactory);
            ReplaceCategory = new ReplaceCategory(this);
            ListenForAnswersCommand = new ListenForAnswers(this);
            QuestionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.1)
            };
            QuestionTimer.Tick += QuestionTimer_Tick;
            LastQuestionFiring = DateTime.Now;
            LastAnswerFiring = DateTime.Now;

            CurrentQuestion = new QuestionViewModel();


            var (category, question) = GenerateDailyDouble();

            hostViewModel.Categories[category].CategoryQuestions[question].IsDailyDouble = true;
        }

        public GameState SnapshotGameState()
        {
            _logger.Trace();
            return new GameState
            {
                CurrentQuestion = IsFinalJeopardy ? FinalJeopardyQuestion : CurrentQuestion.QuestionText,
                QuestionCategory =  IsFinalJeopardy ? FinalJeopardyCategory : HostViewModel.Categories.FirstOrDefault(category => category.CategoryQuestions.Any(question => question == CurrentQuestion))?.CategoryHeader ?? string.Empty,
                Categories = HostViewModel.Categories.Select(cat => new Category
                {
                    CategoryTitle = cat.CategoryHeader,
                    AvailableQuestions = cat.CategoryQuestions.Select(q => !q.IsAsked).ToList()
                }).ToList(),
                Contestants = ContestantsViewModel.Contestants.Select(contestant => new Contestant
                    {Name = contestant.PlayerName, Score = contestant.Score, Guid = contestant.Guid, IsBuzzedIn = contestant.IsBuzzed}).ToList(),
                QuestionTimeRemainingSeconds = QuestionTimeRemaining.TotalSeconds,
                CanBuzzIn = CanBuzzIn,
                PlayerWithDailyDouble = PlayerWithDailyDouble,
                IsDoubleJeopardy = IsDoubleJeopardy,
                IsFinalJeopardy = IsFinalJeopardy,
                FinalJeopardyCategory = FinalJeopardyCategory,
                BuzzedInPlayerId = BuzzedInPlayer?.Guid ?? Guid.Empty,
                ShouldShowQuestion = ShouldShowQuestion
            };
        }

        private async void QuestionTimer_Tick(object? sender, EventArgs e)
        {
            _logger.Trace();
            QuestionTimeRemaining = QuestionTimeRemaining.Subtract(DateTime.Now.Subtract(LastQuestionFiring));
            LastQuestionFiring = DateTime.Now;
            if (QuestionTimeRemaining.TotalSeconds <= 0)
            {
                _logger.LogDebug("Question timer expired");
                CanBuzzIn = false;
                await Server.RequestPlayAudio(AudioClips.Timeout);
                await AdvanceState();
            }
            else
            {
                CanBuzzIn = true;
            }

            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                HostViewModel.QuestionTimeRemaining = QuestionTimeRemaining);
            await PropagateGameState();
        }

        public async Task PropagateGameState()
        {
            var state = SnapshotGameState();
            _logger.Trace(state.ToString());
            await Server.PropagateGameState(state);
        }

        public async Task PlayerBuzzed(ContestantViewModel buzzingPlayer, double timerSecondsAtBuzz)
        {
            _logger.Trace(buzzingPlayer.ToString());
            if (AnswerCommand.CanExecute(null))
            {
                return;
            }
            
            CanBuzzIn = false;
            BuzzedInPlayer = buzzingPlayer;
            foreach (var player in ContestantsViewModel.Contestants)
            {
                player.IsBuzzed = false;
            }
            
            BuzzedInPlayer.IsBuzzed = true;
            QuestionTimer.Stop();
            await PropagateGameState().ConfigureAwait(true);
            AnswerCommand.NotifyExecutabilityChanged();
        }

        private static (int Category, int Question) GenerateDailyDouble()
        {
            var rand = new Random();
            var dailyDoubleCategory = rand.Next(0, 6);
            var dailyDoubleQuestion = rand.NextDouble() switch
            {
                var val when val < 0.15 => 1,
                var val when val < (0.15 + 0.2) => 2,
                var val when val < (0.15 + 0.2 + .35) => 3,
                _ => 4
            };

            return (dailyDoubleCategory, dailyDoubleQuestion);
        }

        public enum GameStates
        {
            Jeff,
            DoubleJeff,
            FinalJeff
        }

        public async Task AdvanceState(GameStates? forceState=null)
        {
            _logger.Trace();
            BuzzedInPlayer = null;
            QuestionTimer.Stop();
            ShouldShowQuestion = false;
            QuestionTimeRemaining = default;
            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                GameStates? targetState = forceState;
                if (forceState != null)
                {
                    targetState = forceState;
                }
                else if(HostViewModel.Categories.All(model => model.CategoryQuestions.All(question => question.IsAsked)))
                {
                    if (!IsDoubleJeopardy && !IsFinalJeopardy)
                    {
                        targetState = GameStates.DoubleJeff;
                    }
                    else if (IsDoubleJeopardy && !IsFinalJeopardy)
                    {
                        targetState = GameStates.FinalJeff;
                    }
                    else
                    {
                        targetState = GameStates.Jeff;
                    }
                }
                
                if(targetState != null)
                {
                    if (targetState == GameStates.DoubleJeff)
                    {
                        _logger.LogDebug("Advancing to double jeopardy");
                        IsDoubleJeopardy = true;
                        HostViewModel.Categories = new List<CategoryViewModel>
                            {
                                CategoryViewModel.CreateRandom() ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom() ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom() ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom() ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom() ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom() ??
                                throw new InvalidOperationException()
                            }.Select(cvm => new CategoryViewModel
                            {
                                CategoryHeader =  cvm.CategoryHeader,
                                CategoryQuestions = cvm.CategoryQuestions.Select(q => new QuestionViewModel
                                    {
                                        AnswerText = q.AnswerText,
                                        IsAsked = false,
                                        IsDailyDouble = false,
                                        PointValue = q.PointValue * 2,
                                        QuestionText = q.QuestionText
                                    })
                                    .ToList()
                            })
                            .ToList();
                        var assignedDailyDoubles = 0;
                        var lastCategory = -1;
                        while (assignedDailyDoubles < 2)
                        {
                            var (candidateCategory, candidateQuestion) = GenerateDailyDouble();
                            var candidate = HostViewModel.Categories[candidateCategory]
                                .CategoryQuestions[candidateQuestion];
                            if (!candidate.IsDailyDouble && lastCategory != candidateCategory)
                            {
                                lastCategory = candidateCategory;
                                candidate.IsDailyDouble = true;
                                assignedDailyDoubles++;
                            }
                        }
                    }
                    else if (targetState == GameStates.FinalJeff)
                    {
                        _logger.LogDebug("Advancing to final jeopardy");
                        IsFinalJeopardy = true;
                        ShouldShowQuestion = false;
                        var finaljs = Directory
                            .EnumerateFiles(@"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories")
                            .Where(fileName => fileName.Contains("finalj")).ToList();
                        var random = new Random();
                        var finalj = finaljs[random.Next(finaljs.Count)];
                        var fj = File.ReadAllLines(finalj);
                        FinalJeopardyCategory = CategoryViewModel.CleanUpString(fj[0]);
                        FinalJeopardyQuestion = CategoryViewModel.CleanUpString(fj[1]);
                        FinalJeopardyAnswer = CategoryViewModel.CleanUpString(fj[2]);
                        CurrentQuestion = new QuestionViewModel
                        {
                            AnswerText = FinalJeopardyAnswer, IsAsked = false, IsDailyDouble = false, PointValue = 0,
                            QuestionText = $"{FinalJeopardyCategory}: {FinalJeopardyQuestion}"
                        };
                        // TODO
                    }
                    else if(targetState == GameStates.Jeff)
                    {
                        _logger.LogDebug("Advancing to new game");
                        HostViewModel.Categories = new List<CategoryViewModel>
                            {
                                CategoryViewModel.CreateRandom() ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom() ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom() ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom() ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom() ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom() ??
                                throw new InvalidOperationException()
                            }.Select(cvm => new CategoryViewModel
                            {
                                CategoryQuestions = cvm.CategoryQuestions.Select(q => new QuestionViewModel
                                    {
                                        AnswerText = q.AnswerText,
                                        IsAsked = false,
                                        IsDailyDouble = false,
                                        PointValue = q.PointValue * 2,
                                        QuestionText = q.QuestionText
                                    })
                                    .ToList()
                            })
                            .ToList();

                        var (category, question) = GenerateDailyDouble();

                        HostViewModel.Categories[category].CategoryQuestions[question].IsDailyDouble = true;
                    }
                }
            });

            AskQuestionCommand.NotifyExecutabilityChanged();
            AnswerCommand.NotifyExecutabilityChanged();
            ListenForAnswersCommand.NotifyExecutabilityChanged();
            await PropagateGameState();
        }

        public bool ShouldShowQuestion { get; set; }
        public ListenForAnswers ListenForAnswersCommand { get; set; }

        public async Task PlayerWagered(ContestantViewModel contestantViewModel, int playerViewWager)
        {
            _logger.Trace();
            contestantViewModel.Wager = playerViewWager;

            if (IsFinalJeopardy)
            {
                ShouldShowQuestion = ContestantsViewModel.Contestants.Where(contestant => contestant.Score > 0)
                    .All(contestant => contestant.Wager != null);
            }
            else
            {
                ShouldShowQuestion = true;
            }

            AskQuestionCommand.NotifyExecutabilityChanged();
            AnswerCommand.NotifyExecutabilityChanged();
            await PropagateGameState();
        }
    }
}