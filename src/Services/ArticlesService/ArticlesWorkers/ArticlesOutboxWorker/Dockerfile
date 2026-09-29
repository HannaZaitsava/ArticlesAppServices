# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
USER $APP_UID
WORKDIR /app

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /app-build

COPY ["src/Services/ArticlesService/ArticlesWorkers/ArticlesOutboxWorker/ArticlesOutboxWorker.csproj", "src/Services/ArticlesService/ArticlesWorkers/ArticlesOutboxWorker/"]

COPY ["src/Services/ArticlesService/Infrastructure/DataAccess/ArticlesService.Infrastructure.DataAccess.csproj", "src/Services/ArticlesService/Infrastructure/DataAccess/"]
COPY ["src/Services/ArticlesService/Application/ArticlesService.Application.csproj", "src/Services/ArticlesService/Application/"]
COPY ["src/Services/ArticlesService/Domain/ArticlesService.Domain.csproj", "src/Services/ArticlesService/Domain/"]

COPY ["src/BuildingBlocks/IntegrationEventLogEF/BuildingBlocks.IntegrationEventLogEF.csproj", "src/BuildingBlocks/IntegrationEventLogEF/"]
COPY ["src/BuildingBlocks/Contracts/BuildingBlocks.Contracts.csproj", "src/BuildingBlocks/Contracts/"]
COPY ["src/BuildingBlocks/Configuration/BuildingBlocks.Configuration.csproj", "src/BuildingBlocks/Configuration/"]
COPY ["src/BuildingBlocks/Logging/BuildingBlocks.Logging.csproj", "src/BuildingBlocks/Logging/"]

RUN dotnet restore "src/Services/ArticlesService/ArticlesWorkers/ArticlesOutboxWorker/ArticlesOutboxWorker.csproj"
COPY . .

RUN dotnet build "src/Services/ArticlesService/ArticlesWorkers/ArticlesOutboxWorker/ArticlesOutboxWorker.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/Services/ArticlesService/ArticlesWorkers/ArticlesOutboxWorker/ArticlesOutboxWorker.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArticlesOutboxWorker.dll"]