using TinyGameEngine.Core.Engine.Interfaces;

namespace TinyGameEngine.Core.Engine.Services;

/// <summary>
/// No-operation implementation of telemetry service for development/testing scenarios
/// </summary>
public class NoOpTelemetryService : ITelemetryService
{
    public void TrackGameStarted(string gameId, string gameMode, string? playerId = null)
    {
        // No-op
    }

    public void TrackGameEnded(string gameId, string gameMode, TimeSpan duration, long finalScore, string? playerId = null)
    {
        // No-op
    }

    public void TrackFrameTime(double frameTimeMs)
    {
        // No-op
    }

    public void TrackStateSaved(string gameId, bool success = true, string? errorMessage = null)
    {
        // No-op
    }

    public void TrackHighScore(string gameMode, long score, string player, bool isNewRecord = false)
    {
        // No-op
    }

    public void TrackEvent(string eventName, Dictionary<string, string>? properties = null, Dictionary<string, double>? metrics = null)
    {
        // No-op
    }

    public void TrackException(Exception exception, Dictionary<string, string>? properties = null)
    {
        // No-op
    }
}
