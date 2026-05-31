using AutoTallerManager.Domain.Common;
using AutoTallerManager.Domain.Enums;

namespace AutoTallerManager.Domain.Entities;

public class OrdenServicio : BaseEntity
{
    public int VehiculoId { get; set; }
    public TipoServicio TipoServicio { get; set; }
    public int? MecanicoId { get; set; }
    public DateTime FechaIngreso { get; set; } = DateTime.UtcNow;
    public DateTime FechaEstimadaEntrega { get; set; }
    public EstadoOrden Estado { get; set; } = EstadoOrden.Pendiente;
    public string? TrabajoRealizado { get; set; }
    public decimal CostoManoObra { get; set; }
    public string? Descripcion { get; set; }

    public Vehiculo Vehiculo { get; set; } = null!;
    public Usuario? Mecanico { get; set; }
    public ICollection<DetalleOrden> Detalles { get; set; } = new List<DetalleOrden>();
    public Factura? Factura { get; set; }
}
