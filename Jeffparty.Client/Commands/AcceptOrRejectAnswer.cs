using System;
using System.Diagnostics;
using System.Windows.Threading;
using Jeffparty.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jeffparty.Client.Commands
{
    public class AcceptOrRejectAnswer : CommandBase
    {
        private readonly GameManager _game;
        private ILogger<AcceptOrRejectAnswer> _logger;

        public AcceptOrRejectAnswer(GameManager game, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<AcceptOrRejectAnswer>();
            _game = game;
        }

        public override bool CanExecute(object? parameter)
        {
            return _game.BuzzedInPlayer != null ||
                   (_game.PlayerWithDailyDouble != Guid.Empty && _game.BuzzedInPlayer?.Wager != null);
        }

        public override async void Execute(object? parameter)
        {
            _logger.Trace();
            if (parameter is string str && bool.TryParse(str, out var b))
            {
                _logger.LogDebug($"Accept/Reject: {b}");
                await _game.Server.RequestPlayAnswerAudio(b);
                if (_game.BuzzedInPlayer == null)
                {
                    _logger.LogError($"Accepting/rejecting a question without a buzzed in player!");
                }
                // handle accept/reject daily double
                if (_game.CurrentQuestion.IsDailyDouble)
                {
                    _logger.LogDebug($"Handling daily double for {_game.BuzzedInPlayer.PlayerName}");
                    var wager = _game.BuzzedInPlayer.Wager;
                    await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                    {
                        if (b)
                        {
                            _game.BuzzedInPlayer.Score += _game.BuzzedInPlayer.Wager ?? 1000;
                            // correct DD
                        }
                        else
                        {
                            _game.BuzzedInPlayer.Score -= _game.BuzzedInPlayer.Wager ?? 1000;
                            // incorrect DD
                        }

                        _game.BuzzedInPlayer.Wager = null;
                        _game.PlayerWithDailyDouble = Guid.Empty;
                    });
                    
                    await _game.AdvanceState();
                }
                else
                {
                    // handle accept/reject normal question
                    await Dispatcher.CurrentDispatcher.InvokeAsync(async () =>
                    {
                        if (b)
                        {
                            _game.BuzzedInPlayer.Score += _game.CurrentQuestion.PointValue;
                            _game.BuzzedInPlayer.IsBuzzed = false;
                            _game.LastCorrectPlayer = _game.BuzzedInPlayer;
                            await _game.AdvanceState();
                        }
                        else
                        {
                            _game.BuzzedInPlayer.Score -= _game.CurrentQuestion.PointValue;
                            _game.BuzzedInPlayer.IsBuzzed = false;
                            _game.BuzzedInPlayer = null;
                            _game.CanBuzzIn = true;
                            _game.LastQuestionFiring = DateTime.Now;
                            _game.QuestionTimer.Start();
                            await _game.PropagateGameState();
                        }
                    });
                }
            }
        }
    }
}