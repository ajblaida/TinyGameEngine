namespace TinyGameEngine.Core.Engine.Interfaces;

/// <summary>
/// Interface for telemetry operations
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    /// Tracks when a game is started
    /// </summary>
    void TrackGameStarted(string gameId, string gameMode, string? playerId = null);

    /// <summary>
    /// Tracks when a game ends
    /// </summary>
    void TrackGameEnded(string gameId, string gameMode, TimeSpan duration, long finalScore, string? playerId = null);

    /// <summary>
    /// Tracks frame time metrics
    /// </summary>
    void TrackFrameTime(double frameTimeMs);

    /// <summary>
    /// Tracks when state is saved
    /// </summary>
    void TrackStateSaved(string gameId, bool success = true, string? errorMessage = null);

    /// <summary>
    /// Tracks high score events
    /// </summary>
    void TrackHighScore(string gameMode, long score, string player, bool isNewRecord = false);

    /// <summary>
    /// Tracks custom game events
    /// </summary>
    void TrackEvent(string eventName, Dictionary<string, string>? properties = null, Dictionary<string, double>? metrics = null);

    /// <summary>
    /// Tracks exceptions
    /// </summary>
    void TrackException(Exception exception, Dictionary<string, string>? properties = null);
}
