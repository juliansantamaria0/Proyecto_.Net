using AutoTallerManager.Domain.Enums;

namespace AutoTallerManager.Application.DTOs;

public class OrdenServicioDto
{
    public int Id { get; set; }
    public int VehiculoId { get; set; }
    public int ClienteId { get; set; }
    public string VehiculoDescripcion { get; set; } = string.Empty;
    public string ClienteNombre { get; set; } = string.Empty;
    public TipoServicio TipoServicio { get; set; }
    public int? MecanicoId { get; set; }
    public string? MecanicoNombre { get; set; }
    public DateTime FechaIngreso { get; set; }
    public DateTime FechaEstimadaEntrega { get; set; }
    public EstadoOrden Estado { get; set; }
    public string? TrabajoRealizado { get; set; }
    public decimal CostoManoObra { get; set; }
    public string? Descripcion { get; set; }
    public List<DetalleOrdenDto> Detalles { get; set; } = [];
}

public class CreateOrdenServicioDto
{
    public int VehiculoId { get; set; }
    public TipoServicio TipoServicio { get; set; }
    public ComplejidadServicio Complejidad { get; set; } = ComplejidadServicio.Media;
    public int? MecanicoId { get; set; }
    public decimal CostoManoObra { get; set; }
    public string? Descripcion { get; set; }
    public List<RepuestoSolicitadoDto> Repuestos { get; set; } = [];
}

public class ActualizarOrdenTrabajoDto
{
    public EstadoOrden Estado { get; set; }
    public string? TrabajoRealizado { get; set; }
    public decimal? CostoManoObra { get; set; }
    public List<RepuestoSolicitadoDto>? RepuestosAdicionales { get; set; }
}

public class RepuestoSolicitadoDto
{
    public int RepuestoId { get; set; }
    public int Cantidad { get; set; }
}
