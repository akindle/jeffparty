using System;

namespace Jeffparty.Client.Commands
{
    public class AskQuestion : CommandBase
    {
        private readonly GameManager game;

        public AskQuestion(GameManager game)
        {
            this.game = game;
        }

        public override bool CanExecute(object? parameter)
        {
            return !game.QuestionTimer.IsEnabled;
        }

        public override void Execute(object? parameter)
        {
            if (parameter is QuestionViewModel question)
            {
                question.IsAsked = true;
            }

            game.GameState.QuestionTimeRemaining = TimeSpan.FromSeconds(15);
            game.LastQuestionFiring = DateTime.Now;
            game.QuestionTimer.Start();
            NotifyExecutabilityChanged();
        }
    }
}