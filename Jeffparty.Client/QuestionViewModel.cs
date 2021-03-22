namespace Jeffparty.Client
{
    public class QuestionViewModel : Notifier
    {
        public string AnswerText
        {
            get;
            set;
        } = string.Empty;

        public bool IsAsked
        {
            get;
            set;
        }

        public bool IsDailyDouble
        {
            get;
            set;
        }

        public uint PointValue
        {
            get;
            set;
        }

        public string QuestionText
        {
            get;
            set;
        } = string.Empty;
    }
}