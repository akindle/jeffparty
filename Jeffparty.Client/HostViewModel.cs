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

        public HostViewModel(IMessageHub server, ContestantsViewModel contestants, ILoggerFactory loggerFactory, bool isLightningRound = false)
        {
            var categoryCount = isLightningRound ? 3 : 6;
            var questionsPerCategory = isLightningRound ? 4 : 5;
            try
            {
                Categories = new List<CategoryViewModel>();
                for (int i = 0; i < categoryCount; i++)
                {
                    Categories.Add(CategoryViewModel.CreateRandom(questionsPerCategory) ??
                        CategoryViewModel.GenerateNonsense());
                }
            }
            catch
            {
                Categories = new List<CategoryViewModel>();
                for (int i = 0; i < categoryCount; i++)
                {
                    Categories.Add(CategoryViewModel.GenerateNonsense());
                }
            }

            GameManager = new GameManager(server, this, contestants, loggerFactory, isLightningRound);
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