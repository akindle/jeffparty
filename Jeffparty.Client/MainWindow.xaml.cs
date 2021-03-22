using System;
using System.Threading.Tasks;
using System.Windows;
using Jeffparty.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;
using SignalR.Strong;

namespace Jeffparty.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IMessageSpoke
    {HubConnection connection;
        public MainWindow()
        {
            InitializeComponent();

            connection = new HubConnectionBuilder()
                .WithUrl("https://localhost:44391/ChatHub")
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(new Random().Next(0,5) * 1000);
                await connection.StartAsync();
            };
        }

        public async Task ReceiveMessage(string user, string message)
        {
            await Dispatcher.InvokeAsync(() =>
                {
                   var newMessage = $"{user}: {message}";
                });
        }

        public Task UpdateGameState(GameState state)
        {
            throw new NotImplementedException();
        }
    }
}
