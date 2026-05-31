using AutoTallerManager.Domain.Common;
using AutoTallerManager.Domain.Enums;

namespace AutoTallerManager.Domain.Entities;

public class Auditoria : BaseEntity
{
    public string Entidad { get; set; } = string.Empty;
    public int EntidadId { get; set; }
    public TipoAccionAuditoria TipoAccion { get; set; }
    public int UsuarioId { get; set; }
    public string? Detalle { get; set; }
    public DateTime FechaAccion { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
}
