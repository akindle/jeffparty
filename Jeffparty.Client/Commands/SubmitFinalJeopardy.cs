using System;
using System.Threading.Tasks;
using Jeffparty.Interfaces;

namespace Jeffparty.Client.Commands
{
    public class SubmitFinalJeopardy : CommandBase
    {
        private readonly PlayerViewModel _player;
        private readonly IMessageHub _messageHub;

        public SubmitFinalJeopardy(PlayerViewModel player, IMessageHub messageHub)
        {
            _player = player;
            _messageHub = messageHub;
        }
        public override bool CanExecute(object? parameter)
        {
            return true;
        }

        public override async void Execute(object? parameter)
        {
            await _messageHub.SubmitFinalJeopardyAnswer(_player.Settings.Guid, _player.FinalJeopardyAnswer);
        }
    }
}