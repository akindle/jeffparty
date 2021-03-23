using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Jeffparty.Client.Commands;
using Jeffparty.Interfaces;

namespace Jeffparty.Client
{
    public class GameManager : Notifier
    {
        public string FinalJeopardyCategory { get; set; }

        public bool IsFinalJeopardy { get; set; }

        public bool IsDoubleJeopardy { get; set; }

        public Guid PlayerWithDailyDouble { get; set; }

        public string CurrentQuestion { get; set; }

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

        public GameState GameState => SnapshotGameState();

        public GameManager(IMessageHub server, HostViewModel hostViewModel, ContestantsViewModel contestants)
        {
            ContestantsViewModel = contestants;
            Server = server;
            HostViewModel = hostViewModel;
            PauseForAnswerCommand = new PauseForAnswer(this);
            AskQuestionCommand = new AskQuestion(this);
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
            CurrentQuestion = "No active question";
        }

        public GameState SnapshotGameState()
        {
            return new GameState
            {
                CurrentQuestion = CurrentQuestion,
                Categories = HostViewModel.Categories.Select(cat => new Category
                {
                    CategoryTitle = cat.CategoryHeader,
                    AvailableQuestions = cat.CategoryQuestions.Select(q => !q.IsAsked).ToList()
                }).ToList(),
                Contestants = ContestantsViewModel.Contestants.Select(contestant => new Contestant
                    {Name = contestant.PlayerName, Score = contestant.Score, Guid = contestant.Guid}).ToList(),
                AnswerTimeRemaining = AnswerTimeRemaining,
                QuestionTimeRemaining = QuestionTimeRemaining,
                CanBuzzIn = CanBuzzIn,
                PlayerWithDailyDouble = PlayerWithDailyDouble,
                IsDoubleJeopardy = IsDoubleJeopardy,
                IsFinalJeopardy = IsFinalJeopardy,
                FinalJeopardyCategory = FinalJeopardyCategory
            };
        }

        private void AnswerTimer_Tick(object? sender, EventArgs e)
        {
            AnswerTimeRemaining = AnswerTimeRemaining.Subtract(DateTime.Now.Subtract(LastAnswerFiring));
            LastAnswerFiring = DateTime.Now;
            if (AnswerTimeRemaining.TotalSeconds <= 0)
            {
                AnswerTimer.Stop();
                AnswerTimeRemaining = default;
            }

            HostViewModel.AnswerTimeRemaining = AnswerTimeRemaining;
        }

        private void QuestionTimer_Tick(object? sender, EventArgs e)
        {
            PauseForAnswerCommand.NotifyExecutabilityChanged();
            QuestionTimeRemaining = QuestionTimeRemaining.Subtract(DateTime.Now.Subtract(LastQuestionFiring));
            LastQuestionFiring = DateTime.Now;
            if (QuestionTimeRemaining.TotalSeconds <= 0)
            {
                QuestionTimer.Stop();
                CanBuzzIn = true;
                QuestionTimeRemaining = default;
                AskQuestionCommand.NotifyExecutabilityChanged();
            }
            else
            {
                CanBuzzIn = false;
            }

            HostViewModel.QuestionTimeRemaining = QuestionTimeRemaining;
        }

        public async Task PropagateGameState()
        {
            await Server.PropagateGameState(GameState);
        }
    }
}