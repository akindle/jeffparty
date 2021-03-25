using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Jeffparty.Client.Commands;
using Jeffparty.Interfaces;
using Microsoft.Extensions.Logging;

namespace Jeffparty.Client
{
    public sealed class ContestantsViewModel : Notifier
    {
        private bool _showWagerColumn;
        private ILogger _logger;

        public ObservableCollection<ContestantViewModel> Contestants { get; set; }

        public int WagerColumnWidth => ShowWagerColumn ? 150 : 0;

        public bool ShowWagerColumn
        {
            get => _showWagerColumn;
            set
            {
                _showWagerColumn = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(WagerColumnWidth));
            }
        }
        
        public GradeFinalJeopardyCommand CorrectFinalJeopardy { get; }
        public GradeFinalJeopardyCommand IncorrectFinalJeopardy { get; }

        public ContestantsViewModel()
        {
            _logger = MainWindow.LogFactory.CreateLogger<ContestantsViewModel>();
            CorrectFinalJeopardy = new GradeFinalJeopardyCommand(true);
            IncorrectFinalJeopardy = new GradeFinalJeopardyCommand(false);
            Contestants = new ObservableCollection<ContestantViewModel>();
            Contestants.CollectionChanged += ContestantsOnCollectionChanged;
        }

        private void ContestantsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _logger.Trace();
            if (e.NewItems != null)
            {
                foreach (var contestant in e.NewItems.OfType<ContestantViewModel>())
                {
                    contestant.PropertyChanged += ContestantOnPropertyChanged;
                }
            }

            if (e.OldItems != null)
            {
                foreach (var contestant in e.OldItems.OfType<ContestantViewModel>())
                {
                    contestant.PropertyChanged -= ContestantOnPropertyChanged;
                }
            }
        }

        private void ContestantOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _logger.Trace();
            if (e.PropertyName == nameof(Contestant.Score))
            {
                GradeFinalJeopardyCommand.Reset();
            }
            
            CorrectFinalJeopardy.NotifyExecutabilityChanged();
            IncorrectFinalJeopardy.NotifyExecutabilityChanged();
        }
    }
}