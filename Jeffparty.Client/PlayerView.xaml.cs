using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Jeffparty.Interfaces;

namespace Jeffparty.Client
{
    /// <summary>
    ///     Interaction logic for PlayerView.xaml
    /// </summary>
    public partial class PlayerView : UserControl
    {
        public PlayerView()
        {
            DataContext = ViewModelLocator.PlayerView;
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debugger.Break();
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
        
        public TimeSpan QuestionTimeRemaining{get;set;}

        public bool IsWagerVisible
        {
            get;
            set;
        }

        public bool IsFinalJeopardy{get;set;}
        public string FinalJeopardyCategory{get;set;}

        public PersistedSettings Settings{get;set;}

        public PlayerViewModel(PersistedSettings settings)
        {
            Settings = settings;

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
            FinalJeopardyCategory = string.Empty;
        }

        public void Update(GameState newState)
        {
            ActiveQuestion = newState.CurrentQuestion;
            GameboardCategories = newState.Categories;
            QuestionTimeRemaining = newState.QuestionTimeRemaining;
            IsWagerVisible = newState.PlayerWithDailyDouble == Settings.Guid || newState.IsFinalJeopardy;
            IsFinalJeopardy = newState.IsFinalJeopardy;
            FinalJeopardyCategory = newState.FinalJeopardyCategory ?? string.Empty;
        }
    }
}