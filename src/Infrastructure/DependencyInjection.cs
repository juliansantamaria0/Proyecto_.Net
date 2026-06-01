using AutoTallerManager.Application.Ports.Output;
using AutoTallerManager.Domain.Ports.Output;
using AutoTallerManager.Infrastructure.Configuration;
using AutoTallerManager.Infrastructure.Persistence;
using AutoTallerManager.Infrastructure.Repositories;
using AutoTallerManager.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AutoTallerManager.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isDevelopment)
    {
        var (provider, connectionString) = DatabaseConnectionResolver.Resolve(configuration, isDevelopment);

        services.AddDbContext<AutoTallerDbContext>(options =>
        {
            switch (provider.ToUpperInvariant())
            {
                case "SQLITE":
                    options.UseSqlite(connectionString);
                    break;
                case "POSTGRESQL":
                    options.UseNpgsql(connectionString);
                    break;
                default:
                    throw new InvalidOperationException($"Proveedor de base de datos no soportado: {provider}");
            }
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IVehiculoRepository, VehiculoRepository>();
        services.AddScoped<IOrdenServicioRepository, OrdenServicioRepository>();
        services.AddScoped<IRepuestoRepository, RepuestoRepository>();
        services.AddScoped<IDetalleOrdenRepository, DetalleOrdenRepository>();
        services.AddScoped<IFacturaRepository, FacturaRepository>();
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();

        services.AddScoped<IJwtTokenProvider, JwtTokenProvider>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        return services;
    }
}
