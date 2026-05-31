using AutoTallerManager.API.Extensions;
using AutoTallerManager.API.Helpers;
using AutoTallerManager.Application;
using AutoTallerManager.Infrastructure;
using AutoTallerManager.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
RenderConnectionHelper.Apply(builder.Configuration);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiInfrastructure(builder.Configuration, builder.Environment);

var app = builder.Build();

app.UseApiPipeline();
app.MapGet("/health", () => Results.Text("OK"));

app.Logger.LogInformation(
    "AutoTallerManager listening on port {Port} | RENDER={Render} | Frontend={Frontend}",
    port,
    Environment.GetEnvironmentVariable("RENDER") ?? "false",
    Directory.Exists(Path.Combine(app.Environment.ContentRootPath, "frontend")) ? "ok" : "missing");

_ = Task.Run(async () =>
{
    await Task.Delay(TimeSpan.FromSeconds(5));
    await InitializeDatabaseInBackground(app);
});

await app.RunAsync();

static async Task InitializeDatabaseInBackground(WebApplication app)
{
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInit");
    try
    {
        await DbInitializer.InitializeWithRetryAsync(app.Services, logger);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed after all retries.");
    }
}
