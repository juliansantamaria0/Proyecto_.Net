using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;

namespace AutoTallerManager.Application.Ports.Input;

public interface IGestionarClientesUseCase
{
    Task<PagedResult<ClienteDto>> GetPagedAsync(PaginationParams pagination, string? nombre = null);
    Task<ClienteDto> GetByIdAsync(int id);
    Task<ClienteDto> CreateAsync(CreateClienteDto dto, int usuarioId);
    Task<ClienteDto> UpdateAsync(int id, UpdateClienteDto dto, int usuarioId);
    Task DeleteAsync(int id, int usuarioId);
    Task<ClienteDto> RegistrarConVehiculosAsync(RegistrarClienteConVehiculoDto dto, int usuarioId);
}
