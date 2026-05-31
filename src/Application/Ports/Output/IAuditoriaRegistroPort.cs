using AutoTallerManager.Domain.Enums;

namespace AutoTallerManager.Application.Ports.Output;

public interface IAuditoriaRegistroPort
{
    Task RegistrarAsync(string entidad, int entidadId, TipoAccionAuditoria accion, int usuarioId, string? detalle = null);
}
