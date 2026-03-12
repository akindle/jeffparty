using Jeffparty.Interfaces;

namespace Jeffparty.Client.Commands
{
    public class KickPlayer : CommandBase
    {
        private readonly GameManager _gameManager;

        public KickPlayer(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public override bool CanExecute(object? parameter)
        {
            return parameter is ContestantViewModel;
        }

        public override async void Execute(object? parameter)
        {
            if (parameter is ContestantViewModel contestant)
            {
                await _gameManager.Server.KickPlayer(contestant.Guid);
            }
        }
    }
}
