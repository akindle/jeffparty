using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace Jeffparty.Client
{
    /// <summary>
    ///     Interaction logic for PlayerView.xaml
    /// </summary>
    public partial class PlayerView : UserControl
    {
        public PlayerView()
        {
            InitializeComponent();
        }
    }

    public class PlayerViewModel : Notifier
    {
        public string ActiveQuestion
        {
            get;
            set;
        }

        public List<PlayerCategoryViewModel> GameboardCategories
        {
            get;
            set;
        }

        public PlayerViewModel()
        {
            GameboardCategories = new List<PlayerCategoryViewModel>
            {
                new PlayerCategoryViewModel("Placeholder Category Title that is long", 200),
                new PlayerCategoryViewModel("Placeholder", 200),
                new PlayerCategoryViewModel("Placeholder", 200),
                new PlayerCategoryViewModel("Placeholder", 200),
                new PlayerCategoryViewModel("Placeholder", 200),
                new PlayerCategoryViewModel("Placeholder", 200)
            };

            ActiveQuestion = "No question selected";
        }

        public void UpdateGameboard(GameboardState state)
        {
            for (var i = 0; i < 6; i++)
            for (var j = 0; j < 5; j++)
            {
                GameboardCategories[i].CategoryValues[j].Available = state.available[i, j];
            }
        }
    }

    public class PlayerCategoryViewModel : Notifier
    {
        public string CategoryTitle
        {
            get;
            set;
        }

        public List<CategoryValueViewModel> CategoryValues
        {
            get;
            set;
        }

        public PlayerCategoryViewModel(string title, uint baseValue)
        {
            CategoryTitle = title;
            CategoryValues = Enumerable.Range(1, 5).Select(a => new CategoryValueViewModel((uint)(a * baseValue)))
                .ToList();
        }
    }

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

    public readonly struct GameboardState
    {
        public readonly bool[,] available;

        public GameboardState(bool[,] available)
        {
            this.available = available;
        }
    }
}