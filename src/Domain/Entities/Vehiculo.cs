using AutoTallerManager.Domain.Common;

namespace AutoTallerManager.Domain.Entities;

public class Vehiculo : BaseEntity
{
    public int ClienteId { get; set; }
    public string Marca { get; set; } = string.Empty;
    public string Modelo { get; set; } = string.Empty;
    public int Anio { get; set; }
    public string Vin { get; set; } = string.Empty;
    public int Kilometraje { get; set; }

    public Cliente Cliente { get; set; } = null!;
    public ICollection<OrdenServicio> OrdenesServicio { get; set; } = new List<OrdenServicio>();
}
