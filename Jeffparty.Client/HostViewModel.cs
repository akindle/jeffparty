using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Jeffparty.Client.Commands;
using Jeffparty.Interfaces;
using System.Linq;

namespace Jeffparty.Client
{
    public class HostViewModel : Notifier
    {
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

        public List<CategoryViewModel> Categories
        {
            get;
            set;
        }

        public GameManager GameManager
        {
            get;
        }

        public HostViewModel(IMessageHub server)
        {
            Categories = new List<CategoryViewModel>
            {
                CategoryViewModel.CreateRandom(
                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ?? throw new InvalidOperationException(),
                CategoryViewModel.CreateRandom(
                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ?? throw new InvalidOperationException(),
                CategoryViewModel.CreateRandom(
                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ?? throw new InvalidOperationException(),
                CategoryViewModel.CreateRandom(
                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ?? throw new InvalidOperationException(),
                CategoryViewModel.CreateRandom(
                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ?? throw new InvalidOperationException(),
                CategoryViewModel.CreateRandom(@"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ?? throw new InvalidOperationException()
            };

            GameManager = new GameManager(server, this);
        }
    }

    public class GameManager : Notifier
    {
        public IMessageHub Server
        {
            get;
        }

        public GameManager(IMessageHub server, HostViewModel hostViewModel)
        {
            this.Server = server;
            PauseForAnswerCommand = new PauseForAnswer(this);
            AskQuestionCommand = new AskQuestion(this);
            GameState = new GameState { Categories = hostViewModel.Categories.Select(category => new PlayerCategoryViewModel(category.CategoryHeader, 200)).ToList() };
            GameState.PropertyChanged += (sender, e) =>
            {
                //server.PropagateGameState(GameState);
                switch (e.PropertyName)
                {
                    case nameof(GameState.QuestionTimeRemaining):
                        hostViewModel.QuestionTimeRemaining = GameState.QuestionTimeRemaining;
                        break;
                    case nameof(GameState.AnswerTimeRemaining):
                        hostViewModel.AnswerTimeRemaining = GameState.AnswerTimeRemaining;
                        break;
                    default:
                        return;
                }
            };
            hostViewModel.PropertyChanged += (sender, e) =>
            {
                switch (e.PropertyName)
                {
                    case nameof(hostViewModel.Categories):
                        GameState.Categories = hostViewModel.Categories.Select(category => new PlayerCategoryViewModel(category.CategoryHeader, 200)).ToList();
                        break;
                    default:
                        return;
                }
            };
//            PropertyChanged += (_, __) => server.PropagateGameState(GameState);
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
        }

        public DispatcherTimer AnswerTimer
        {
            get;
        }

        public DispatcherTimer QuestionTimer
        {
            get;
        }
        

        public DateTime LastQuestionFiring
        {
            get;
            set;
        }

        public DateTime LastAnswerFiring
        {
            get;
            set;
        }
        private void AnswerTimer_Tick(object? sender, EventArgs e)
        {
            GameState.AnswerTimeRemaining = GameState.AnswerTimeRemaining.Subtract(DateTime.Now.Subtract(LastAnswerFiring));
            LastAnswerFiring = DateTime.Now;
            if (GameState.AnswerTimeRemaining.TotalSeconds <= 0)
            {
                AnswerTimer.Stop();
                GameState.AnswerTimeRemaining = default;
            }
        }

        private void QuestionTimer_Tick(object? sender, EventArgs e)
        {
            PauseForAnswerCommand.NotifyExecutabilityChanged();
            GameState.QuestionTimeRemaining = GameState.QuestionTimeRemaining.Subtract(DateTime.Now.Subtract(LastQuestionFiring));
            LastQuestionFiring = DateTime.Now;
            if (GameState.QuestionTimeRemaining.TotalSeconds <= 0)
            {
                QuestionTimer.Stop();
                GameState.CanBuzzIn = true;
                GameState.QuestionTimeRemaining = default;
                AskQuestionCommand.NotifyExecutabilityChanged();
            }
            else
            {
                GameState.CanBuzzIn = false;
            }
        }

        public PauseForAnswer PauseForAnswerCommand{get;}
        public AskQuestion AskQuestionCommand{get;}

        public GameState GameState{get;}
    }
}