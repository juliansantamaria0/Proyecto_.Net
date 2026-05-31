using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;

namespace AutoTallerManager.Application.Ports.Input;

public interface IGestionarVehiculosUseCase
{
    Task<PagedResult<VehiculoDto>> GetPagedAsync(PaginationParams pagination, int? clienteId = null, string? vin = null);
    Task<VehiculoDto> GetByIdAsync(int id);
    Task<VehiculoDto> CreateAsync(CreateVehiculoDto dto, int usuarioId);
    Task<VehiculoDto> UpdateAsync(int id, UpdateVehiculoDto dto, int usuarioId);
    Task DeleteAsync(int id, int usuarioId);
}
