using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;

namespace AutoTallerManager.Application.Ports.Input;

public interface IGestionarFacturasUseCase
{
    Task<PagedResult<FacturaDto>> GetPagedAsync(PaginationParams pagination, int? clienteId = null, int? ordenId = null, DateTime? fechaDesde = null);
    Task<FacturaDto> GetByIdAsync(int id);
    Task<FacturaDto> GenerarAsync(GenerarFacturaDto dto, int usuarioId);
}
