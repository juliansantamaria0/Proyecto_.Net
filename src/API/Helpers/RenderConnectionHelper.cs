using Npgsql;

namespace AutoTallerManager.API.Helpers;

public static class RenderConnectionHelper
{
    public static void Apply(ConfigurationManager configuration)
    {
        var provider = configuration.GetValue<string>("DatabaseProvider") ?? "PostgreSQL";
        if (!provider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
            return;

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["DATABASE_URL"];

        if (string.IsNullOrWhiteSpace(connectionString))
            return;

        configuration["ConnectionStrings:DefaultConnection"] = NormalizePostgres(connectionString);
    }

    private static string NormalizePostgres(string connectionString)
    {
        if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
            connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                SslMode = SslMode.Require
            };
            return builder.ConnectionString;
        }

        var csb = new NpgsqlConnectionStringBuilder(connectionString)
        {
            SslMode = SslMode.Require
        };
        return csb.ConnectionString;
    }
}
