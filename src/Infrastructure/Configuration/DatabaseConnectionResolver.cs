using Microsoft.Extensions.Configuration;

namespace AutoTallerManager.Infrastructure.Configuration;

/// <summary>
/// Resuelve proveedor y cadena de conexión: variables de entorno (Railway/Render) primero, appsettings después.
/// </summary>
public static class DatabaseConnectionResolver
{
    public static (string Provider, string ConnectionString) Resolve(IConfiguration configuration, bool isDevelopment)
    {
        if (isDevelopment)
            return ResolveDevelopment(configuration);

        return ResolveProduction(configuration);
    }

    private static (string Provider, string ConnectionString) ResolveDevelopment(IConfiguration configuration)
    {
        var provider = configuration["DatabaseProvider"] ?? "SQLite";
        var connectionString = GetConnectionString(configuration);

        if (!provider.Equals("SQLite", StringComparison.OrdinalIgnoreCase)
            && !provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "En desarrollo solo se admite DatabaseProvider=SQLite o PostgreSQL.");
        }

        return (provider, connectionString);
    }

    private static (string Provider, string ConnectionString) ResolveProduction(IConfiguration configuration)
    {
        var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
        if (!string.IsNullOrWhiteSpace(databaseUrl))
            return ("PostgreSQL", ConvertPostgresDatabaseUrl(databaseUrl));

        var configuredProvider = configuration["DatabaseProvider"]
            ?? Environment.GetEnvironmentVariable("DATABASE_PROVIDER")
            ?? "PostgreSQL";

        if (!configuredProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "En producción solo se admite PostgreSQL. Configure DATABASE_URL o DatabaseProvider=PostgreSQL.");
        }

        return ("PostgreSQL", GetConnectionString(configuration));
    }

    private static string GetConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "No se encontró cadena de conexión. Configure ConnectionStrings:DefaultConnection en appsettings " +
                "o las variables DATABASE_URL / ConnectionStrings__DefaultConnection en el hosting.");

        return connectionString;
    }

    /// <summary>
    /// Convierte URI postgres:// de Railway/Render al formato Npgsql.
    /// </summary>
    public static string ConvertPostgresDatabaseUrl(string databaseUrl)
    {
        var uri = new Uri(databaseUrl);
        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var database = uri.AbsolutePath.TrimStart('/');
        var port = uri.Port > 0 ? uri.Port : 5432;

        var ssl = uri.Query.Contains("sslmode=require", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Contains("railway", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Contains("render", StringComparison.OrdinalIgnoreCase);

        return $"Host={uri.Host};Port={port};Database={database};Username={username};Password={password};" +
               $"SSL Mode={(ssl ? "Require" : "Prefer")};Trust Server Certificate=true";
    }
}
