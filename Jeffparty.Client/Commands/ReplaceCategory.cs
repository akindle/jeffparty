using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Jeffparty.Client.Commands
{
    public class ReplaceCategory : CommandBase
    {
        private readonly GameManager _gameManager;

        public ReplaceCategory(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public override bool CanExecute(object? parameter)
        {
            if (parameter is CategoryViewModel categoryViewModel)
            {
                return categoryViewModel.CategoryQuestions.All(q => !q.IsAsked);
            }

            return false;
        }

        public override async void Execute(object? parameter)
        {
            if (parameter is CategoryViewModel category)
            {
                await Dispatcher.CurrentDispatcher.InvokeAsync(() =>
                {
                    var questionValues = category.CategoryQuestions.ToList();
                    var tempReplacement = CategoryViewModel.CreateRandom() ?? CategoryViewModel.GenerateNonsense();
                    category.CategoryHeader = tempReplacement.CategoryHeader;
                    category.CategoryQuestions = tempReplacement.CategoryQuestions;
                    foreach (var (source, target) in questionValues.Zip(category.CategoryQuestions))
                    {
                        target.PointValue = source.PointValue;
                        target.IsDailyDouble = source.IsDailyDouble;
                    }
                });
            }

            await _gameManager.PropagateGameState();
        }
    }
}