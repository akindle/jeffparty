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
                if (question.IsDailyDouble && game.LastCorrectPlayer != null && game.LastCorrectPlayer.Guid != Guid.Empty)
                {
                    await game.Server.RequestPlayAudio(AudioClips.Airhorn);
                    game.PlayerWithDailyDouble = game.LastCorrectPlayer.Guid;
                    game.BuzzedInPlayer = game.LastCorrectPlayer;
                    game.ShouldShowQuestion = false;
                    game.LikelyCurrentGameState = GameManager.GameStates.Wagering;
                }
                else
                {
                    game.ShouldShowQuestion = true;
                    game.LikelyCurrentGameState = GameManager.GameStates.ReadingQuestion;
                }
                
                question.IsAsked = true;
                game.CurrentQuestion = question;
            }
            
            await game.PropagateGameState().ConfigureAwait(true);
            NotifyExecutabilityChanged();
            game.ReplaceCategory.NotifyExecutabilityChanged();
            game.ListenForAnswersCommand.NotifyExecutabilityChanged();
        }
    }
}