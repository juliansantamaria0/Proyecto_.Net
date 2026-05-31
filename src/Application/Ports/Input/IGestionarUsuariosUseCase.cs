using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;

namespace AutoTallerManager.Application.Ports.Input;

public interface IGestionarUsuariosUseCase
{
    Task<PagedResult<UsuarioDto>> GetPagedAsync(PaginationParams pagination);
    Task<UsuarioDto> GetByIdAsync(int id);
    Task<UsuarioDto> CreateAsync(CreateUsuarioDto dto, int usuarioId);
    Task<UsuarioDto> UpdateAsync(int id, UpdateUsuarioDto dto, int usuarioId);
    Task DeleteAsync(int id, int usuarioId);
}
