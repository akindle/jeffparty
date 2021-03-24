using System;
using System.Windows.Threading;
using Jeffparty.Interfaces;

namespace Jeffparty.Client.Commands
{
    public class AcceptOrRejectAnswer : CommandBase
    {
        private readonly GameManager _game;

        public AcceptOrRejectAnswer(GameManager game)
        {
            _game = game;
        }

        public override bool CanExecute(object? parameter)
        {
            return _game.BuzzedInPlayer != null ||
                   (_game.PlayerWithDailyDouble != Guid.Empty && _game.BuzzedInPlayer?.Wager != null);
        }

        public override async void Execute(object? parameter)
        {
            if (parameter is string str && Boolean.TryParse(str, out var b))
            {
                if (b)
                {
                    if (_game.BuzzedInPlayer != null)
                    {
                        await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                        {
                            _game.BuzzedInPlayer.Score += _game.BuzzedInPlayer.Wager ?? _game.CurrentQuestion.PointValue;
                            _game.LastCorrectPlayer = _game.BuzzedInPlayer;
                            _game.BuzzedInPlayer.Wager = null;
                        });
                    }

                    await _game.AdvanceState();
                }
                else
                {
                    if (_game.BuzzedInPlayer != null)
                    {
                        await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                        {
                             _game.BuzzedInPlayer.Score -=
                                    _game.BuzzedInPlayer.Wager ?? _game.CurrentQuestion.PointValue;
                             _game.BuzzedInPlayer.Wager = null;
                        });
                    }

                    
                    _game.BuzzedInPlayer = null;
                    _game.CanBuzzIn = true;
                    _game.QuestionTimer.Start();
                    await _game.PropagateGameState();
                }
            }
        }
    }
}