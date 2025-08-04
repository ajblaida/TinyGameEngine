using Microsoft.Identity.Client;
using TinyGameEngine.Controllers;
using TinyGameEngine.Engine.Interfaces;
using TinyGameEngine.Engine.Models;

namespace TinyGameEngine.Engine.Services;

/// <summary>
/// Main game engine implementation with state synchronization
/// </summary>
public abstract class TinyEngine : IGameEngine, IDisposable
{
    private readonly IGameStateService _gameStateService;
    protected readonly ITelemetryService _telemetryService;
    private readonly IGameIdProvider _gameIdProvider;
    protected readonly ILogger<TinyEngine> _logger;
    private readonly Timer _syncTimer;
    private readonly SemaphoreSlim _syncSemaphore = new(1, 1);
    
    private GameState? _currentGameState;
    private bool _isRunning;
    private string? _lastStateHash;
    private bool _disposed;

    public GameState? CurrentGameState => _currentGameState;
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Gets the game mode for this engine.
    /// </summary>
    public abstract string GameMode { get; }

    public TinyEngine(
        IGameStateService gameStateService,
        ITelemetryService telemetryService,
        IGameIdProvider gameIdProvider,
        ILogger<TinyEngine> logger)
    {
        _gameStateService = gameStateService;
        _telemetryService = telemetryService;
        _gameIdProvider = gameIdProvider;
        _logger = logger;

        // Set up periodic sync timer (max once per second)
        _syncTimer = new Timer(SyncStateCallback, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    public async Task<GameState> StartGameAsync(string playerId, CancellationToken cancellationToken = default)
    {
        var gameId = _gameIdProvider.GetGameId(); // Generate a new game ID
        
        try
        {
            _logger.LogInformation("Starting game for player {PlayerId}", playerId);

            // Try to load existing game state
            var existingState = await _gameStateService.LoadGameStateAsync(gameId, cancellationToken);

            // Create new game state
            _currentGameState = new GameState
            {
                GameId = gameId,
                PlayerId = playerId,
                StartTime = DateTimeOffset.UtcNow,
                LastUpdated = DateTimeOffset.UtcNow
            };

            // Save initial state
            await _gameStateService.SaveGameStateAsync(_currentGameState, cancellationToken);
            _logger.LogInformation("Created new game state for {GameId}", gameId);

            _isRunning = true;
            _telemetryService.TrackGameStarted(gameId, playerId);

            return _currentGameState;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game {GameId}", gameId);
            _telemetryService.TrackException(ex, new Dictionary<string, string>
            {
                ["operation"] = "StartGame",
                ["gameId"] = gameId
            });
            throw;
        }
    }

    public async Task UpdateGameStateAsync(UpdateGameRequest updateAction, CancellationToken cancellationToken = default)
    {
        if (_currentGameState == null)
        {
            throw new InvalidOperationException("No active game state to update");
        }

        try
        {
            await DoGameTickAsync(_currentGameState, updateAction, cancellationToken);
            _currentGameState.Touch();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update game state for {GameId}", _currentGameState.GameId);
            _telemetryService.TrackException(ex, new Dictionary<string, string> 
            { 
                ["operation"] = "UpdateGameState",
                ["gameId"] = _currentGameState.GameId 
            });
            throw;
        }
    }

    public abstract Task DoGameTickAsync(GameState currentState, UpdateGameRequest updateAction, CancellationToken cancellationToken = default);

    public async Task EndGameAsync(CancellationToken cancellationToken = default)
    {
        if (_currentGameState == null)
        {
            _logger.LogWarning("Attempted to end game but no active game state exists");
            return;
        }

        try
        {
            var gameId = _currentGameState.GameId;
            var gameMode = _currentGameState.GameMode;
            var duration = _currentGameState.GetDuration();
            var finalScore = _currentGameState.Score;
            var playerId = _currentGameState.PlayerId;

            _currentGameState.IsActive = false;
            _currentGameState.Touch();

            // Force final sync
            await ForceSyncAsync(cancellationToken);

            _isRunning = false;
            _telemetryService.TrackGameEnded(gameId, gameMode, duration, finalScore, playerId);

            _logger.LogInformation("Game {GameId} ended with score {Score} after {Duration}",
                gameId, finalScore, duration);

            _currentGameState = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to end game properly");
            _telemetryService.TrackException(ex, new Dictionary<string, string>
            {
                ["operation"] = "EndGame"
            });
            throw;
        }
    }

    public async Task ForceSyncAsync(CancellationToken cancellationToken = default)
    {
        if (_disposed || _currentGameState == null) return;

        await _syncSemaphore.WaitAsync(cancellationToken);
        try
        {
            await SyncStateAsync(cancellationToken);
        }
        finally
        {
            _syncSemaphore.Release();
        }
    }
    private async void SyncStateCallback(object? state)
    {
        if (_disposed || !_isRunning || _currentGameState == null)
            return;

        try
        {
            await SyncStateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync state in background");
        }
    }

    private async Task SyncStateAsync(CancellationToken cancellationToken = default)
    {
        if (_currentGameState == null) return;

        try
        {
            // Calculate current state hash to avoid unnecessary writes
            var currentHash = CalculateStateHash(_currentGameState);
            
            if (_lastStateHash != null && _lastStateHash == currentHash)
            {
                // State hasn't changed, skip sync
                return;
            }

            await _gameStateService.SaveGameStateAsync(_currentGameState, cancellationToken);
            _lastStateHash = currentHash;
            
            _telemetryService.TrackStateSaved(_currentGameState.GameId, true);
            _logger.LogDebug("Game state synced for {GameId}", _currentGameState.GameId);
        }
        catch (Exception ex)
        {
            _telemetryService.TrackStateSaved(_currentGameState.GameId, false, ex.Message);
            _logger.LogError(ex, "Failed to sync game state for {GameId}", _currentGameState.GameId);
            throw;
        }
    }

    private static string CalculateStateHash(GameState gameState)
    {
        // Simple hash based on key state properties
        var hashInput = $"{gameState.Score}|{gameState.Level}|{gameState.IsActive}|{gameState.CustomData.Count}";
        return hashInput.GetHashCode().ToString();
    }

    public void Dispose()
    {
        if (_disposed) return;

        try
        {
            // Stop the timer first to prevent new sync operations
            _syncTimer?.Dispose();
            
            // Force final sync if there's an active game (before disposing semaphore)
            if (_currentGameState != null && _isRunning)
            {
                try
                {
                    ForceSyncAsync().Wait(TimeSpan.FromSeconds(5));
                }
                catch (Exception syncEx)
                {
                    _logger.LogWarning(syncEx, "Failed to perform final sync during disposal");
                }
            }
            
            // Now dispose the semaphore after sync is complete
            _syncSemaphore?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during GameEngine disposal");
        }
        finally
        {
            _disposed = true;
        }
    }
}
