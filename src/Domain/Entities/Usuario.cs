using AutoTallerManager.Domain.Common;
using AutoTallerManager.Domain.Enums;

namespace AutoTallerManager.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public RolUsuario Rol { get; set; }
    public bool Activo { get; set; } = true;
    public int? ClienteId { get; set; }

    public Cliente? Cliente { get; set; }
    public ICollection<OrdenServicio> OrdenesAsignadas { get; set; } = new List<OrdenServicio>();
}
