using AutoTallerManager.Domain.Enums;

namespace AutoTallerManager.Domain.Services;

public static class OrdenServicioRules
{
    private static readonly Dictionary<TipoServicio, int> DiasEstimados = new()
    {
        { TipoServicio.MantenimientoPreventivo, 1 },
        { TipoServicio.Reparacion, 3 },
        { TipoServicio.Diagnostico, 2 }
    };

    public static DateTime CalcularFechaEstimadaEntrega(
        TipoServicio tipoServicio,
        DateTime fechaIngreso,
        ComplejidadServicio complejidad = ComplejidadServicio.Media)
    {
        var diasBase = DiasEstimados.GetValueOrDefault(tipoServicio, 2);
        var factor = complejidad switch
        {
            ComplejidadServicio.Baja => 1.0m,
            ComplejidadServicio.Media => 1.0m,
            ComplejidadServicio.Alta => 1.5m,
            _ => 1.0m
        };
        var dias = (int)Math.Ceiling(diasBase * factor);
        return fechaIngreso.AddDays(Math.Max(1, dias));
    }

    public static bool EsEstadoActivo(EstadoOrden estado) =>
        estado is EstadoOrden.Pendiente or EstadoOrden.EnProceso;

    public static void ValidarTransicionEstado(EstadoOrden actual, EstadoOrden nuevo)
    {
        if (actual == EstadoOrden.Cancelada)
            throw new Exceptions.BusinessRuleException("No se puede modificar una orden cancelada.");

        if (actual == EstadoOrden.Completada && nuevo != EstadoOrden.Completada)
            throw new Exceptions.BusinessRuleException("No se puede modificar una orden completada.");
    }

    public static void ValidarDisponibilidadMecanico(bool mecanicoActivo, bool tieneOrdenActiva)
    {
        if (!mecanicoActivo)
            throw new Exceptions.BusinessRuleException("El mecánico asignado no está activo.");

        if (tieneOrdenActiva)
            throw new Exceptions.BusinessRuleException("El mecánico ya tiene una orden de servicio activa asignada.");
    }
}
