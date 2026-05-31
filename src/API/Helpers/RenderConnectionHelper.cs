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

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (IsLocalPlaceholder(connectionString))
            connectionString = null;

        connectionString ??= configuration["DATABASE_URL"]
            ?? configuration["DATABASE_PRIVATE_URL"]
            ?? configuration["POSTGRES_URL"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            if (IsCloudHost)
                throw new InvalidOperationException(
                    "DATABASE_URL no configurada. En Railway: Variables → Add Reference → PostgreSQL → DATABASE_URL.");
            return;
        }

        configuration["ConnectionStrings:DefaultConnection"] = NormalizePostgres(connectionString);
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

        var isInternal = builder.Host?.Contains("railway.internal", StringComparison.OrdinalIgnoreCase) == true
            || builder.Host?.Contains("railway.app", StringComparison.OrdinalIgnoreCase) == true;

        builder.SslMode = isInternal ? SslMode.Prefer : SslMode.Require;
        return builder.ConnectionString;
    }
}
