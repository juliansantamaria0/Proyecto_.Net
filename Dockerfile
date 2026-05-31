# .NET 10 — tag "10.0" (Ubuntu Noble). No usar bookworm-slim: no existe en v10.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/API/AutoTallerManager.API.csproj
RUN dotnet publish src/API/AutoTallerManager.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/frontend ./frontend

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_GCConserveMemory=9
ENV MALLOC_ARENA_MAX=2

EXPOSE 8080
ENTRYPOINT ["dotnet", "AutoTallerManager.API.dll"]
