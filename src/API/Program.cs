using AutoTallerManager.API.Extensions;
using AutoTallerManager.Application;
using AutoTallerManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(port))
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddApiInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseApiPipeline();
await app.InitializeDatabaseAsync();

app.Run();
