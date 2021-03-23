using System;
using System.Collections.Generic;
using Jeffparty.Interfaces;

namespace Jeffparty.Client
{
    public class HostViewModel : Notifier
    {
        public TimeSpan AnswerTimeRemaining { get; set; }

        public TimeSpan QuestionTimeRemaining { get; set; }

        public List<CategoryViewModel> Categories { get; set; }

        public GameManager GameManager { get; }

        public HostViewModel(IMessageHub server, ContestantsViewModel contestants)
        {
            Categories = new List<CategoryViewModel>
            {
                CategoryViewModel.CreateRandom(
                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ??
                throw new InvalidOperationException(),
                CategoryViewModel.CreateRandom(
                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ??
                throw new InvalidOperationException(),
                CategoryViewModel.CreateRandom(
                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ??
                throw new InvalidOperationException(),
                CategoryViewModel.CreateRandom(
                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ??
                throw new InvalidOperationException(),
                CategoryViewModel.CreateRandom(
                    @"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ??
                throw new InvalidOperationException(),
                CategoryViewModel.CreateRandom(@"C:\Users\AlexKindle\source\repos\TurdFerguson\venv\categories") ??
                throw new InvalidOperationException()
            };

            GameManager = new GameManager(server, this, contestants);
        }
    }
}