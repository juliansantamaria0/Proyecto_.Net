using AutoTallerManager.Domain.Common;

namespace AutoTallerManager.Domain.Entities;

public class Factura : BaseEntity
{
    public int OrdenServicioId { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public DateTime FechaEmision { get; set; } = DateTime.UtcNow;
    public decimal MontoManoObra { get; set; }
    public decimal MontoRepuestos { get; set; }
    public decimal MontoTotal { get; set; }

    public OrdenServicio OrdenServicio { get; set; } = null!;
}
