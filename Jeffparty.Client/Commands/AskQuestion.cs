using System;
using System.Linq;

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
            return !game.QuestionTimer.IsEnabled && game.PlayerWithDailyDouble == Guid.Empty;
        }

        public override async void Execute(object? parameter)
        {
            if (parameter is QuestionViewModel question)
            {
                question.IsAsked = true;
                game.CurrentQuestion = question;

                if (question.IsDailyDouble)
                {
                    game.PlayerWithDailyDouble = game.LastCorrectPlayer?.Guid ?? Guid.Empty;
                    game.BuzzedInPlayer =
                        game.ContestantsViewModel.Contestants.FirstOrDefault(c => c.Guid == game.PlayerWithDailyDouble);
                }
                else
                {
                    game.QuestionTimeRemaining = TimeSpan.FromSeconds(15);
                    game.LastQuestionFiring = DateTime.Now;
                    game.QuestionTimer.Start();
                }
            }
            
            await game.PropagateGameState().ConfigureAwait(true);
            NotifyExecutabilityChanged();
        }
    }
}