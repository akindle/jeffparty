﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Markup;
using System.Windows.Threading;
using Jeffparty.Client.Commands;
using Jeffparty.Interfaces;

namespace Jeffparty.Client
{
    public class GameManager : Notifier
    {
        private ContestantViewModel? _lastCorrectPlayer;
        public string FinalJeopardyCategory { get; set; }

        public bool IsFinalJeopardy { get; set; }

        public bool IsDoubleJeopardy { get; set; }

        public Guid PlayerWithDailyDouble { get; set; }

        public QuestionViewModel CurrentQuestion { get; set; }

        public IMessageHub Server { get; }

        public HostViewModel HostViewModel { get; }

        public ContestantsViewModel ContestantsViewModel { get; }

        public DispatcherTimer AnswerTimer { get; }

        public DispatcherTimer QuestionTimer { get; }

        public DateTime LastQuestionFiring { get; set; }

        public DateTime LastAnswerFiring { get; set; }

        public TimeSpan AnswerTimeRemaining { get; set; }

        public TimeSpan QuestionTimeRemaining { get; set; }

        public bool CanBuzzIn { get; set; }

        public PauseForAnswer PauseForAnswerCommand { get; }

        public AskQuestion AskQuestionCommand { get; }
        
        public AcceptOrRejectAnswer AnswerCommand { get; }
        
        public ContestantViewModel? BuzzedInPlayer { get; set; }

        public ContestantViewModel? LastCorrectPlayer
        {
            get
            {
                return _lastCorrectPlayer ??= ContestantsViewModel.Contestants.FirstOrDefault();
            }
            set => _lastCorrectPlayer = value;
        }


        public GameState GameState => SnapshotGameState();

