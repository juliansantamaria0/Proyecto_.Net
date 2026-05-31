using AutoTallerManager.Domain.Enums;

namespace AutoTallerManager.Application.DTOs;

public class AuditoriaDto
{
    public int Id { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public int EntidadId { get; set; }
    public TipoAccionAuditoria TipoAccion { get; set; }
    public int UsuarioId { get; set; }
    public string UsuarioNombre { get; set; } = string.Empty;
    public string? Detalle { get; set; }
    public DateTime FechaAccion { get; set; }
}
