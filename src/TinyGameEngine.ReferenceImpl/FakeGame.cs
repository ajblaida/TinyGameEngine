using TinyGameEngine.Core.Controllers;
using TinyGameEngine.Core.Engine.Interfaces;
using TinyGameEngine.Core.Engine.Models;
using TinyGameEngine.Core.Engine.Services;

namespace TinyGameEngine.ReferenceImpl;

/// <summary>
/// A reference implementation of a game that demonstrates how to extend TinyEngine
/// </summary>
public class FakeGame : TinyEngine
{
    public FakeGame(IGameStateService gameStateService, ITelemetryService telemetryService, IGameIdProvider gameIdProvider, ILogger<TinyEngine> logger) 
        : base(gameStateService, telemetryService, gameIdProvider, logger)
    {
    }

    public override string GameMode => "Fake Game";

    /// <summary>
    /// Example implementation of game logic - this is where your game rules would go
    /// </summary>
    public override Task DoGameTickAsync(GameState currentState, UpdateGameRequest updateAction, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("FakeGame DoGameTickAsync called with PlayerId: {PlayerId}, GameId: {GameId}", updateAction.PlayerId, updateAction.GameId);
        
        // Example: You could modify the game state here based on the updateAction
        // currentState.Score += 10;
        // currentState.LastUpdated = DateTime.UtcNow;
        
        return Task.CompletedTask;
    }
}
