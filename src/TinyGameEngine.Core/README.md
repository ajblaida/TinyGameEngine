# TinyGameEngine.Core

A lightweight, opinionated game engine core designed for Azure Container Apps with state persistence via Azure Blob Storage and telemetry via Azure Application Insights.

## Features

- **State Management**: Automatic load/save game state via Azure Blob Storage
- **Periodic Sync**: State synced automatically at most once per second with hash-based change detection
- **High Scores**: Blob-based high score tracking with leaderboards
- **Telemetry**: Integrated Azure Application Insights for metrics and events
- **Container Ready**: Designed for Azure Container Apps with scale-to-zero
- **Restart Tolerance**: Containers can spin up/down seamlessly and recover last known state

## Quick Start

### 1. Install the Package

```bash
dotnet add package TinyGameEngine.Core
```

### 2. Create Your Game Engine

Inherit from `TinyEngine` and implement the `DoGameTickAsync` method:

```csharp
using TinyGameEngine.Core.Engine.Interfaces;
using TinyGameEngine.Core.Engine.Models;
using TinyGameEngine.Core.Engine.Services;

public class MyGameEngine : TinyEngine
{
    public MyGameEngine(
        IGameStateService gameStateService,
        ITelemetryService telemetryService,
        IGameIdProvider gameIdProvider,
        ILogger<TinyEngine> logger) 
        : base(gameStateService, telemetryService, gameIdProvider, logger)
    {
    }

    public override string GameMode => "My Awesome Game";

    public override async Task DoGameTickAsync(
        GameState currentState, 
        UpdateGameRequest updateAction, 
        CancellationToken cancellationToken = default)
    {
        // Your game logic here!
        // Example: Update score based on action data
        if (updateAction.Data?.TryGetValue("points", out var points) == true)
        {
            currentState.Score += Convert.ToInt64(points);
        }

        // Track telemetry
        _telemetryService.TrackEvent("GameTick", new Dictionary<string, string>
        {
            ["gameId"] = currentState.GameId,
            ["playerId"] = currentState.PlayerId
        });

        _logger.LogDebug("Game tick processed for {GameId}", currentState.GameId);
    }
}
```

### 3. Configure Your Application

In your `Program.cs`:

```csharp
using TinyGameEngine.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add TinyGameEngine services
builder.Services.AddTinyGameEngine(builder.Configuration, options =>
{
    options.StorageAccountName = "your-storage-account"; // Production
    options.BlobStorageConnectionString = "UseDevelopmentStorage=true"; // Development
    options.ApplicationInsightsConnectionString = "your-app-insights-connection";
});

// Register your game engine
builder.Services.AddGameEngine<MyGameEngine>();

var app = builder.Build();

// Configure pipeline
app.UseCors();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

### 4. Configure Settings

**appsettings.Development.json:**
```json
{
  "ConnectionStrings": {
    "BlobStorage": "UseDevelopmentStorage=true",
    "ApplicationInsights": ""
  }
}
```

**appsettings.json:**
```json
{
  "ConnectionStrings": {
    "BlobStorage": "",
    "ApplicationInsights": ""
  },
  "Azure": {
    "StorageAccountName": "your-storage-account"
  }
}
```

## API Endpoints

The framework automatically provides these REST endpoints:

### Game Management
- `POST /api/game/start` - Start a new game session
- `GET /api/game/state` - Get current game state
- `POST /api/game/update` - Update game state (calls your `DoGameTickAsync`)
- `POST /api/game/end` - End current game session
- `POST /api/game/sync` - Force state synchronization

### Health
- `GET /health` - Application health check

## Architecture

### Core Components

- **TinyEngine**: Abstract base class for your game implementation
- **GameController**: REST API controller for game operations
- **IGameStateService**: Manages state persistence (Azure Blob Storage)
- **IHighScoreService**: Handles high score tracking and leaderboards
- **ITelemetryService**: Application Insights integration
- **IGameIdProvider**: Generates unique game identifiers

### Game State Model

```csharp
public class GameState
{
    public string GameId { get; set; }
    public string GameMode { get; set; }
    public string PlayerId { get; set; }
    public long Score { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset StartTime { get; set; }
    public DateTimeOffset LastUpdated { get; set; }
    public Dictionary<string, object> CustomData { get; set; }
}
```

## Configuration Options

```csharp
builder.Services.AddTinyGameEngine(configuration, options =>
{
    // Azure Storage account name (production with managed identity)
    options.StorageAccountName = "mystorageaccount";
    
    // Connection string for development
    options.BlobStorageConnectionString = "UseDevelopmentStorage=true";
    
    // Application Insights connection string
    options.ApplicationInsightsConnectionString = "your-connection-string";
    
    // CORS origins (null allows any origin)
    options.AllowedOrigins = new[] { "https://yourdomain.com" };
    
    // Initialize blob containers on startup
    options.InitializeBlobContainers = true;
});
```

## Development

### Prerequisites

- .NET 9.0 SDK
- Azurite for local development
- Azure Storage Account and Application Insights for production

### Local Development

1. **Install Azurite:**
   ```bash
   npm install -g azurite
   ```

2. **Start Azurite:**
   ```bash
   azurite --silent --location c:\azurite
   ```

3. **Run your application:**
   ```bash
   dotnet run
   ```

### Testing Your Game

```bash
# Start a game
curl -X POST http://localhost:5000/api/game/start \
  -H "Content-Type: application/json" \
  -d '{"playerId":"player1"}'

# Update game state
curl -X POST http://localhost:5000/api/game/update \
  -H "Content-Type: application/json" \
  -d '{"playerId":"player1","gameId":"game-123","data":{"points":100}}'

# Get current state
curl http://localhost:5000/api/game/state
```

## Production Deployment

### Azure Container Apps

The framework is optimized for Azure Container Apps with:

- **Scale-to-zero** capability
- **Managed Identity** for secure Azure resource access
- **Automatic state persistence** and recovery
- **Built-in health checks**

### Required Azure Resources

- **Azure Container Apps Environment**
- **Azure Storage Account** (with blob containers: `gamestate`, `highscores`)
- **Azure Application Insights**
- **Azure Container Registry** (for your custom game image)

## License

MIT License
