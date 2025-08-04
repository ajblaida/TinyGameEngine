# TinyGameEngine.ReferenceImpl

This package provides a reference implementation and example usage of the TinyGameEngine.Core package.

## What's Included

- **FakeGame**: A sample game implementation that demonstrates how to extend `TinyEngine`
- **Program.cs**: A minimal ASP.NET Core application showing how to bootstrap the engine
- **Configuration files**: Example appsettings for development and production
- **Infrastructure**: Bicep templates and Azure YAML for deployment

## Quick Start

1. **Install the packages**:
   ```bash
   dotnet add package TinyGameEngine.Core
   # Or reference this sample project
   ```

2. **Create your game class**:
   ```csharp
   public class MyGame : TinyEngine
   {
       public MyGame(IGameStateService gameStateService, ITelemetryService telemetryService, 
                     IGameIdProvider gameIdProvider, ILogger<TinyEngine> logger) 
           : base(gameStateService, telemetryService, gameIdProvider, logger)
       {
       }

       public override string GameMode => "My Awesome Game";

       public override Task DoGameTickAsync(GameState currentState, UpdateGameRequest updateAction, 
                                          CancellationToken cancellationToken = default)
       {
           // Your game logic here
           return Task.CompletedTask;
       }
   }
   ```

3. **Configure your application**:
   ```csharp
   var builder = WebApplication.CreateBuilder(args);

   // Add TinyGameEngine Core services
   builder.Services.AddTinyGameEngine(builder.Configuration);

   // Register your game implementation
   builder.Services.AddScoped<IGameEngine, MyGame>();

   var app = builder.Build();

   // Configure the pipeline
   app.UseTinyGameEngine();

   app.Run();
   ```

## Configuration

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

## Deployment

This reference implementation includes:

- **azure.yaml**: Azure Developer CLI configuration
- **infra/**: Bicep templates for Azure Container Apps deployment
- **Dockerfile**: Container configuration

To deploy:

```bash
azd init
azd up
```

## API Endpoints

The engine automatically provides these endpoints:

- `POST /api/game/start` - Start a new game
- `POST /api/game/update` - Update game state
- `GET /api/game/{gameId}` - Get current game state
- `GET /api/game/{gameId}/highscores` - Get high scores
- `GET /health` - Health check

## Next Steps

1. Implement your `DoGameTickAsync` method with your game logic
2. Customize the `GameState` model if needed
3. Add your own controllers for additional endpoints
4. Configure Application Insights for telemetry
5. Deploy to Azure Container Apps for scale-to-zero hosting

For more details, see the [TinyGameEngine.Core documentation](../TinyGameEngine.Core/README.md).
