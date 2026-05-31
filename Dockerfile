# .NET 10 — imágenes Ubuntu Noble (bookworm-slim no existe en v10)
# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/API/AutoTallerManager.API.csproj
RUN dotnet publish src/API/AutoTallerManager.API.csproj -c Release -o /app/publish --no-restore

# Runtime: noble estándar (chiseled puede provocar exit 139 en plan free)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/frontend ./frontend

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_GCConserveMemory=9
ENV MALLOC_ARENA_MAX=2

EXPOSE 8080
ENTRYPOINT ["dotnet", "AutoTallerManager.API.dll"]
