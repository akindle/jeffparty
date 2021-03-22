using System;

namespace Jeffparty.Client.Commands
{
    public class AskQuestion : CommandBase
    {
        private readonly HostViewModel host;

        public AskQuestion(HostViewModel host)
        {
            this.host = host;
        }

        public override bool CanExecute(object? parameter)
        {
            return !host.QuestionTimer.IsEnabled;
        }

        public override void Execute(object? parameter)
        {
            if (parameter is QuestionViewModel question)
            {
                question.IsAsked = true;
            }

            host.QuestionTimeRemaining = TimeSpan.FromSeconds(15);
            host.lastQuestionFiring = DateTime.Now;
            host.QuestionTimer.Start();
            NotifyExecutabilityChanged();
        }
    }
}