FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Dobley.Endpoints.Auth/Dobley.Endpoints.Auth.csproj", "Dobley.Endpoints.Auth/"]
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
