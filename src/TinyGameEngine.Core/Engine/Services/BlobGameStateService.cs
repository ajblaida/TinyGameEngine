using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;
using System.Text.Json;
using TinyGameEngine.Core.Engine.Interfaces;
using TinyGameEngine.Core.Engine.Models;

namespace TinyGameEngine.Core.Engine.Services;

/// <summary>
/// Azure Blob Storage implementation of game state service
/// </summary>
public class BlobGameStateService : IGameStateService
{
    private readonly BlobContainerClient _containerClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public BlobGameStateService(BlobServiceClient blobServiceClient)
    {
        _containerClient = blobServiceClient.GetBlobContainerClient("gamestate");
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<GameState?> LoadGameStateAsync(string gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobName = GetBlobName(gameId);
            var blobClient = _containerClient.GetBlobClient(blobName);

            if (!await blobClient.ExistsAsync(cancellationToken))
            {
                return null;
            }

            var response = await blobClient.DownloadContentAsync(cancellationToken);
            var json = response.Value.Content.ToString();
            
            return JsonSerializer.Deserialize<GameState>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            // Log the exception (would be handled by telemetry service)
            throw new InvalidOperationException($"Failed to load game state for {gameId}", ex);
        }
    }

    public async Task SaveGameStateAsync(GameState gameState, CancellationToken cancellationToken = default)
    {
        try
        {
            gameState.Touch(); // Update the last modified time
            
            var json = JsonSerializer.Serialize(gameState, _jsonOptions);
            var blobName = GetBlobName(gameState.GameId);
            var blobClient = _containerClient.GetBlobClient(blobName);

            var content = new BinaryData(Encoding.UTF8.GetBytes(json));
            
            // Use optimistic concurrency with ETag if possible
            var uploadOptions = new BlobUploadOptions
            {
                Conditions = new BlobRequestConditions(),
                Metadata = new Dictionary<string, string>
                {
                    ["gameMode"] = gameState.GameMode,
                    ["playerId"] = gameState.PlayerId,
                    ["lastUpdated"] = gameState.LastUpdated.ToString("O")
                }
            };

            await blobClient.UploadAsync(content, uploadOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save game state for {gameState.GameId}", ex);
        }
    }

    public async Task<bool> GameStateExistsAsync(string gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobName = GetBlobName(gameId);
            var blobClient = _containerClient.GetBlobClient(blobName);
            
            var response = await blobClient.ExistsAsync(cancellationToken);
            return response.Value;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to check if game state exists for {gameId}", ex);
        }
    }

    public async Task DeleteGameStateAsync(string gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var blobName = GetBlobName(gameId);
            var blobClient = _containerClient.GetBlobClient(blobName);
            
            await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete game state for {gameId}", ex);
        }
    }

    private static string GetBlobName(string gameId)
    {
        return $"games/{gameId}/state.json";
    }
}
