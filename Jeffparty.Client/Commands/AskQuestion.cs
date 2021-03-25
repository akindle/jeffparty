using System;
using System.Linq;
using Jeffparty.Interfaces;

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
            if (parameter is QuestionViewModel question)
            {
                return !game.QuestionTimer.IsEnabled && game.PlayerWithDailyDouble == Guid.Empty && !question.IsAsked;
            }

            return false;
        }

        public override async void Execute(object? parameter)
        {
            if (parameter is QuestionViewModel question)
            {
                question.IsAsked = true;
                game.CurrentQuestion = question;

                if (question.IsDailyDouble)
                {
                    await game.Server.RequestPlayAudio(AudioClips.Airhorn);
                    game.PlayerWithDailyDouble = game.LastCorrectPlayer?.Guid ?? Guid.Empty;
                    game.BuzzedInPlayer =
                        game.ContestantsViewModel.Contestants.FirstOrDefault(c => c.Guid == game.PlayerWithDailyDouble);
                    game.ShouldShowQuestion = false;
                }
                else
                {
                    game.ShouldShowQuestion = true;
                }
            }
            
            await game.PropagateGameState().ConfigureAwait(true);
            NotifyExecutabilityChanged();
            game.ReplaceCategory.NotifyExecutabilityChanged();
            game.ListenForAnswersCommand.NotifyExecutabilityChanged();
        }
    }
}