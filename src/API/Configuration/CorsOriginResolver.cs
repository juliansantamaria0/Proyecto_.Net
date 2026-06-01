namespace AutoTallerManager.API.Configuration;

/// <summary>
/// Orígenes permitidos para CORS: localhost (desarrollo) + URL de Netlify desde variables de entorno.
/// </summary>
public static class CorsOriginResolver
{
    private static readonly string[] DefaultLocalOrigins =
    [
        "http://localhost:5192",
        "https://localhost:7197",
        "http://localhost:5173",
        "http://127.0.0.1:5192",
        "http://127.0.0.1:5173",
    ];

    public static string[] GetAllowedOrigins(IConfiguration configuration)
    {
        var origins = new HashSet<string>(DefaultLocalOrigins, StringComparer.OrdinalIgnoreCase);

        AddFromEnv(origins, Environment.GetEnvironmentVariable("FRONTEND_URL"));
        AddFromEnv(origins, Environment.GetEnvironmentVariable("NETLIFY_URL"));
        AddFromEnv(origins, configuration["Cors:FrontendUrl"]);

        var extra = configuration["Cors:AllowedOrigins"];
        if (!string.IsNullOrWhiteSpace(extra))
        {
            foreach (var part in extra.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                AddFromEnv(origins, part);
        }

        var extraEnv = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
        if (!string.IsNullOrWhiteSpace(extraEnv))
        {
            foreach (var part in extraEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                AddFromEnv(origins, part);
        }

        return origins.ToArray();
    }

    private static void AddFromEnv(HashSet<string> origins, string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return;
        origins.Add(value.Trim().TrimEnd('/'));
    }
}
