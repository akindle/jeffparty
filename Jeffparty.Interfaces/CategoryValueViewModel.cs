namespace Jeffparty.Interfaces
{
    public class CategoryValueViewModel : Notifier
    {
        public bool Available
        {
            get;
            set;
        }

        public int Value
        {
            get;
            set;
        }

        public CategoryValueViewModel(int value)
        {
            Value = value;
            Available = true;
        }
    }
}