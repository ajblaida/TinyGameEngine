# Use the official .NET runtime as a parent image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/TinyGameEngine.Core/TinyGameEngine.Core.csproj", "src/TinyGameEngine.Core/"]
COPY ["src/TinyGameEngine.ReferenceImpl/TinyGameEngine.ReferenceImpl.csproj", "src/TinyGameEngine.ReferenceImpl/"]

# Restore dependencies
RUN dotnet restore "src/TinyGameEngine.ReferenceImpl/TinyGameEngine.ReferenceImpl.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/src/TinyGameEngine.ReferenceImpl"
RUN dotnet build "TinyGameEngine.ReferenceImpl.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TinyGameEngine.ReferenceImpl.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

ENTRYPOINT ["dotnet", "TinyGameEngine.ReferenceImpl.dll"]
