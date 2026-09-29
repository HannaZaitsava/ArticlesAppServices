FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app-build

# Copy project files for efficient caching
COPY ["src/Services/NotificationService/NotificationService.csproj", "src/Services/NotificationService/"]

COPY ["src/BuildingBlocks/Contracts/BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Configuration/BuildingBlocks.Configuration.csproj", "src/BuildingBlocks/Configuration/"]
COPY ["src/BuildingBlocks/Logging/BuildingBlocks.Logging.csproj", "src/BuildingBlocks/Logging/"]

# Restore NuGet dependencies
RUN dotnet restore "src/Services/NotificationService/NotificationService.csproj"

# Copy full source code and build
COPY . .
RUN dotnet build "src/Services/NotificationService/NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/Services/NotificationService/NotificationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final production image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.dll"]