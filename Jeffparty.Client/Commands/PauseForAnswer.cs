namespace Jeffparty.Client.Commands
{
    public class PauseForAnswer : CommandBase
    {
        private readonly GameManager game;

        public PauseForAnswer(GameManager game)
        {
            this.game = game;
        }

        public override bool CanExecute(object? parameter)
        {
            return game.AnswerTimer.IsEnabled;
        }

        public override void Execute(object? parameter)
        {
            game.AnswerTimer.Stop();
            NotifyExecutabilityChanged();
        }
    }
}