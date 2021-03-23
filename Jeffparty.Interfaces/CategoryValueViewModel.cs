namespace Jeffparty.Interfaces
{
    public class CategoryValueViewModel : Notifier
    {
        public bool Available
        {
            get;
            set;
        }

        public uint Value
        {
            get;
            set;
        }

        public CategoryValueViewModel(uint value)
        {
            Value = value;
            Available = true;
        }
    }
}