using AutoTallerManager.Domain.Enums;

namespace AutoTallerManager.Application.DTOs;

public class FacturaDto
{
    public int Id { get; set; }
    public int OrdenServicioId { get; set; }
    public int ClienteId { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; }
    public decimal MontoManoObra { get; set; }
    public decimal MontoRepuestos { get; set; }
    public decimal MontoTotal { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string VehiculoDescripcion { get; set; } = string.Empty;
}

public class GenerarFacturaDto
{
    public int OrdenServicioId { get; set; }
}
