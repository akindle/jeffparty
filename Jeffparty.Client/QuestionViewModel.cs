using Jeffparty.Interfaces;

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

        public int PointValue
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