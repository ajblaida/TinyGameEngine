# Tiny Game Engine

A lightweight, opinionated game engine designed to run in short-lived Azure Container Apps with state persistence via Azure Blob Storage and telemetry via Azure Application Insights.

## NuGet
https://www.nuget.org/packages/TinyGameEngine.Core/

## Status
[![.github/workflows/ci-cd.yml](https://github.com/ajblaida/TinyGameEngine/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/ajblaida/TinyGameEngine/actions/workflows/ci-cd.yml)

## 🎮 NuGet Packages

TinyGameEngine consists of two packages for easy consumption:

### [TinyGameEngine.Core](src/TinyGameEngine.Core/)
The core engine with all the infrastructure you need:
- State management with Azure Blob Storage
- High score tracking and leaderboards  
- Application Insights telemetry
- Complete ASP.NET Core API
- One-line service registration

### [TinyGameEngine.ReferenceImpl](src/TinyGameEngine.ReferenceImpl/)
Example implementation and deployment templates:
- Sample `FakeGame` implementation
- Minimal bootstrap code
- Azure deployment templates
- Configuration examples

## 🚀 Quick Start

### Option 1: Clone and Run Reference Implementation
```bash
# Clone this repository
git clone https://github.com/your-org/tiny-game-engine.git
cd tiny-game-engine

# Build the solution
dotnet build

# Run the reference implementation
dotnet run --project src/TinyGameEngine.ReferenceImpl

# Or run via Docker
docker build -t tinygameengine .
docker run -p 8080:8080 tinygameengine
```

### Option 2: Create Your Own Game
```bash
# Create new project
dotnet new webapi -n MyGame
cd MyGame

# Install the core package (when published to NuGet)
dotnet add package TinyGameEngine.Core
```

**Create your game** (`MyGame.cs`):
```csharp
using TinyGameEngine.Core.Engine.Interfaces;
using TinyGameEngine.Core.Engine.Models;
using TinyGameEngine.Core.Engine.Services;
using TinyGameEngine.Core.Controllers;

public class MyGame : TinyEngine
{
    public MyGame(IGameStateService gameStateService, ITelemetryService telemetryService, 
                  IGameIdProvider gameIdProvider, ILogger<TinyEngine> logger) 
        : base(gameStateService, telemetryService, gameIdProvider, logger) { }

    public override string GameMode => "My Awesome Game";

    public override Task DoGameTickAsync(GameState currentState, UpdateGameRequest updateAction, 
                                       CancellationToken cancellationToken = default)
    {
        // Your game logic here!
        currentState.Score += updateAction.Score ?? 0;
        currentState.Level = Math.Max(1, currentState.Level);
        
        // Example: Level up every 1000 points
        var newLevel = (currentState.Score / 1000) + 1;
        if (newLevel > currentState.Level)
        {
            currentState.Level = newLevel;
            _logger.LogInformation("Player {PlayerId} leveled up to {Level}!", 
                updateAction.PlayerId, newLevel);
        }
        
        return Task.CompletedTask;
    }
}
```

**Bootstrap your app** (`Program.cs`):
```csharp
using TinyGameEngine.Core.Extensions;
using TinyGameEngine.Core.Engine.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// One line to add all TinyGameEngine services
builder.Services.AddTinyGameEngine(builder.Configuration);
builder.Services.AddScoped<IGameEngine, MyGame>();

var app = builder.Build();
app.UseTinyGameEngine(); // Configures the entire pipeline
app.Run();
```

## ⚙️ Configuration

Update your `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "BlobStorage": "UseDevelopmentStorage=true",
    "ApplicationInsights": "your-app-insights-connection-string"  
  },
  "Azure": {
    "StorageAccountName": "your-storage-account-name"
  }
}
```

For development, use [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) for local blob storage:
```bash
# Install Azurite
npm install -g azurite

# Start Azurite
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

## 🏗️ Building and Running

### Prerequisites
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/get-started) (optional)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli) (for deployment)
- [Azurite](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-azurite) (for local development)

### Build Commands
```bash
# Build all packages
dotnet build TinyGameEngine.Packages.sln

# Build specific package
dotnet build src/TinyGameEngine.Core
dotnet build src/TinyGameEngine.ReferenceImpl

# Pack for NuGet
dotnet pack src/TinyGameEngine.Core
dotnet pack src/TinyGameEngine.ReferenceImpl
```

### Run Commands
```bash
# Run the reference implementation
dotnet run --project src/TinyGameEngine.ReferenceImpl

# Run with specific profile
dotnet run --project src/TinyGameEngine.ReferenceImpl --launch-profile https

# Build and run Docker container
docker build -t tinygameengine .
docker run -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development tinygameengine
```

### Testing the API
Use the included `TinyGameEngine.http` file with REST Client or:

```bash
# Health check
curl http://localhost:5107/health

# Start a game
curl -X POST http://localhost:5107/api/game/start \
  -H "Content-Type: application/json" \
  -d '{"gameId":"test-123","gameMode":"Fake Game","playerId":"player1"}'

# Update game state  
curl -X POST http://localhost:5107/api/game/update \
  -H "Content-Type: application/json" \
  -d '{"gameId":"test-123","playerId":"player1","score":100}'

# Get game state
curl http://localhost:5107/api/game/test-123
```

## 🚀 Deployment to Azure

### Using Azure Developer CLI (Recommended)
```bash
# Install azd
winget install microsoft.azd

# Initialize and deploy
cd src/TinyGameEngine.ReferenceImpl
azd init
azd up
```

### Using Docker + Azure Container Apps
```bash
# Build and push to registry
docker build -t your-registry.azurecr.io/tinygameengine .
docker push your-registry.azurecr.io/tinygameengine

# Deploy using Azure CLI or portal
az containerapp create \
  --name tinygameengine \
  --resource-group your-rg \
  --environment your-env \
  --image your-registry.azurecr.io/tinygameengine
```

## 📚 Features

- **State Management**: Load/save game state via Azure Blob Storage
- **Periodic Sync**: State synced automatically at most once per second
- **High Scores**: Blob-based high score tracking with leaderboards
- **Telemetry**: Integrated Azure Application Insights for metrics and events
- **Container Ready**: Designed for Azure Container Apps with scale-to-zero
- **Restart Tolerance**: Containers can spin up/down seamlessly
- **CORS Enabled**: Ready for web client integration
- **Health Checks**: Built-in health monitoring
- **Swagger UI**: Interactive API documentation

## 🏗️ Architecture

### Core Components

- **GameEngine**: Main orchestrator with automatic state sync
- **GameStateService**: Manages state persistence in Blob Storage
- **HighScoreService**: Handles high score tracking and leaderboards  
- **TelemetryService**: Application Insights integration
- **REST API**: HTTP endpoints for game operations

### Azure Resources

- **Azure Container Apps**: Scale-to-zero container host
- **Azure Blob Storage**: Game state and high score persistence
- **Azure Application Insights**: Telemetry and monitoring
- **Managed Identity**: Secure access without connection strings

## 🔌 API Endpoints

### Game Management

- `POST /api/game/start` - Start or load a game
  ```json
  {
    "gameId": "my-game-123",
    "gameMode": "My Game",
    "playerId": "player1"
  }
  ```

- `POST /api/game/update` - Update game state
  ```json
  {
    "gameId": "my-game-123", 
    "playerId": "player1",
    "score": 1500,
    "level": 3,
    "customData": {
      "lives": 3,
      "powerups": ["speed", "shield"]
    }
  }
  ```

- `GET /api/game/{gameId}` - Get current game state

### High Scores
- `POST /api/game/highscore` - Submit a high score
- `GET /api/game/highscores?count=10` - Get top high scores

### Health & Monitoring
- `GET /health` - Health check endpoint
- `GET /swagger` - Swagger UI (development only)

## 🧪 Development

### Local Setup
1. **Clone the repository**
   ```bash
   git clone https://github.com/your-org/tiny-game-engine.git
   cd tiny-game-engine
   ```

2. **Start Azurite** (for local blob storage)
   ```bash
   azurite --silent --location c:\azurite --debug c:\azurite\debug.log
   ```

3. **Run the reference implementation**
   ```bash
   dotnet run --project src/TinyGameEngine.ReferenceImpl
   ```

4. **Open Swagger UI**
   Navigate to `http://localhost:5107/swagger`

### Project Structure
```
src/
├── TinyGameEngine.Core/              # 📦 Core Package
│   ├── Controllers/                  # API controllers
│   ├── Engine/
│   │   ├── Interfaces/              # Service contracts
│   │   ├── Models/                  # Data models
│   │   └── Services/                # Implementations
│   └── Extensions/                  # DI registration
└── TinyGameEngine.ReferenceImpl/     # 📦 Reference Package
    ├── FakeGame.cs                  # Example game
    ├── Program.cs                   # Bootstrap
    ├── infra/                       # Bicep templates
    └── azure.yaml                   # Azure deployment
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🔗 Links

- [Azure Container Apps Documentation](https://docs.microsoft.com/en-us/azure/container-apps/)
- [Azure Blob Storage Documentation](https://docs.microsoft.com/en-us/azure/storage/blobs/)
- [Application Insights Documentation](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [Migration Guide](MIGRATION-GUIDE.md) - How to migrate from the old monolithic structure
  --name tinygameengine \
  --resource-group your-rg \
  --environment your-env \
  --image your-registry.azurecr.io/tinygameengine
```

## Features

- **State Management**: Load/save game state via Azure Blob Storage
- **Periodic Sync**: State synced automatically at most once per second
- **High Scores**: Blob-based high score tracking with leaderboards
- **Telemetry**: Integrated Azure Application Insights for metrics and events
- **Container Ready**: Designed for Azure Container Apps with scale-to-zero
- **Restart Tolerance**: Containers can spin up/down seamlessly

## Architecture

### Core Components

- **GameEngine**: Main orchestrator with automatic state sync
- **GameStateService**: Manages state persistence in Blob Storage
- **HighScoreService**: Handles high score tracking and leaderboards  
- **TelemetryService**: Application Insights integration
- **REST API**: HTTP endpoints for game operations

### Azure Resources

- **Azure Container Apps**: Scale-to-zero container host
- **Azure Blob Storage**: Game state and high score persistence
- **Azure Application Insights**: Telemetry and monitoring
- **Managed Identity**: Secure access without connection strings

## API Endpoints

### Game Management

- `POST /api/game/start` - Start or load a game
- `GET /api/game/state` - Get current game state  
- `POST /api/game/update` - Update game state
- `POST /api/game/end` - End current game
- `POST /api/game/sync` - Force state synchronization

### High Scores

- `POST /api/game/highscore` - Submit a high score
- `GET /api/game/highscores` - Get leaderboard

### Health

- `GET /health` - Health check endpoint

## Local Development

### Prerequisites

- .NET 9.0 SDK
- Docker Desktop
- Azurite (Azure Storage Emulator)

### Setup

1. **Install Azurite**:
   ```bash
   npm install -g azurite
   ```

2. **Start Azurite**:
   ```bash
   azurite --silent --location c:\azurite --debug c:\azurite\debug.log
   ```

3. **Run the application**:
   ```bash
   dotnet run
   ```

4. **Test the API**:
   ```bash
   # Start a game
   curl -X POST http://localhost:5000/api/game/start \
     -H "Content-Type: application/json" \
     -d '{"gameId":"test-123","gameMode":"default","playerId":"player1"}'

   # Update game state
   curl -X POST http://localhost:5000/api/game/update \
     -H "Content-Type: application/json" \
     -d '{"score":1000,"level":2}'

   # Submit high score
   curl -X POST http://localhost:5000/api/game/highscore \
     -H "Content-Type: application/json" \
     -d '{"score":1000,"player":"player1"}'
   ```

## Azure Deployment

### Option 1: Azure Developer CLI (Recommended)

1. **Install Azure Developer CLI**:
   ```bash
   winget install Microsoft.AzureDeveloperCLI
   ```

2. **Initialize and deploy**:
   ```bash
   azd init
   azd up
   ```

### Option 2: Manual Deployment

1. **Build and push container**:
   ```bash
   # Build the image
   docker build -t tinygameengine .

   # Tag for Azure Container Registry
   docker tag tinygameengine <your-acr>.azurecr.io/tinygameengine:latest

   # Push to ACR
   docker push <your-acr>.azurecr.io/tinygameengine:latest
   ```

2. **Deploy infrastructure**:
   ```bash
   az deployment group create \
     --resource-group <your-rg> \
     --template-file infra/main.bicep \
     --parameters infra/main.parameters.json \
     --parameters containerImage=<your-acr>.azurecr.io/tinygameengine:latest
   ```

## Configuration

### Environment Variables

- `Azure__StorageAccountName`: Name of the storage account (production)
- `ConnectionStrings__BlobStorage`: Storage connection string (development)
- `ConnectionStrings__ApplicationInsights`: App Insights connection string
- `AZURE_CLIENT_ID`: Managed identity client ID (production)

### Security

- Uses **Managed Identity** in production for secure access
- **RBAC** permissions for Blob Storage access
- **No connection strings** stored in container apps

## Monitoring

### Application Insights Events

- `GameStarted`: When a game session begins
- `GameEnded`: When a game session ends (with duration/score)
- `HighScore`: When high scores are submitted
- `StateSaved`: When state is persisted

### Metrics

- `FrameTime`: Game frame timing metrics
- Custom metrics via `TrackMetric()`

### Health Checks

- `/health`: Overall application health
- Blob storage connectivity check

## Performance Considerations

- **State sync throttling**: Max 1 write per second to Blob Storage
- **Hash-based change detection**: Skip writes when state unchanged
- **Optimistic concurrency**: ETag support for safe concurrent writes
- **Scale-to-zero**: Container Apps automatically scale based on traffic

## Development Notes

### State Management

States are stored as JSON blobs in the pattern:
```
games/{gameId}/state.json
```

### High Scores

High scores are stored as JSON collections:
```
hiscores/{gameMode}.json
```

### Error Handling

- All exceptions are logged to Application Insights
- Graceful degradation when storage is unavailable
- Automatic retry logic for transient failures

## Testing

```bash
# Run unit tests
dotnet test

# Build and test container
docker build -t tinygame-test .
docker run -p 8080:8080 tinygame-test
```

## License

MIT License - see LICENSE file for details.
