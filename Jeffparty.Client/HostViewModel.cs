using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Jeffparty.Client.Commands;
using Jeffparty.Interfaces;

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

        public List<CategoryViewModel>? Categories
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
            GameManager = new GameManager(server);
        }
    }

    public class GameManager : Notifier
    {
        IMessageHub server;
        public GameManager(IMessageHub server)
        {
            this.server = server;
            PauseForAnswerCommand = new PauseForAnswer(this);
            AskQuestionCommand = new AskQuestion(this);
            GameState = new GameState();
            PropertyChanged += (_, __) => server.PropagateGameState(GameState);
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
                GameState.QuestionTimeRemaining = default;
                AskQuestionCommand.NotifyExecutabilityChanged();
            }
        }

        public PauseForAnswer PauseForAnswerCommand{get;}
        public AskQuestion AskQuestionCommand{get;}

        public GameState GameState{get;}
    }
}