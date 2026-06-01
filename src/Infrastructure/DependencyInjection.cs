using AutoTallerManager.Application.Ports.Output;
using AutoTallerManager.Domain.Ports.Output;
using AutoTallerManager.Infrastructure.Configuration;
using AutoTallerManager.Infrastructure.Persistence;
using AutoTallerManager.Infrastructure.Repositories;
using AutoTallerManager.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace AutoTallerManager.Infrastructure;

public static class DependencyInjection
{
    private static readonly MySqlServerVersion MySqlVersion = new(new Version(8, 0, 36));

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var (provider, connectionString) = DatabaseConnectionResolver.Resolve(configuration);

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
                case "MYSQL":
                default:
                    options.UseMySql(connectionString, MySqlVersion);
                    break;
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
