namespace Jeffparty.Client
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

        public ContestantViewModel()
        {
            PlayerName = "Player 1";
        }
    }
}