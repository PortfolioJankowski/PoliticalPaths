FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/PoliticalPaths.ImportWorker/PoliticalPaths.ImportWorker.csproj
RUN dotnet publish src/PoliticalPaths.ImportWorker/PoliticalPaths.ImportWorker.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

# ImportWorker uses Microsoft.Extensions.Hosting and resolves the
# Microsoft.AspNetCore.App shared framework through its dependencies.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PoliticalPaths.ImportWorker.dll"]
