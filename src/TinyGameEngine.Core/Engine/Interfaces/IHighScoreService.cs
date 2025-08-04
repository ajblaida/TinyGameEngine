using TinyGameEngine.Core.Engine.Models;

namespace TinyGameEngine.Core.Engine.Interfaces;

/// <summary>
/// Interface for managing high scores
/// </summary>
public interface IHighScoreService
{
    /// <summary>
    /// Adds a new high score
    /// </summary>
    Task AddHighScoreAsync(HighScore highScore, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets high scores for a specific game mode
    /// </summary>
    Task<List<HighScore>> GetHighScoresAsync(string gameMode, int count = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all high scores for a game mode
    /// </summary>
    Task<HighScoreCollection?> GetHighScoreCollectionAsync(string gameMode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a score qualifies as a high score
    /// </summary>
    Task<bool> IsHighScoreAsync(string gameMode, long score, CancellationToken cancellationToken = default);
}
