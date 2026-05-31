using Npgsql;

namespace AutoTallerManager.API.Helpers;

public static class RenderConnectionHelper
{
    public static bool IsCloudHost =>
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT")) ||
        !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RENDER"));

    public static void Apply(ConfigurationManager configuration)
    {
        ValidateJwt(configuration);

        var provider = configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";
        if (!provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            return;

        var connectionString = ResolvePostgresConnectionString(configuration);

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (IsCloudHost)
            {
                throw new InvalidOperationException(
                    "No hay connection string de PostgreSQL. En Railway → autotaller-manager → Variables: " +
                    "elimine DATABASE_PRIVATE_URL (no existe en autotaller-db) y añada " +
                    "DATABASE_URL=${{autotaller-db.DATABASE_URL}}.");
            }
            return;
        }

        configuration["ConnectionStrings:DefaultConnection"] = NormalizePostgres(connectionString);
    }

    public static string GetDatabaseHost(IConfiguration configuration)
    {
        try
        {
            var cs = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(cs)) return "not-set";
            return new NpgsqlConnectionStringBuilder(cs).Host ?? "unknown";
        }
        catch
        {
            return "invalid";
        }
    }

    private static string? ResolvePostgresConnectionString(IConfiguration configuration)
    {
        var fromConfig = configuration.GetConnectionString("DefaultConnection");
        if (!IsLocalPlaceholder(fromConfig))
            return fromConfig;

        // Railway: DATABASE_URL = red privada (*.railway.internal). No usar DATABASE_PRIVATE_URL (nombre obsoleto).
        foreach (var key in new[] { "DATABASE_URL", "DATABASE_PUBLIC_URL", "POSTGRES_URL", "DATABASE_PRIVATE_URL" })
        {
            var value = GetEnv(configuration, key);
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return BuildFromPgVars(configuration);
    }

    private static string? BuildFromPgVars(IConfiguration configuration)
    {
        var host = GetEnv(configuration, "PGHOST");
        var user = GetEnv(configuration, "PGUSER");
        var database = GetEnv(configuration, "PGDATABASE");
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(database))
            return null;

        var portText = GetEnv(configuration, "PGPORT") ?? "5432";
        if (!int.TryParse(portText, out var port))
            port = 5432;

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = host,
            Port = port,
            Username = user,
            Password = GetEnv(configuration, "PGPASSWORD") ?? "",
            Database = database,
        };
        return builder.ConnectionString;
    }

    private static string? GetEnv(IConfiguration configuration, string key)
    {
        var value = configuration[key];
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void ValidateJwt(ConfigurationManager configuration)
    {
        if (!IsCloudHost) return;

        var key = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
            throw new InvalidOperationException(
                "Jwt__Key no configurada o demasiado corta (mín. 32 caracteres). Añádala en Railway → Variables.");
    }

    private static bool IsLocalPlaceholder(string? connectionString) =>
        !string.IsNullOrWhiteSpace(connectionString) &&
        connectionString.Contains("localhost", StringComparison.OrdinalIgnoreCase);

    private static string NormalizePostgres(string connectionString)
    {
        var builder = connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
                      connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)
            ? new NpgsqlConnectionStringBuilder(connectionString)
            : new NpgsqlConnectionStringBuilder(connectionString);

        var isInternal = builder.Host?.Contains("railway.internal", StringComparison.OrdinalIgnoreCase) == true;

        builder.SslMode = isInternal ? SslMode.Disable : SslMode.Require;
        return builder.ConnectionString;
    }
}
