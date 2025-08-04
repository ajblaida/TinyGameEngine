using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TinyGameEngine.Core.Engine.Interfaces;
using TinyGameEngine.Core.Engine.Models;

namespace TinyGameEngine.Core.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GameController : ControllerBase
{
    private readonly IGameEngine _gameEngine;
    private readonly ILogger<GameController> _logger;

    public GameController(IGameEngine gameEngine, ILogger<GameController> logger)
    {
        _gameEngine = gameEngine;
        _logger = logger;
    }

    [HttpPost("start")]
    public async Task<ActionResult<GameState>> StartGame([FromBody] StartGameRequest request)
    {
        try
        {
            var gameState = await _gameEngine.StartGameAsync(
                request.PlayerId, 
                HttpContext.RequestAborted);
                
            return Ok(gameState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start game");
            return StatusCode(500, new { error = "Failed to start game" });
        }
    }

    [HttpGet("state")]
    public ActionResult<GameState> GetCurrentState([FromQuery] string gameId, [FromQuery] string? playerId)
    {
        if (string.IsNullOrEmpty(gameId))
        {
            return BadRequest(new { error = "Game ID is required" });
        }

        var state = _gameEngine.GetGameStateAsync(gameId, playerId, HttpContext.RequestAborted).Result;
        if (state == null)
        {
            return NotFound(new { error = "No active game" });
        }
        
        return Ok(state);
    }

    [HttpPost("update")]
    public async Task<ActionResult<GameState>> UpdateGame([FromBody] UpdateGameRequest request)
    {
        try
        {
            await _gameEngine.UpdateGameStateAsync(request, HttpContext.RequestAborted);

            return Ok(_gameEngine.CurrentGameState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update game");
            return StatusCode(500, new { error = "Failed to update game" });
        }
    }

    [HttpPost("end")]
    public async Task<IActionResult> EndGame()
    {
        try
        {
            await _gameEngine.EndGameAsync(HttpContext.RequestAborted);
            return Ok(new { message = "Game ended successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to end game");
            return StatusCode(500, new { error = "Failed to end game" });
        }
    }

    [HttpPost("sync")]
    public async Task<IActionResult> ForceSync()
    {
        try
        {
            await _gameEngine.ForceSyncAsync(HttpContext.RequestAborted);
            return Ok(new { message = "State synchronized" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to sync state");
            return StatusCode(500, new { error = "Failed to sync state" });
        }
    }
}
