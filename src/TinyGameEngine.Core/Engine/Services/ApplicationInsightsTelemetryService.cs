using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using TinyGameEngine.Core.Engine.Interfaces;

namespace TinyGameEngine.Core.Engine.Services;

/// <summary>
/// Application Insights implementation of telemetry service
/// </summary>
public class ApplicationInsightsTelemetryService : ITelemetryService
{
    private readonly TelemetryClient _telemetryClient;

    public ApplicationInsightsTelemetryService(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void TrackGameStarted(string gameId, string gameMode, string? playerId = null)
    {
        var properties = new Dictionary<string, string>
        {
            ["gameId"] = gameId,
            ["gameMode"] = gameMode
        };

        if (!string.IsNullOrEmpty(playerId))
        {
            properties["playerId"] = playerId;
        }

        _telemetryClient.TrackEvent("GameStarted", properties);
    }

    public void TrackGameEnded(string gameId, string gameMode, TimeSpan duration, long finalScore, string? playerId = null)
    {
        var properties = new Dictionary<string, string>
        {
            ["gameId"] = gameId,
            ["gameMode"] = gameMode
        };

        if (!string.IsNullOrEmpty(playerId))
        {
            properties["playerId"] = playerId;
        }

        var metrics = new Dictionary<string, double>
        {
            ["duration"] = duration.TotalSeconds,
            ["finalScore"] = finalScore
        };

        _telemetryClient.TrackEvent("GameEnded", properties, metrics);
    }

    public void TrackFrameTime(double frameTimeMs)
    {
        _telemetryClient.TrackMetric("FrameTime", frameTimeMs);
    }

    public void TrackStateSaved(string gameId, bool success = true, string? errorMessage = null)
    {
        var properties = new Dictionary<string, string>
        {
            ["gameId"] = gameId,
            ["success"] = success.ToString()
        };

        if (!string.IsNullOrEmpty(errorMessage))
        {
            properties["errorMessage"] = errorMessage;
        }

        var logLevel = success ? SeverityLevel.Information : SeverityLevel.Error;
        _telemetryClient.TrackTrace($"StateSaved for {gameId}", logLevel, properties);
    }

    public void TrackHighScore(string gameMode, long score, string player, bool isNewRecord = false)
    {
        var properties = new Dictionary<string, string>
        {
            ["gameMode"] = gameMode,
            ["player"] = player,
            ["isNewRecord"] = isNewRecord.ToString()
        };

        var metrics = new Dictionary<string, double>
        {
            ["score"] = score
        };

        _telemetryClient.TrackEvent("HighScore", properties, metrics);
    }

    public void TrackEvent(string eventName, Dictionary<string, string>? properties = null, Dictionary<string, double>? metrics = null)
    {
        _telemetryClient.TrackEvent(eventName, properties, metrics);
    }

    public void TrackException(Exception exception, Dictionary<string, string>? properties = null)
    {
        _telemetryClient.TrackException(exception, properties);
    }
}
