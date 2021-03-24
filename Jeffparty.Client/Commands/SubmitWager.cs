using Jeffparty.Interfaces;

namespace Jeffparty.Client.Commands
{
    public sealed class SubmitWager : CommandBase
    {
        private readonly PlayerViewModel _playerView;
        private readonly IMessageHub _server;
        private bool hasWagered;

        public SubmitWager(PlayerViewModel playerView, IMessageHub server)
        {
            _playerView = playerView;
            _server = server;
            playerView.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(_playerView.IsWagerVisible))
                {
                    NotifyExecutabilityChanged();
                    hasWagered = false;
                }
            };
        }
        
        public override bool CanExecute(object? parameter)
        {
            return _playerView.IsWagerVisible && !hasWagered;
        }

        public override async void Execute(object? parameter)
        {
            hasWagered = true;
            await _server.SubmitWager(_playerView.Settings.Guid, (int)_playerView.Wager);
            NotifyExecutabilityChanged();
        }
    }
}