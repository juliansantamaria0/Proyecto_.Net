using AutoTallerManager.Domain.Common;

namespace AutoTallerManager.Domain.Entities;

public class DetalleOrden : BaseEntity
{
    public int OrdenServicioId { get; set; }
    public int RepuestoId { get; set; }
    public int Cantidad { get; set; }
    public decimal CostoUnitario { get; set; }

    public OrdenServicio OrdenServicio { get; set; } = null!;
    public Repuesto Repuesto { get; set; } = null!;

    public decimal Subtotal => Cantidad * CostoUnitario;
}
