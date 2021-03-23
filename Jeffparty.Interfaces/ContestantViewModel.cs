using System;

namespace Jeffparty.Interfaces
{
    public sealed class ContestantViewModel : Notifier
    {
        public string PlayerName
        {
            get;
            set;
        }

        public int Score
        {
            get;
            set;
        }

        public Guid Guid
        {
            get;set;
        }

        public ContestantViewModel()
        {
            PlayerName = "Player 1";
        }
    }
}