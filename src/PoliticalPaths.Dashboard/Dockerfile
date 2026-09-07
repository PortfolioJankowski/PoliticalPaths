FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/PoliticalPaths.Dashboard/PoliticalPaths.Dashboard.csproj
RUN dotnet publish src/PoliticalPaths.Dashboard/PoliticalPaths.Dashboard.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PoliticalPaths.Dashboard.dll"]
