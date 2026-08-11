FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app
RUN mkdir -p /app/logs /app/data-protection-keys \
    && chown -R $APP_UID /app/logs /app/data-protection-keys
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Dobley.Workers.Notifications/Dobley.Workers.Notifications.csproj", "Dobley.Workers.Notifications/"]
COPY ["Dobley.Data.Core/Dobley.Data.Core.csproj", "Dobley.Data.Core/"]
COPY ["Dobley.Domain.Core/Dobley.Domain.Core.csproj", "Dobley.Domain.Core/"]
RUN dotnet restore "Dobley.Workers.Notifications/Dobley.Workers.Notifications.csproj"
COPY . .
WORKDIR "/src/Dobley.Workers.Notifications"
RUN dotnet build "./Dobley.Workers.Notifications.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Dobley.Workers.Notifications.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dobley.Workers.Notifications.dll"]
