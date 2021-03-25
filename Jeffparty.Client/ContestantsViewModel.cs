using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Jeffparty.Client.Commands;
using Jeffparty.Interfaces;

namespace Jeffparty.Client
{
    public sealed class ContestantsViewModel : Notifier
    {
        private bool _showWagerColumn;

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
            CorrectFinalJeopardy = new GradeFinalJeopardyCommand(true);
            IncorrectFinalJeopardy = new GradeFinalJeopardyCommand(false);
            Contestants = new ObservableCollection<ContestantViewModel>();
            Contestants.CollectionChanged += ContestantsOnCollectionChanged;
        }

        private void ContestantsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {if (e.NewItems != null)
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
            CorrectFinalJeopardy.NotifyExecutabilityChanged();
            IncorrectFinalJeopardy.NotifyExecutabilityChanged();
        }
    }

    public class GradeFinalJeopardyCommand : CommandBase
    {
        private readonly bool _isGradingYes;

        public GradeFinalJeopardyCommand(bool isGradingYes)
        {
            _isGradingYes = isGradingYes;
        }

        public override bool CanExecute(object? parameter)
        {
            if (parameter is ContestantViewModel contestant)
            {
                return contestant.FinalJeopardyAnswer != null;
            }
            
            return false;
        }

        public override void Execute(object? parameter)
        {
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
        }
    }
}