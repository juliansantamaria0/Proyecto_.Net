using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;

namespace AutoTallerManager.Application.Ports.Input;

public interface IGestionarRepuestosUseCase
{
    Task<PagedResult<RepuestoDto>> GetPagedAsync(PaginationParams pagination, string? categoria = null, string? descripcion = null, int? stockMinimo = null);
    Task<RepuestoDto> GetByIdAsync(int id);
    Task<RepuestoDto> CreateAsync(CreateRepuestoDto dto, int usuarioId);
    Task<RepuestoDto> UpdateAsync(int id, UpdateRepuestoDto dto, int usuarioId);
    Task UpdateStockAsync(int id, UpdateStockDto dto, int usuarioId);
    Task DeleteAsync(int id, int usuarioId);
}
