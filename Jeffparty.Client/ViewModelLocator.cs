using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jeffparty.Interfaces;

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
                return playerView ??= new PlayerViewModel(new PersistedSettings(Guid.Empty, "New Player", "http://localhost/"));
            }
        }

        public static HostViewModel HostView
        {
            get
            {
                return hostView ??= new HostViewModel(new MockHub(), new ContestantsViewModel())
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

        private class MockHub : IMessageHub
        {
            public Task<bool> SendMessage(string user, string message)
            {
                return Task.FromResult(false);
            }

            public Task<int> PropagateGameState(GameState state)
            {
                return Task.FromResult(5);
            }

            public Task<bool> NotifyPlayerJoined(Guid joiner, string playerName)
            {
                return Task.FromResult(false);
            }

            public Task<bool> FoundJoiningPlayer(ContestantViewModel contestant)
            {
                return Task.FromResult(false);
            }

            public Task<bool> BuzzIn()
            {
                return Task.FromResult(false);
            }
        }
    }
}