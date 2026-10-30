FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY OpenGarden.Server/OpenGarden.Server.csproj OpenGarden.Server/
RUN dotnet restore OpenGarden.Server/OpenGarden.Server.csproj

COPY OpenGarden.Server/ OpenGarden.Server/
RUN dotnet publish OpenGarden.Server/OpenGarden.Server.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "OpenGarden.Server.dll"]
