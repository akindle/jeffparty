using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Jeffparty.Interfaces
{
    public sealed class ContestantsViewModel : Notifier
    {
        public ObservableCollection<ContestantViewModel> Contestants
        {
            get;
            set;
        }

        public ContestantsViewModel()
        {
            Contestants = new ObservableCollection<ContestantViewModel>
            {
                new ContestantViewModel { PlayerName = "Player 1", Score = -12000 },
                new ContestantViewModel { PlayerName = "Player 2", Score = 4600 },
                new ContestantViewModel { PlayerName = "Player 3", Score = 18000 }
            };
        }
    }
}