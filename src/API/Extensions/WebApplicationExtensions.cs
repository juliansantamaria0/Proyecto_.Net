using AspNetCoreRateLimit;
using AutoTallerManager.API.Middleware;
using AutoTallerManager.Infrastructure.Seed;
using Microsoft.Extensions.FileProviders;

namespace AutoTallerManager.API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "AutoTallerManager v1");
                options.DocumentTitle = "AutoTallerManager - Swagger";
            });
        }

        app.UseIpRateLimiting();

        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        app.UseCors("AllowAll");
        app.UseStaticFrontend();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapSpaFallback();

        return app;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        await DbInitializer.InitializeAsync(app.Services);
    }

    private static void MapSpaFallback(this WebApplication app)
    {
        var frontendPath = ResolveFrontendPath(app);
        if (frontendPath is null) return;

        app.MapFallback(async context =>
        {
            if (context.Request.Path.StartsWithSegments("/api") ||
                context.Request.Path.StartsWithSegments("/swagger"))
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                return;
            }

            context.Response.ContentType = "text/html";
            context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            await context.Response.SendFileAsync(Path.Combine(frontendPath, "index.html"));
        });
    }

    private static string? ResolveFrontendPath(WebApplication app)
    {
        var frontendPath = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, "..", "..", "frontend"));
        return Directory.Exists(frontendPath) ? frontendPath : null;
    }

    private static void UseStaticFrontend(this WebApplication app)
    {
        var frontendPath = ResolveFrontendPath(app);
        if (frontendPath is null)
            return;

        var fileProvider = new PhysicalFileProvider(frontendPath);

        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = fileProvider,
            DefaultFileNames = ["index.html"]
        });
        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider,
            RequestPath = "",
            OnPrepareResponse = ctx =>
            {
                if (string.Equals(ctx.File.Name, "index.html", StringComparison.OrdinalIgnoreCase))
                    ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            }
        });
    }
}
