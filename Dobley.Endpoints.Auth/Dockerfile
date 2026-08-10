FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/logs /app/data-protection-keys \
    && chown -R $APP_UID /app/logs /app/data-protection-keys
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Dobley.Endpoints.Auth/Dobley.Endpoints.Auth.csproj", "Dobley.Endpoints.Auth/"]
COPY ["Dobley.Data.Core/Dobley.Data.Core.csproj", "Dobley.Data.Core/"]
COPY ["Dobley.Domain.Core/Dobley.Domain.Core.csproj", "Dobley.Domain.Core/"]
COPY ["Dobley.Observability/Dobley.Observability.csproj", "Dobley.Observability/"]
RUN dotnet restore "Dobley.Endpoints.Auth/Dobley.Endpoints.Auth.csproj"
COPY . .
WORKDIR "/src/Dobley.Endpoints.Auth"
RUN dotnet build "./Dobley.Endpoints.Auth.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Dobley.Endpoints.Auth.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dobley.Endpoints.Auth.dll"]
