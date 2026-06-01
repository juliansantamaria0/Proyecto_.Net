using AutoTallerManager.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoTallerManager.API.Extensions;

/// <summary>
/// Rutas de estado cuando la API se despliega sin la carpeta frontend (Railway, etc.).
/// En local, si existe frontend/, la SPA sigue sirviéndose en /.
/// </summary>
public static class ApiStatusEndpoints
{
    public static void MapApiStatusEndpoints(this WebApplication app, bool frontendAvailable)
    {
        if (frontendAvailable)
            return;

        app.MapGet("/", () => Results.Json(new
        {
            status = "ok",
            service = "AutoTallerManager API",
            message = "API operativa. El frontend se sirve desde Netlify.",
            apiBase = "/api",
            health = "/health"
        })).AllowAnonymous();

        app.MapGet("/health", async (AutoTallerDbContext db, CancellationToken ct) =>
        {
            try
            {
                var canConnect = await db.Database.CanConnectAsync(ct);
                if (!canConnect)
                {
                    return Results.Json(
                        new { status = "unhealthy", database = "postgresql", detail = "No se pudo conectar." },
                        statusCode: StatusCodes.Status503ServiceUnavailable);
                }

                return Results.Json(new { status = "healthy", database = "postgresql" });
            }
            catch (Exception ex)
            {
                return Results.Json(
                    new { status = "unhealthy", database = "postgresql", detail = ex.Message },
                    statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        }).AllowAnonymous();
    }
}
