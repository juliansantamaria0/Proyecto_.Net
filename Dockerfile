# .NET 10 usa Ubuntu Noble (no existe bookworm-slim para v10)
# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/API/AutoTallerManager.API.csproj
RUN dotnet publish src/API/AutoTallerManager.API.csproj -c Release -o /app/publish --no-restore

# Runtime (chiseled = imagen más liviana, mejor para plan free de Render)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/frontend ./frontend

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0
ENV DOTNET_GCConserveMemory=1
ENV DOTNET_GCHeapHardLimit=450000000

EXPOSE 8080
ENTRYPOINT ["dotnet", "AutoTallerManager.API.dll"]
