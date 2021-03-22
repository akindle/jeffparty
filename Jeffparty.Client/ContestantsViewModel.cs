using System.Collections.Generic;

namespace Jeffparty.Client
{
    public sealed class ContestantsViewModel : Notifier
    {
        public List<ContestantViewModel> Contestants
        {
            get;
            set;
        }

        public ContestantsViewModel()
        {
            Contestants = new List<ContestantViewModel>
            {
                new ContestantViewModel { PlayerName = "Player 1", Score = -12000 },
                new ContestantViewModel { PlayerName = "Player 2", Score = 4600 },
                new ContestantViewModel { PlayerName = "Player 3", Score = 18000 }
            };
        }
    }
}