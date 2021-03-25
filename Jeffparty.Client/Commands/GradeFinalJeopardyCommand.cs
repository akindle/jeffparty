using System.Collections.Generic;
using Jeffparty.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jeffparty.Client.Commands
{
    public class GradeFinalJeopardyCommand : CommandBase
    {
        private readonly bool _isGradingYes;
        private readonly ILogger _logger;

        private static bool _hasGraded;
        private static readonly List<GradeFinalJeopardyCommand> _graders = new List<GradeFinalJeopardyCommand>();

        public static void Reset()
        {
            _hasGraded = false;
            foreach (var gradeFinalJeopardyCommand in _graders)
            {
                gradeFinalJeopardyCommand.NotifyExecutabilityChanged();
            }
        }

        public GradeFinalJeopardyCommand(bool isGradingYes)
        {
            _logger = MainWindow.LogFactory.CreateLogger<GradeFinalJeopardyCommand>();
            _isGradingYes = isGradingYes;
            _graders.Add(this);
        }

        public override bool CanExecute(object? parameter)
        {
            _logger.Trace();
            _logger.LogDebug($"CanExecute parameter {parameter}");
            if (parameter is ContestantViewModel contestant)
            {
                return contestant.FinalJeopardyAnswer != null && !_hasGraded;
            }
            
            return false;
        }

        public override void Execute(object? parameter)
        {
            _logger.Trace();
            _logger.LogDebug($"Execute parameter {parameter}");
            _hasGraded = true;
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
            }

            foreach (var g in _graders)
            {
                g.NotifyExecutabilityChanged();
            }
        }
    }
}