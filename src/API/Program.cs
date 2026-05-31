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

var initLogger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInit");
await DbInitializer.InitializeWithRetryAsync(app.Services, initLogger);

app.UseApiPipeline();
app.MapGet("/health", () => Results.Text("OK"));

app.Logger.LogInformation(
    "AutoTallerManager listening on port {Port} | Cloud={Cloud} | Frontend={Frontend}",
    port,
    Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT")
        ?? Environment.GetEnvironmentVariable("RENDER")
        ?? "false",
    Directory.Exists(Path.Combine(app.Environment.ContentRootPath, "frontend")) ? "ok" : "missing");

await app.RunAsync();
