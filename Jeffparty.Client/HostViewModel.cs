using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Jeffparty.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jeffparty.Client
{
    public class HostViewModel : Notifier
    {
        public TimeSpan QuestionTimeRemaining { get; set; }

        public List<CategoryViewModel> Categories { get; set; }

        public GameManager GameManager { get; }

        public string BoardController => GameManager.LastCorrectPlayer?.PlayerName ?? "Unknown";

        public HostViewModel(IMessageHub server, ContestantsViewModel contestants, ILoggerFactory loggerFactory)
        {
            try
            {
                Categories = new List<CategoryViewModel>
                {
                    CategoryViewModel.CreateRandom() ??
                    CategoryViewModel.GenerateNonsense(),
                    CategoryViewModel.CreateRandom() ??
                    CategoryViewModel.GenerateNonsense(),
                    CategoryViewModel.CreateRandom() ??
                    CategoryViewModel.GenerateNonsense(),
                    CategoryViewModel.CreateRandom() ??
                    CategoryViewModel.GenerateNonsense(),
                    CategoryViewModel.CreateRandom() ??
                    CategoryViewModel.GenerateNonsense(),
                    CategoryViewModel.CreateRandom() ??
                    CategoryViewModel.GenerateNonsense()
                };
            }
            catch
            {
                Categories = new List<CategoryViewModel>
                {
                    CategoryViewModel.GenerateNonsense(),
                    CategoryViewModel.GenerateNonsense(),
                    CategoryViewModel.GenerateNonsense(),
                    CategoryViewModel.GenerateNonsense(),
                    CategoryViewModel.GenerateNonsense(),
                    CategoryViewModel.GenerateNonsense()
                };
            }

            GameManager = new GameManager(server, this, contestants, loggerFactory);
            GameManager.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(GameManager.LastCorrectPlayer))
                {
                    OnPropertyChanged(nameof(BoardController));
                }
            };

            contestants.Contestants.CollectionChanged += (sender, args) =>
            {
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    OnPropertyChanged(nameof(BoardController));
                }
            };
        }
    }
}