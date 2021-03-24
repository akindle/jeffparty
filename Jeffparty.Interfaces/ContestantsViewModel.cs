using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jeffparty.Interfaces
{
    public sealed class ContestantsViewModel : Notifier
    {
        private bool _showWagerColumn;

        public ObservableCollection<ContestantViewModel> Contestants
        {
            get;
            set;
        }

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

        public ContestantsViewModel()
        {
            Contestants = new ObservableCollection<ContestantViewModel>();
        }
    }
}