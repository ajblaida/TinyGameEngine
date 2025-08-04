using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;
using System.Text.Json;
using TinyGameEngine.Core.Engine.Interfaces;
using TinyGameEngine.Core.Engine.Models;

namespace TinyGameEngine.Core.Engine.Services;

/// <summary>
/// Azure Blob Storage implementation of high score service
/// </summary>
public class BlobHighScoreService : IHighScoreService
{
    private readonly BlobContainerClient _containerClient;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public BlobHighScoreService(BlobServiceClient blobServiceClient)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient("highscores");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task AddHighScoreAsync(HighScore highScore, CancellationToken cancellationToken = default)
    {
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            // Load existing collection
            var collection = await GetHighScoreCollectionAsync(highScore.GameMode, cancellationToken) 
                            ?? new HighScoreCollection { GameMode = highScore.GameMode };

            // Add the new score
            collection.AddScore(highScore);

            // Save back to storage
            await SaveHighScoreCollectionAsync(collection, cancellationToken);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<List<HighScore>> GetHighScoresAsync(string gameMode, int count = 10, CancellationToken cancellationToken = default)
    {
        var collection = await GetHighScoreCollectionAsync(gameMode, cancellationToken);
        return collection?.GetTopScores(count) ?? new List<HighScore>();
    }

    public async Task<HighScoreCollection?> GetHighScoreCollectionAsync(string gameMode, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobName = GetBlobName(gameMode);
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                return null;
            }

            var response = await blobClient.DownloadContentAsync(cancellationToken);
            var json = response.Value.Content.ToString();
            
            return JsonSerializer.Deserialize<HighScoreCollection>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to load high scores for {gameMode}", ex);
        }
    }

    public async Task<bool> IsHighScoreAsync(string gameMode, long score, CancellationToken cancellationToken = default)
    {
        var collection = await GetHighScoreCollectionAsync(gameMode, cancellationToken);
        
        if (collection == null || collection.Scores.Count < collection.MaxEntries)
        {
            return true; // Always a high score if we have space
        }

        var lowestHighScore = collection.Scores.LastOrDefault();
        return lowestHighScore == null || score > lowestHighScore.Score;
    }

    private async Task SaveHighScoreCollectionAsync(HighScoreCollection collection, CancellationToken cancellationToken = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(collection, _jsonOptions);
            var blobName = GetBlobName(collection.GameMode);
            var blobClient = _containerClient.GetBlobClient(blobName);

            var content = new BinaryData(Encoding.UTF8.GetBytes(json));
            
            var uploadOptions = new BlobUploadOptions
            {
                Conditions = new BlobRequestConditions(),
                Metadata = new Dictionary<string, string>
                {
                    ["gameMode"] = collection.GameMode,
                    ["scoreCount"] = collection.Scores.Count.ToString(),
                    ["lastUpdated"] = collection.LastUpdated.ToString("O")
                }
            };

            await blobClient.UploadAsync(content, uploadOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save high scores for {collection.GameMode}", ex);
        }
    }

    private static string GetBlobName(string gameMode)
    {
        return $"hiscores/{gameMode}.json";
    }
}
