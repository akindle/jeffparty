using Jeffparty.Interfaces;
using Microsoft.AspNetCore.SignalR.Client;

namespace Jeffparty.Web.Services;

public sealed class GameHubService : IAsyncDisposable
{
    private readonly HubConnection _connection;
    private Guid _playerId;

    public GameState CurrentState { get; private set; }
    public List<ContestantViewModel> Contestants { get; } = new();
    public bool IsConnected => _connection.State == HubConnectionState.Connected;
    public bool IsKicked { get; private set; }

    public event Action? StateChanged;

    public GameHubService(string baseAddress)
    {
        var hubUrl = new Uri(new Uri(baseAddress), "chatHub").ToString();
        _connection = new HubConnectionBuilder()
            .WithUrl(hubUrl)
            .WithAutomaticReconnect()
            .Build();

        _connection.On<GameState>(nameof(IMessageSpoke.UpdateGameState), state =>
        {
            Console.WriteLine($"[GameHub] UpdateGameState received: CanBuzzIn={state.CanBuzzIn}, ShouldShowQuestion={state.ShouldShowQuestion}, TimeRemaining={state.QuestionTimeRemainingSeconds:F1}");
            CurrentState = state;
            SyncContestants(state);
            StateChanged?.Invoke();
        });

        _connection.On(nameof(IMessageSpoke.OnConnected), () =>
        {
            if (_playerId != Guid.Empty)
            {
                _connection.InvokeAsync<bool>(
                    nameof(IMessageHub.NotifyPlayerJoined), _playerId, PlayerName ?? "");
            }
        });

        _connection.On<ContestantViewModel>(nameof(IMessageSpoke.NotifyPlayerJoined), joiner =>
        {
            if (Contestants.All(c => c.Guid != joiner.Guid))
            {
                Contestants.Add(joiner);
            }
            StateChanged?.Invoke();
        });

        _connection.On<Guid, string>(nameof(IMessageSpoke.FindOrCreatePlayerData), (guid, name) =>
        {
            if (guid == _playerId)
            {
                var contestant = new ContestantViewModel { Guid = guid, PlayerName = name };
                _connection.InvokeAsync<bool>(nameof(IMessageHub.FoundJoiningPlayer), contestant);
            }
        });

        _connection.On<Guid, double>(nameof(IMessageSpoke.NotifyPlayerBuzzed), (guid, time) =>
        {
            Console.WriteLine($"[GameHub] NotifyPlayerBuzzed: {guid}, time={time}");
            StateChanged?.Invoke();
        });

        _connection.On<Guid, int>(nameof(IMessageSpoke.NotifyPlayerWagered), (guid, wager) =>
        {
            StateChanged?.Invoke();
        });

        _connection.On<Guid, string>(nameof(IMessageSpoke.NotifyFinalJeopardyAnswer), (guid, answer) =>
        {
            StateChanged?.Invoke();
        });

        _connection.On<AudioClips>(nameof(IMessageSpoke.DoPlayAudio), clip =>
        {
            // Audio not supported in browser client for now
        });

        _connection.On<Guid>(nameof(IMessageSpoke.NotifyPlayerKicked), async kickedId =>
        {
            if (kickedId == _playerId)
            {
                IsKicked = true;
                await _connection.StopAsync();
                StateChanged?.Invoke();
            }
        });
    }

    public string? PlayerName { get; private set; }

    public async Task ConnectAsync(string playerName)
    {
        _playerId = Guid.NewGuid();
        PlayerName = playerName;
        Console.WriteLine($"[GameHub] Connecting to hub... PlayerId={_playerId}");
        await _connection.StartAsync();
        Console.WriteLine($"[GameHub] Connected! State={_connection.State}");
        await _connection.InvokeAsync<bool>(
            nameof(IMessageHub.NotifyPlayerJoined), _playerId, playerName);
        Console.WriteLine($"[GameHub] NotifyPlayerJoined sent for '{playerName}'");
    }

    public async Task BuzzIn()
    {
        if (IsConnected)
        {
            await _connection.InvokeAsync<bool>(
                nameof(IMessageHub.BuzzIn), _playerId, CurrentState.QuestionTimeRemainingSeconds);
        }
    }

    public async Task SubmitWager(int wager)
    {
        if (IsConnected)
        {
            await _connection.InvokeAsync<bool>(
                nameof(IMessageHub.SubmitWager), _playerId, wager);
        }
    }

    public async Task SubmitFinalJeopardyAnswer(string answer)
    {
        if (IsConnected)
        {
            await _connection.InvokeAsync<bool>(
                nameof(IMessageHub.SubmitFinalJeopardyAnswer), _playerId, answer);
        }
    }

    public string? GetBuzzedInPlayerName()
    {
        if (CurrentState.BuzzedInPlayerId == Guid.Empty) return null;
        return Contestants.FirstOrDefault(c => c.Guid == CurrentState.BuzzedInPlayerId)?.PlayerName;
    }

    public bool IsMe(Guid guid) => guid == _playerId;

    private void SyncContestants(GameState state)
    {
        if (state.Contestants == null) return;
        foreach (var sc in state.Contestants)
        {
            var existing = Contestants.FirstOrDefault(c => c.Guid == sc.Guid);
            if (existing != null)
            {
                existing.Score = sc.Score;
                existing.PlayerName = sc.Name;
                existing.IsBuzzed = sc.IsBuzzedIn;
            }
            else
            {
                Contestants.Add(new ContestantViewModel
                {
                    Guid = sc.Guid,
                    PlayerName = sc.Name,
                    Score = sc.Score,
                    IsBuzzed = sc.IsBuzzedIn
                });
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync();
    }
}
