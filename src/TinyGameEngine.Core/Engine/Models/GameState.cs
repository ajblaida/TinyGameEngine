using System.Text.Json.Serialization;

namespace TinyGameEngine.Core.Engine.Models;

/// <summary>
/// Represents the complete state of a game instance
/// </summary>
public class GameState
{
    [JsonPropertyName("gameId")]
    public string GameId { get; set; } = string.Empty;

    [JsonPropertyName("gameMode")]
    public string GameMode { get; set; } = "default";

    [JsonPropertyName("playerId")]
    public string PlayerId { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public long Score { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; } = 1;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("startTime")]
    public DateTimeOffset StartTime { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("lastUpdated")]
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("customData")]
    public Dictionary<string, object> CustomData { get; set; } = new();

    /// <summary>
    /// Updates the last updated timestamp
    /// </summary>
    public void Touch()
    {
        LastUpdated = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Calculates the duration of the current game session
    /// </summary>
    public TimeSpan GetDuration() => LastUpdated - StartTime;
}
