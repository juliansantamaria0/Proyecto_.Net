using AutoTallerManager.Application.DTOs;

namespace AutoTallerManager.API.Helpers;

public static class OrdenServicioResponseHelper
{
    /// <summary>
    /// Oculta información interna de la orden para Recepcionista (sin costos, repuestos ni trabajo del mecánico).
    /// </summary>
    public static OrdenServicioDto OcultarDetalleInterno(OrdenServicioDto orden)
    {
        orden.TrabajoRealizado = null;
        orden.CostoManoObra = 0;
        orden.Detalles = [];
        orden.MecanicoNombre = null;
        orden.MecanicoId = null;
        return orden;
    }

    public static IReadOnlyList<OrdenServicioDto> OcultarDetalleInterno(IReadOnlyList<OrdenServicioDto> ordenes) =>
        ordenes.Select(OcultarDetalleInterno).ToList();
}
