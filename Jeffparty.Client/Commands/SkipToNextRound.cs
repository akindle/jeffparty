using System.Threading.Tasks;

namespace Jeffparty.Client.Commands
{
    public class SkipToNextRound : CommandBase
    {
        private readonly GameManager _gameManager;

        public SkipToNextRound(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public override bool CanExecute(object? parameter) => true;

        public override async void Execute(object? parameter)
        {
            GameManager.GameModes target;
            if (_gameManager.IsFinalJeopardy)
            {
                target = GameManager.GameModes.Jeff;
            }
            else if (_gameManager.IsDoubleJeopardy || _gameManager.IsLightningRound)
            {
                target = GameManager.GameModes.FinalJeff;
            }
            else
            {
                target = GameManager.GameModes.DoubleJeff;
            }

            await _gameManager.AdvanceState(target);
        }
    }
}
