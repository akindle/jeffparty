namespace Jeffparty.Client.Commands
{
    public class SwapFinalJeopardy : CommandBase
    {
        private readonly GameManager _gameManager;

        public SwapFinalJeopardy(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public override bool CanExecute(object? parameter) => !_gameManager.IsFinalJeopardy;

        public override void Execute(object? parameter)
        {
            _gameManager.LoadRandomFinalJeopardy();
        }
    }
}
