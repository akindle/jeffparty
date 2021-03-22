using System;
using System.Collections.Generic;
using System.Windows.Threading;
using Jeffparty.Client.Commands;

namespace Jeffparty.Client
{
    public class HostViewModel : Notifier
    {
        public AskQuestion AskQuestionCommand
        {
            get;
            set;
        }

        public PauseForAnswer PauseForAnswerCommand
        {
            get;
            set;
        }

        public TimeSpan AnswerTimeRemaining
        {
            get;
            private set;
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

        public DispatcherTimer AnswerTimer
        {
            get;
        }

        public DispatcherTimer QuestionTimer
        {
            get;
        }

        public DateTime lastQuestionFiring
        {
            get;
            set;
        }

        public DateTime lastAnswerFiring
        {
            get;
            set;
        }

        public HostViewModel()
        {
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
            lastQuestionFiring = DateTime.Now;
            lastAnswerFiring = DateTime.Now;
            AskQuestionCommand = new AskQuestion(this);
            PauseForAnswerCommand = new PauseForAnswer(this);
        }

        private void AnswerTimer_Tick(object? sender, EventArgs e)
        {
            AnswerTimeRemaining = AnswerTimeRemaining.Subtract(DateTime.Now.Subtract(lastAnswerFiring));
            lastAnswerFiring = DateTime.Now;
            if (AnswerTimeRemaining.TotalSeconds <= 0)
            {
                AnswerTimer.Stop();
                AnswerTimeRemaining = default;
            }
        }

        private void QuestionTimer_Tick(object? sender, EventArgs e)
        {
            PauseForAnswerCommand.NotifyExecutabilityChanged();
            QuestionTimeRemaining = QuestionTimeRemaining.Subtract(DateTime.Now.Subtract(lastQuestionFiring));
            lastQuestionFiring = DateTime.Now;
            if (QuestionTimeRemaining.TotalSeconds <= 0)
            {
                QuestionTimer.Stop();
                QuestionTimeRemaining = default;
                AskQuestionCommand.NotifyExecutabilityChanged();
            }
        }
    }
}