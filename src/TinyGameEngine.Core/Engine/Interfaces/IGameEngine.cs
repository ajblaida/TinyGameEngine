using TinyGameEngine.Core.Engine.Models;

namespace TinyGameEngine.Core.Engine.Interfaces;

/// <summary>
/// Main game engine interface
/// </summary>
public interface IGameEngine
{
    /// <summary>
    /// Current game state
    /// </summary>
    GameState? CurrentGameState { get; }

    /// <summary>
    /// Gets whether the engine is running
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets the game mode for this engine
    /// </summary>
    string GameMode { get; }

    /// <summary>
    /// Starts or loads a game
    /// </summary>
    Task<GameState> StartGameAsync(string playerId, CancellationToken cancellationToken = default);

    Task<GameState?> GetGameStateAsync(string gameId, string? playerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the current game state with custom update logic
    /// </summary>
    Task UpdateGameStateAsync(UpdateGameRequest updateAction, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends the current game
    /// </summary>
    Task EndGameAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Forces a state sync to storage
    /// </summary>
    Task ForceSyncAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Request to update game state
/// </summary>
/// <param name="PlayerId">The unique identifier for the player</param>
/// <param name="GameId">The unique identifier for the game</param>
/// <param name="Data">Arbitrary additional data for extensibility</param>
public record UpdateGameRequest(
    string PlayerId,
    string GameId,
    Dictionary<string, object>? Data = null);

/// <summary>
/// Request to start a new game session
/// </summary>
/// <param name="PlayerId">The unique identifier for the player</param>
/// <param name="Data">Arbitrary additional arguments for extensibility</param>
public record StartGameRequest(
    string PlayerId,
    Dictionary<string, object>? Data = null);
