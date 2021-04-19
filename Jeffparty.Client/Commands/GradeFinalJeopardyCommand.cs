using System.Collections.Generic;
using Jeffparty.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jeffparty.Client.Commands
{
    public class GradeFinalJeopardyCommand : CommandBase
    {
        private readonly bool _isGradingYes;
        private readonly GameManager _gameManager;
        private readonly ILogger _logger;

        public GradeFinalJeopardyCommand(GameManager gameManager, bool isGradingYes)
        {
            _logger = MainWindow.LogFactory.CreateLogger<GradeFinalJeopardyCommand>();
            _isGradingYes = isGradingYes;
            _gameManager = gameManager;
        }

        public override bool CanExecute(object? parameter)
        {
            _logger.Trace();
            _logger.LogDebug($"CanExecute parameter {parameter}");
            if (parameter is ContestantViewModel contestant)
            {
                return contestant.FinalJeopardyAnswer != null;
            }
            
            return false;
        }

        public override async void Execute(object? parameter)
        {
            _logger.Trace();
            _logger.LogDebug($"Execute parameter {parameter}");
            if (parameter is ContestantViewModel contestant)
            {
                if (_isGradingYes)
                {
                    contestant.Score += contestant.Wager ?? 0;
                }
                else
                {
                    contestant.Score -= contestant.Wager ?? 0;
                }

                await _gameManager.PropagateGameState();
            }
        }
    }
}