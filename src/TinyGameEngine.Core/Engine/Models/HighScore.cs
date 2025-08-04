using System.Text.Json.Serialization;

namespace TinyGameEngine.Core.Engine.Models;

/// <summary>
/// Represents a high score entry
/// </summary>
public class HighScore
{
    [JsonPropertyName("player")]
    public string Player { get; set; } = string.Empty;

    [JsonPropertyName("score")]
    public long Score { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("gameMode")]
    public string GameMode { get; set; } = "default";

    [JsonPropertyName("metadata")]
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Collection of high scores for a specific game mode
/// </summary>
public class HighScoreCollection
{
    [JsonPropertyName("gameMode")]
    public string GameMode { get; set; } = string.Empty;

    [JsonPropertyName("scores")]
    public List<HighScore> Scores { get; set; } = new();

    [JsonPropertyName("lastUpdated")]
    public DateTimeOffset LastUpdated { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("maxEntries")]
    public int MaxEntries { get; set; } = 100;

    /// <summary>
    /// Adds a new score and maintains the collection sorted and within max entries
    /// </summary>
    public void AddScore(HighScore score)
    {
        Scores.Add(score);
        Scores = Scores.OrderByDescending(s => s.Score)
                      .ThenBy(s => s.Timestamp)
                      .Take(MaxEntries)
                      .ToList();
        LastUpdated = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Gets the top N scores
    /// </summary>
    public List<HighScore> GetTopScores(int count = 10)
    {
        return Scores.Take(count).ToList();
    }
}
