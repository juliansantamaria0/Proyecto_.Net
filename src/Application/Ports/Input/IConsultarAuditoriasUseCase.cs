using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;

namespace AutoTallerManager.Application.Ports.Input;

public interface IConsultarAuditoriasUseCase
{
    Task<PagedResult<AuditoriaDto>> GetPagedAsync(PaginationParams pagination, string? entidad = null, int? usuarioId = null);
}
