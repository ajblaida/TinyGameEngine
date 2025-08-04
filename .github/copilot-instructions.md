<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# Tiny Game Engine - Copilot Instructions

This is an Azure-native C# game engine designed for Container Apps with the following characteristics:

## Architecture Principles

- **Stateless containers** with state persisted to Azure Blob Storage
- **Scale-to-zero** design for cost efficiency
- **Managed Identity** for secure Azure resource access
- **Periodic state sync** (max 1 per second) to avoid excessive writes
- **JSON serialization** for simple state management

## Key Design Patterns

1. **Dependency Injection**: All services are registered and injected
2. **Interface-based design**: All core services implement interfaces for testability
3. **Async/await**: All I/O operations are asynchronous
4. **Cancellation tokens**: All async methods accept cancellation tokens
5. **Logging**: Structured logging with ILogger throughout
6. **Telemetry**: Application Insights integration for events and metrics

## Code Style Guidelines

- Use **nullable reference types** and handle null cases appropriately
- **Prefer records** for DTOs and request/response objects
- **Use System.Text.Json** for serialization with camelCase naming
- **Include XML documentation** for public APIs
- **Handle exceptions gracefully** and log to telemetry
- **Use meaningful variable names** that indicate intent

## Azure Integration

- **BlobServiceClient** for storage operations
- **TelemetryClient** for Application Insights
- **Managed Identity** authentication in production
- **Health checks** for service dependencies
- **CORS** enabled for web client access

## Testing Considerations

- Services should be **mockable via interfaces**
- **Azurite** for local Blob Storage testing
- **Unit tests** for business logic
- **Integration tests** for end-to-end scenarios

## Security

- **No secrets in code** - use configuration and managed identity
- **Least privilege** RBAC permissions
- **Input validation** on all API endpoints
- **HTTPS only** in production

When making changes:
1. Maintain the async/await pattern throughout
2. Add appropriate logging and telemetry
3. Update interfaces if adding new service methods
4. Consider the impact on state synchronization
5. Test with Azurite for storage operations
