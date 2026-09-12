FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/PoliticalPaths.NotificationWorker/PoliticalPaths.NotificationWorker.csproj
RUN dotnet publish src/PoliticalPaths.NotificationWorker/PoliticalPaths.NotificationWorker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PoliticalPaths.NotificationWorker.dll"]
