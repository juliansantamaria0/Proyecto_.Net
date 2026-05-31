using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Domain.Enums;

namespace AutoTallerManager.Application.Ports.Input;

public interface IGestionarOrdenesServicioUseCase
{
    Task<PagedResult<OrdenServicioDto>> GetPagedAsync(
        PaginationParams pagination,
        EstadoOrden? estado = null,
        int? mecanicoId = null,
        int? clienteId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null);
    Task<OrdenServicioDto> GetByIdAsync(int id);
    Task<OrdenServicioDto> CreateAsync(CreateOrdenServicioDto dto, int usuarioId);
    Task<OrdenServicioDto> ActualizarTrabajoAsync(int id, ActualizarOrdenTrabajoDto dto, int usuarioId);
    Task CancelarAsync(int id, int usuarioId);
}
