using System;
using Jeffparty.Interfaces;

namespace Jeffparty.Client.Commands
{
    public class BuzzIn : CommandBase
    {
        private bool _canBuzzIn;
        private Guid id;
        private readonly IMessageHub _messageHub;

        public BuzzIn(Guid playerId, IMessageHub messageHub)
        {
            id = playerId;
            _messageHub = messageHub;
        }

        public override void Execute(object? parameter)
        {
            if (parameter is PlayerViewModel playerViewModel)
            {
                _messageHub.BuzzIn(id, playerViewModel.QuestionTimeRemaining.TotalSeconds);
            }
        }

        public override bool CanExecute(object? parameter)
        {
            return CanBuzzIn;
        }

        public bool CanBuzzIn
        {
            get => _canBuzzIn;
            set
            {
                if (_canBuzzIn != value)
                {
                    _canBuzzIn = value;
                    NotifyExecutabilityChanged();
                }
            }
        }
    }
}