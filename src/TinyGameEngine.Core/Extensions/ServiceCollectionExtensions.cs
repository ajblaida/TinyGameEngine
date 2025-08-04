using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TinyGameEngine.Core.Engine.Interfaces;
using TinyGameEngine.Core.Engine.Services;

namespace TinyGameEngine.Core.Extensions;

/// <summary>
/// Extension methods for configuring TinyGameEngine services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds TinyGameEngine core services to the DI container
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">Configuration instance</param>
    /// <param name="configureOptions">Optional configuration action</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddTinyGameEngine(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<TinyGameEngineOptions>? configureOptions = null)
    {
        // Configure options
        var options = new TinyGameEngineOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // Add controllers
        services.AddControllers();
        // Add logging
        services.AddLogging();

        // Add Application Insights and configure telemetry service
        var appInsightsConnectionString = options.ApplicationInsightsConnectionString 
            ?? configuration.GetConnectionString("ApplicationInsights");
            
        if (!string.IsNullOrEmpty(appInsightsConnectionString))
        {
            services.AddApplicationInsightsTelemetry(appInsightsOptions =>
            {
                appInsightsOptions.ConnectionString = appInsightsConnectionString;
            });
            
            // Register Application Insights telemetry service when AI is configured
            services.AddScoped<ITelemetryService, ApplicationInsightsTelemetryService>();
        }
        else
        {
            // Register no-op telemetry service when AI is not configured
            services.AddScoped<ITelemetryService, NoOpTelemetryService>();
        }

        // Configure Azure services
        services.AddAzureClients(clientBuilder =>
        {
            var storageConnectionString = options.BlobStorageConnectionString
                ?? configuration.GetConnectionString("BlobStorage");

            if (!string.IsNullOrEmpty(storageConnectionString))
            {
                // Development: use connection string
                clientBuilder.AddBlobServiceClient(storageConnectionString);
            }
            else
            {
                // Production: use managed identity
                var storageAccountName = options.StorageAccountName
                    ?? configuration["Azure:StorageAccountName"];
                    
                if (!string.IsNullOrEmpty(storageAccountName))
                {
                    var storageUri = new Uri($"https://{storageAccountName}.blob.core.windows.net");
                    clientBuilder.AddBlobServiceClient(storageUri);
                    clientBuilder.UseCredential(new DefaultAzureCredential());
                }
            }
        });

        // Register game engine services
        services.AddScoped<IGameStateService, BlobGameStateService>();
        services.AddScoped<IHighScoreService, BlobHighScoreService>();
        services.AddScoped<IGameIdProvider, GameIdProvider>();

        // Add CORS for web clients
        services.AddCors(corsOptions =>
        {
            corsOptions.AddDefaultPolicy(policy =>
            {
                if (options.AllowedOrigins?.Any() == true)
                {
                    policy.WithOrigins(options.AllowedOrigins);
                }
                else
                {
                    policy.AllowAnyOrigin();
                }
                
                policy.AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Add health checks
        services.AddHealthChecks()
            .AddCheck("blob-storage", () =>
            {
                try
                {
                    var connectionString = options.BlobStorageConnectionString
                        ?? configuration.GetConnectionString("BlobStorage");
                        
                    if (string.IsNullOrEmpty(connectionString))
                    {
                        return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Degraded(
                            "No blob storage connection configured");
                    }
                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
                }
                catch
                {
                    return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(
                        "Blob storage health check failed");
                }
            });

        return services;
    }

    /// <summary>
    /// Adds a custom game engine implementation
    /// </summary>
    /// <typeparam name="TGameEngine">The game engine implementation</typeparam>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGameEngine<TGameEngine>(this IServiceCollection services)
        where TGameEngine : class, IGameEngine
    {
        services.AddScoped<IGameEngine, TGameEngine>();
        return services;
    }
}

/// <summary>
/// Extension methods for configuring TinyGameEngine application pipeline
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Configures the TinyGameEngine middleware pipeline
    /// </summary>
    /// <param name="app">The web application</param>
    /// <param name="configureOptions">Optional configuration action</param>
    /// <returns>The web application for chaining</returns>
    public static WebApplication UseTinyGameEngine(this WebApplication app, Action<TinyGameEngineAppOptions>? configureOptions = null)
    {
        var options = new TinyGameEngineAppOptions();
        configureOptions?.Invoke(options);

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseRouting();

        // Map controllers
        app.MapControllers();

        // Map health checks
        app.MapHealthChecks("/health");

        // Initialize blob containers on startup if enabled
        var serviceOptions = app.Services.GetService<TinyGameEngineOptions>();
        if (serviceOptions?.InitializeBlobContainers == true)
        {
            // Run initialization in background
            _ = Task.Run(async () => await InitializeBlobContainersAsync(app.Services));
        }

        return app;
    }

    /// <summary>
    /// Helper method to ensure blob containers exist
    /// </summary>
    private static async Task InitializeBlobContainersAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<BlobServiceClient>>();

        try
        {
            var blobServiceClient = scope.ServiceProvider.GetRequiredService<BlobServiceClient>();

            // Create containers if they don't exist
            var gameStateContainer = blobServiceClient.GetBlobContainerClient("gamestate");
            await gameStateContainer.CreateIfNotExistsAsync();

            var highScoresContainer = blobServiceClient.GetBlobContainerClient("highscores");
            await highScoresContainer.CreateIfNotExistsAsync();

            logger.LogInformation("Blob containers initialized successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize blob containers");
            // Don't fail startup for this - let the app start and handle errors gracefully
        }
    }
}

/// <summary>
/// Configuration options for TinyGameEngine application
/// </summary>
public class TinyGameEngineAppOptions
{
    /// <summary>
    /// Whether to enable Swagger UI in production
    /// </summary>
    public bool EnableSwagger { get; set; } = false;
}

/// <summary>
/// Configuration options for TinyGameEngine
/// </summary>
public class TinyGameEngineOptions
{
    /// <summary>
    /// Azure Storage account name for production (managed identity)
    /// </summary>
    public string? StorageAccountName { get; set; }

    /// <summary>
    /// Blob storage connection string for development
    /// </summary>
    public string? BlobStorageConnectionString { get; set; }

    /// <summary>
    /// Application Insights connection string
    /// </summary>
    public string? ApplicationInsightsConnectionString { get; set; }

    /// <summary>
    /// Allowed CORS origins. If null/empty, allows any origin.
    /// </summary>
    public string[]? AllowedOrigins { get; set; }

    /// <summary>
    /// Whether to initialize blob containers on startup
    /// </summary>
    public bool InitializeBlobContainers { get; set; } = true;
}
