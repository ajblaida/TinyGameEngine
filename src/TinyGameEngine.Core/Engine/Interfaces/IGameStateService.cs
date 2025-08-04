using TinyGameEngine.Core.Engine.Models;

namespace TinyGameEngine.Core.Engine.Interfaces;

/// <summary>
/// Interface for managing game state persistence
/// </summary>
public interface IGameStateService
{
    /// <summary>
    /// Loads game state from storage
    /// </summary>
    Task<GameState?> LoadGameStateAsync(string gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves game state to storage
    /// </summary>
    Task SaveGameStateAsync(GameState gameState, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a game state exists
    /// </summary>
    Task<bool> GameStateExistsAsync(string gameId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes game state from storage
    /// </summary>
    Task DeleteGameStateAsync(string gameId, CancellationToken cancellationToken = default);
}
