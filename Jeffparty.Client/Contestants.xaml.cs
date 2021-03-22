using System.Windows.Controls;

namespace Jeffparty.Client
{
    /// <summary>
    ///     Interaction logic for Contestant.xaml
    /// </summary>
    public partial class Contestants : UserControl
    {
        public Contestants()
        {
            DataContext = ViewModelLocator.ContestantsView;
            InitializeComponent();
        }
    }
}