using AutoTallerManager.Domain.Common;

namespace AutoTallerManager.Domain.Entities;

public class Cliente : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;

    public ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
