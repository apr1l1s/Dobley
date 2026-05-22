FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Dobley.Endpoints.Api/Dobley.Endpoints.Api.csproj", "Dobley.Endpoints.Api/"]
RUN dotnet restore "Dobley.Endpoints.Api/Dobley.Endpoints.Api.csproj"
COPY . .
WORKDIR "/src/Dobley.Endpoints.Api"
RUN dotnet build "./Dobley.Endpoints.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Dobley.Endpoints.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dobley.Endpoints.Api.dll"]
