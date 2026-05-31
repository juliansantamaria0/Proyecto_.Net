using Npgsql;

namespace AutoTallerManager.API.Helpers;

public static class RenderConnectionHelper
{
    public static void Apply(ConfigurationManager configuration)
    {
        var provider = configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";
        if (!provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            return;

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (IsLocalPlaceholder(connectionString))
            connectionString = null;

        connectionString ??= configuration["DATABASE_URL"]
            ?? configuration["DATABASE_PRIVATE_URL"];

        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        configuration["ConnectionStrings:DefaultConnection"] = NormalizePostgres(connectionString);
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
        builder.SslMode = isInternal ? SslMode.Prefer : SslMode.Require;
        return builder.ConnectionString;
    }
}
