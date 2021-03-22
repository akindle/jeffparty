namespace Jeffparty.Client.Commands
{
    public class PauseForAnswer : CommandBase
    {
        private readonly HostViewModel host;

        public PauseForAnswer(HostViewModel host)
        {
            this.host = host;
        }

        public override bool CanExecute(object? parameter)
        {
            return host.AnswerTimer.IsEnabled;
        }

        public override void Execute(object? parameter)
        {
            host.AnswerTimer.Stop();
            NotifyExecutabilityChanged();
        }
    }
}