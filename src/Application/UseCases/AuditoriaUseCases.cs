using AutoMapper;
using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using AutoTallerManager.Application.Ports.Output;
using AutoTallerManager.Domain.Entities;
using AutoTallerManager.Domain.Enums;
using AutoTallerManager.Domain.Ports.Output;

namespace AutoTallerManager.Application.UseCases;

public class AuditoriaRegistroService(IUnitOfWork unitOfWork) : IAuditoriaRegistroPort
{
    public async Task RegistrarAsync(string entidad, int entidadId, TipoAccionAuditoria accion, int usuarioId, string? detalle = null)
    {
        await unitOfWork.Auditorias.AddAsync(new Auditoria
        {
            Entidad = entidad,
            EntidadId = entidadId,
            TipoAccion = accion,
            UsuarioId = usuarioId,
            Detalle = detalle,
            FechaAccion = DateTime.UtcNow
        });
    }
}

public class ConsultarAuditoriasUseCase(IUnitOfWork unitOfWork, IMapper mapper) : IConsultarAuditoriasUseCase
{
    public async Task<PagedResult<AuditoriaDto>> GetPagedAsync(PaginationParams pagination, string? entidad = null, int? usuarioId = null)
    {
        var (items, total) = await unitOfWork.Auditorias.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            a => (entidad == null || a.Entidad == entidad) && (usuarioId == null || a.UsuarioId == usuarioId),
            a => a.FechaAccion,
            descending: true,
            a => a.Usuario);

        return new PagedResult<AuditoriaDto>
        {
            Items = mapper.Map<IReadOnlyList<AuditoriaDto>>(items),
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }
}
