using System.Collections.Generic;
using System.Linq;

namespace Jeffparty.Interfaces
{
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
        
        public bool IsActiveCategory { get; set; }

        public PlayerCategoryViewModel(string title, int baseValue)
        {
            CategoryTitle = title;
            CategoryValues = Enumerable.Range(1, 5).Select(a => new CategoryValueViewModel((int)(a * baseValue)))
                .ToList();
        }
    }
}