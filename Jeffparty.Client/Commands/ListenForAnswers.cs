﻿using System;

namespace Jeffparty.Client.Commands
{
    public class ListenForAnswers : CommandBase
    {
        private readonly GameManager _game;

        public ListenForAnswers(GameManager game)
        {
            _game = game;
        }

        public override bool CanExecute(object? parameter)
        {
            return _game.ShouldShowQuestion;
        }

        public override void Execute(object? parameter)
        {
            _game.QuestionTimeRemaining = TimeSpan.FromSeconds(7.5);
            _game.LastQuestionFiring = DateTime.Now;
            _game.LikelyCurrentGameState = GameManager.GameStates.WaitingForBuzzes;
            _game.EnableBuzzProcessing();
            _game.QuestionTimer.Start();
        }
    }
}