# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

ARG APP_UID=1000

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER ${APP_UID}
WORKDIR /app
EXPOSE 8080 
EXPOSE 8081 

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["EstateAccessManagement.sln", "."]
COPY ["src/EstateAccessManagement.API/EstateAccessManagement.API.csproj", "src/EstateAccessManagement.API/"]
COPY ["src/EstateAccessManagement.Application/EstateAccessManagement.Application.csproj", "src/EstateAccessManagement.Application/"]
COPY ["src/EstateAccessManagement.Infrastructure/EstateAccessManagement.Infrastructure.csproj", "src/EstateAccessManagement.Infrastructure/"]
COPY ["src/EstateAccessManagement.Core/EstateAccessManagement.Core.csproj", "src/EstateAccessManagement.Core/"]
COPY ["tests/EstateAccessManagement.IntegrationTests/EstateAccessManagement.IntegrationTests.csproj", "tests/EstateAccessManagement.IntegrationTests/"]
COPY ["tests/EstateAccessManagement.UnitTests/EstateAccessManagement.UnitTests.csproj", "tests/EstateAccessManagement.UnitTests/"]

RUN dotnet restore "EstateAccessManagement.sln"
COPY . .
RUN dotnet build "src/EstateAccessManagement.API/EstateAccessManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "src/EstateAccessManagement.API/EstateAccessManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode
FROM base AS final
ARG APP_UID
WORKDIR /app

USER root
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /app/logs \
    && chown -R ${APP_UID}:${APP_UID} /app/logs
USER ${APP_UID}

COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD curl -f http://localhost:80/health || exit 1 

ENTRYPOINT ["dotnet", "EstateAccessManagement.API.dll"]