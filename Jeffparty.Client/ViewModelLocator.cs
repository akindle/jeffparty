using System;
using System.Collections.Generic;

namespace Jeffparty.Client
{
    internal static class ViewModelLocator
    {
        private static PlayerViewModel? playerView;

        private static HostViewModel? hostView;

        private static ContestantsViewModel? contestantView;

        public static ContestantsViewModel ContestantsView
        {
            get
            {
                return contestantView ??= new ContestantsViewModel();
            }
        }

        public static PlayerViewModel PlayerView
        {
            get
            {
                if (playerView == null)
                {
                    playerView = new PlayerViewModel();
                }

                return playerView;
            }
        }

        public static HostViewModel HostView
        {
            get
            {
                return hostView ??= new HostViewModel
                {
                    Categories = new List<CategoryViewModel>
                    {
                        CategoryViewModel.CreateRandom(
                            @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ?? throw new InvalidOperationException(),
                        CategoryViewModel.CreateRandom(
                            @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ?? throw new InvalidOperationException(),
                        CategoryViewModel.CreateRandom(
                            @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ?? throw new InvalidOperationException(),
                        CategoryViewModel.CreateRandom(
                            @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ?? throw new InvalidOperationException(),
                        CategoryViewModel.CreateRandom(
                            @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ?? throw new InvalidOperationException(),
                        CategoryViewModel.CreateRandom(@"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ?? throw new InvalidOperationException()
                    }
                };
            }
        }
    }
}