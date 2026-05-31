using AutoTallerManager.Application.Ports.Input;
using AutoTallerManager.Application.Ports.Output;
using AutoTallerManager.Application.Mappings;
using AutoTallerManager.Application.UseCases;
using Microsoft.Extensions.DependencyInjection;

namespace AutoTallerManager.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddAutoMapper(typeof(MappingProfile));

        services.AddScoped<IAuditoriaRegistroPort, AuditoriaRegistroService>();

        services.AddScoped<IAutenticacionUseCase, AutenticacionUseCase>();
        services.AddScoped<IGestionarClientesUseCase, GestionarClientesUseCase>();
        services.AddScoped<IGestionarVehiculosUseCase, GestionarVehiculosUseCase>();
        services.AddScoped<IGestionarOrdenesServicioUseCase, GestionarOrdenesServicioUseCase>();
        services.AddScoped<IGestionarRepuestosUseCase, GestionarRepuestosUseCase>();
        services.AddScoped<IGestionarFacturasUseCase, GestionarFacturasUseCase>();
        services.AddScoped<IGestionarUsuariosUseCase, GestionarUsuariosUseCase>();
        services.AddScoped<IConsultarAuditoriasUseCase, ConsultarAuditoriasUseCase>();

        return services;
    }
}
