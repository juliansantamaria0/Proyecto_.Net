using AutoTallerManager.API.Extensions;
using AutoTallerManager.Application;
using AutoTallerManager.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

ApplyRenderConnectionString(builder.Configuration);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseApiPipeline();
await app.InitializeDatabaseAsync();

app.Run();

static void ApplyRenderConnectionString(ConfigurationManager configuration)
{
    if (!string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")))
        return;

    var databaseUrl = configuration["DATABASE_URL"];
    if (!string.IsNullOrWhiteSpace(databaseUrl))
        configuration["ConnectionStrings:DefaultConnection"] = databaseUrl;
}
