# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0-bookworm-slim AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/API/AutoTallerManager.API.csproj
RUN dotnet publish src/API/AutoTallerManager.API.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-bookworm-slim AS runtime
WORKDIR /app
COPY --from=build /app/publish .
COPY --from=build /src/frontend ./frontend

ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080
ENTRYPOINT ["dotnet", "AutoTallerManager.API.dll"]
