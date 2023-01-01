# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY src/TheHunt.Bot/*.csproj ./TheHunt.Bot/
COPY src/TheHunt.Core/*.csproj ./TheHunt.Core/
COPY src/TheHunt.Data/*.csproj ./TheHunt.Data/
COPY src/TheHunt.Sheets/*.csproj ./TheHunt.Sheets/

RUN dotnet restore --use-current-runtime TheHunt.Bot

# copy everything else and build app
COPY src/ .
RUN dotnet publish -c Release -o /app --use-current-runtime --self-contained false --no-restore TheHunt.Bot

# final stage/image
FROM mcr.microsoft.com/dotnet/runtime:7.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "TheHunt.Bot.dll"]