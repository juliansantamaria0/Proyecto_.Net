# AutoTallerManager API — despliegue Railway / Render
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore src/API/AutoTallerManager.API.csproj
RUN dotnet publish src/API/AutoTallerManager.API.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
# Railway inyecta PORT; Program.cs enlaza 0.0.0.0:$PORT (fallback 8080 en la imagen)
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AutoTallerManager.API.dll"]
