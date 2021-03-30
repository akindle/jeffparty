using Jeffparty.Interfaces;

namespace Jeffparty.Client.Commands
{
    public sealed class SubmitWager : CommandBase
    {
        private readonly PlayerViewModel _playerView;
        private readonly IMessageHub _server;
        private bool _hasWagered;

        public SubmitWager(PlayerViewModel playerView, IMessageHub server)
        {
            _playerView = playerView;
            _server = server;
            playerView.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(_playerView.IsWagerVisible) || args.PropertyName == nameof(_playerView.IsFinalJeopardy))
                {
                    _hasWagered = false;
                }
            };
        }
        
        public override bool CanExecute(object? parameter)
        {
            return _playerView.IsWagerVisible && !_hasWagered && _playerView.Wager != null;
        }

        public override async void Execute(object? parameter)
        {
            if (_playerView.Wager == null)
            {
                return;
            }
            
            _hasWagered = true;
            _playerView.IsQuestionVisible = true;
            await _server.SubmitWager(_playerView.Settings.Guid, (int)_playerView.Wager);
            NotifyExecutabilityChanged();
        }
    }
}