using System;
using System.Threading.Tasks;
using System.Windows.Threading;
using Jeffparty.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jeffparty.Client.Commands
{
    public class SubmitFinalJeopardy : CommandBase
    {
        private readonly PlayerViewModel _player;
        private readonly IMessageHub _messageHub;
        private readonly ILogger _logger;
        private bool _hasSubmitted;
        
        public SubmitFinalJeopardy(PlayerViewModel player, IMessageHub messageHub)
        {
            _logger = MainWindow.LogFactory.CreateLogger<SubmitFinalJeopardy>();
            _player = player;
            _messageHub = messageHub;
            player.PropertyChanged += (_, args) =>
            {
                if (args.PropertyName == nameof(player.IsFinalJeopardy))
                {
                    _hasSubmitted = false;
                }
            };
        }
        
        public override bool CanExecute(object? parameter)
        {
            _logger.Trace();
            return !string.IsNullOrWhiteSpace(_player.FinalJeopardyAnswer) && !_hasSubmitted;
        }

        public override async void Execute(object? parameter)
        {
            _logger.Trace();
            _logger.LogDebug($"Submitting {_player.FinalJeopardyAnswer} for {_player.Settings.Guid}");
            _hasSubmitted = true;
            await _messageHub.SubmitFinalJeopardyAnswer(_player.Settings.Guid, _player.FinalJeopardyAnswer);
            await Dispatcher.CurrentDispatcher.InvokeAsync(() => _player.FinalJeopardyAnswer = string.Empty);
            NotifyExecutabilityChanged();
        }
    }
}