        public GameManager(IMessageHub server, HostViewModel hostViewModel, ContestantsViewModel contestants)
        {
            ContestantsViewModel = contestants;
            Server = server;
            HostViewModel = hostViewModel;
            PauseForAnswerCommand = new PauseForAnswer(this);
            AskQuestionCommand = new AskQuestion(this);
            AnswerCommand = new AcceptOrRejectAnswer(this);
            AnswerTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.1)
            };
            AnswerTimer.Tick += AnswerTimer_Tick;
            QuestionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(0.1)
            };
            QuestionTimer.Tick += QuestionTimer_Tick;
            LastQuestionFiring = DateTime.Now;
            LastAnswerFiring = DateTime.Now;

            FinalJeopardyCategory = string.Empty;
            CurrentQuestion = new QuestionViewModel();
            

            var (category, question) = GenerateDailyDouble();

            hostViewModel.Categories[category].CategoryQuestions[question].IsDailyDouble = true;
        }

        public GameState SnapshotGameState()
        {
            return new GameState
            {
                CurrentQuestion = CurrentQuestion.QuestionText,
                Categories = HostViewModel.Categories.Select(cat => new Category
                {
                    CategoryTitle = cat.CategoryHeader,
                    AvailableQuestions = cat.CategoryQuestions.Select(q => !q.IsAsked).ToList()
                }).ToList(),
                Contestants = ContestantsViewModel.Contestants.Select(contestant => new Contestant
                    {Name = contestant.PlayerName, Score = contestant.Score, Guid = contestant.Guid}).ToList(),
                AnswerTimeRemainingSeconds = AnswerTimeRemaining.TotalSeconds,
                QuestionTimeRemainingSeconds = QuestionTimeRemaining.TotalSeconds,
                CanBuzzIn = CanBuzzIn,
                PlayerWithDailyDouble = PlayerWithDailyDouble,
                IsDoubleJeopardy = IsDoubleJeopardy,
                IsFinalJeopardy = IsFinalJeopardy,
                FinalJeopardyCategory = FinalJeopardyCategory,
                BuzzedInPlayerId = BuzzedInPlayer?.Guid ?? Guid.Empty
            };
        }

        private async void AnswerTimer_Tick(object? sender, EventArgs e)
        {
            AnswerTimeRemaining = AnswerTimeRemaining.Subtract(DateTime.Now.Subtract(LastAnswerFiring));
            LastAnswerFiring = DateTime.Now;
            if (AnswerTimeRemaining.TotalSeconds <= 0)
            {
                await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    if (BuzzedInPlayer != null) BuzzedInPlayer.Score -= CurrentQuestion.PointValue;
                });
                BuzzedInPlayer = null;
                AnswerTimer.Stop();
                AnswerTimeRemaining = default;
                QuestionTimer.Start();
                CanBuzzIn = true;
                PauseForAnswerCommand.NotifyExecutabilityChanged();
            }

            await Dispatcher.CurrentDispatcher.InvokeAsync(()=>
            HostViewModel.AnswerTimeRemaining = AnswerTimeRemaining);
            await PropagateGameState();
        }

        private async void QuestionTimer_Tick(object? sender, EventArgs e)
        {
            PauseForAnswerCommand.NotifyExecutabilityChanged();
            QuestionTimeRemaining = QuestionTimeRemaining.Subtract(DateTime.Now.Subtract(LastQuestionFiring));
            LastQuestionFiring = DateTime.Now;
            if (QuestionTimeRemaining.TotalSeconds <= 0)
            {
                QuestionTimer.Stop();
                CanBuzzIn = false;
                QuestionTimeRemaining = default;
                AskQuestionCommand.NotifyExecutabilityChanged();
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
            await Server.PropagateGameState(GameState);
        }

        public async Task PlayerBuzzed(ContestantViewModel buzzingPlayer, double timerSecondsAtBuzz)
        {
            CanBuzzIn = false;
            QuestionTimer.Stop();
            AnswerTimeRemaining = TimeSpan.FromSeconds(5);
            LastAnswerFiring = DateTime.Now;
            AnswerTimer.Start();
            await PropagateGameState().ConfigureAwait(true);
            PauseForAnswerCommand.NotifyExecutabilityChanged();
            BuzzedInPlayer = buzzingPlayer;
            AnswerCommand.NotifyExecutabilityChanged();
        }

        private static (int Category, int Question) GenerateDailyDouble()
        {
            var rand = new Random();
            var dailyDoubleCategory = rand.Next(0, 6);
            var dailyDoubleQuestion = rand.NextDouble() switch
            {
                var val when val <0.15 => 1,
                var val when val <(0.15 + 0.2) => 2,
                var val when val <(0.15 + 0.2 + .35) => 3,
                _ => 4
            };

            return (dailyDoubleCategory, dailyDoubleQuestion);
        }
        
        public async Task AdvanceState()
        {
            BuzzedInPlayer = null;
            QuestionTimer.Stop();
            AnswerTimer.Stop();
            QuestionTimeRemaining = default;
            AnswerTimeRemaining = default;
            await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
            {
                if (HostViewModel.Categories.All(model => model.CategoryQuestions.All(question => question.IsAsked)))
                {
                    if (!IsDoubleJeopardy)
                    {
                        IsDoubleJeopardy = true;
                        HostViewModel.Categories = new List<CategoryViewModel>
                            {
                                CategoryViewModel.CreateRandom(
                                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom(
                                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom(
                                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom(
                                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom(
                                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ??
                                throw new InvalidOperationException(),
                                CategoryViewModel.CreateRandom(
                                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ??
                                throw new InvalidOperationException()
                            }.Select(cvm => new CategoryViewModel
                            {
                                CategoryQuestions = cvm.CategoryQuestions.Select(q => new QuestionViewModel
                                    {
                                        AnswerText = q.AnswerText,
                                        IsAsked = false,
                                        IsDailyDouble = false, // TODO
                                        PointValue = q.PointValue * 2,
                                        QuestionText = q.QuestionText
                                    })
                                    .ToList()
                            })
                            .ToList();
                        var assignedDailyDoubles = 0;
                        while (assignedDailyDoubles < 2)
                        {
                            var (candidateCategory, candidateQuestion) = GenerateDailyDouble();
                            var candidate = HostViewModel.Categories[candidateCategory]
                                .CategoryQuestions[candidateQuestion];
                            if (!candidate.IsDailyDouble)
                            {
                                candidate.IsDailyDouble = true;
                                assignedDailyDoubles++;
                            }
                        }
                    }
                    else
                    {
                        IsFinalJeopardy = true;
                        // TODO
                    }
                }
            });
            
            AskQuestionCommand.NotifyExecutabilityChanged();
            PauseForAnswerCommand.NotifyExecutabilityChanged();
            AnswerCommand.NotifyExecutabilityChanged();
            await PropagateGameState();
        }

        public async Task PlayerWagered(ContestantViewModel contestantViewModel, int playerViewWager)
        {
            contestantViewModel.Wager = playerViewWager;
            
            AskQuestionCommand.NotifyExecutabilityChanged();
            PauseForAnswerCommand.NotifyExecutabilityChanged();
            AnswerCommand.NotifyExecutabilityChanged();
            await PropagateGameState();
        }
    }
}