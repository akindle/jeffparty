using Jeffparty.Interfaces;

namespace Jeffparty.Client.Commands
{
    public class AdminSetScore : CommandBase
    {
        private GameManager _game;

        public AdminSetScore(GameManager game)
        {
            _game = game;
        }
        
        public override bool CanExecute(object? parameter)
        {
            return true;
        }

        public override async void Execute(object? parameter)
        {
            if (parameter is ContestantViewModel contestant)
            {
                contestant.Score = contestant.ScoreOverride;
                await _game.PropagateGameState();
            }
        }
    }
}