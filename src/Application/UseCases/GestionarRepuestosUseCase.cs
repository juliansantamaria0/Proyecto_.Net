using AutoMapper;
using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using AutoTallerManager.Application.Ports.Output;
using AutoTallerManager.Domain.Entities;
using AutoTallerManager.Domain.Enums;
using AutoTallerManager.Domain.Exceptions;
using AutoTallerManager.Domain.Ports.Output;

namespace AutoTallerManager.Application.UseCases;

public class GestionarRepuestosUseCase(IUnitOfWork unitOfWork, IMapper mapper, IAuditoriaRegistroPort auditoriaRegistro) : IGestionarRepuestosUseCase
{
    public async Task<PagedResult<RepuestoDto>> GetPagedAsync(PaginationParams pagination, string? categoria = null, string? descripcion = null, int? stockMinimo = null)
    {
        var (items, total) = await unitOfWork.Repuestos.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            r => (categoria == null || r.Categoria.Contains(categoria)) &&
                 (descripcion == null || r.Descripcion.Contains(descripcion)) &&
                 (stockMinimo == null || r.CantidadStock <= r.StockMinimo),
            r => r.Descripcion);

        return new PagedResult<RepuestoDto>
        {
            Items = mapper.Map<IReadOnlyList<RepuestoDto>>(items),
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<RepuestoDto> GetByIdAsync(int id)
    {
        var repuesto = await unitOfWork.Repuestos.GetByIdAsync(id)
            ?? throw new NotFoundException("Repuesto", id);
        return mapper.Map<RepuestoDto>(repuesto);
    }

    public async Task<RepuestoDto> CreateAsync(CreateRepuestoDto dto, int usuarioId)
    {
        if (await unitOfWork.Repuestos.ExistsAsync(r => r.Codigo == dto.Codigo))
            throw new BusinessRuleException($"El código de repuesto {dto.Codigo} ya existe.");

        var repuesto = mapper.Map<Repuesto>(dto);
        await unitOfWork.Repuestos.AddAsync(repuesto);
        await unitOfWork.CommitAsync();
        await auditoriaRegistro.RegistrarAsync(nameof(Repuesto), repuesto.Id, TipoAccionAuditoria.Crear, usuarioId);
        await unitOfWork.CommitAsync();
        return mapper.Map<RepuestoDto>(repuesto);
    }

    public async Task<RepuestoDto> UpdateAsync(int id, UpdateRepuestoDto dto, int usuarioId)
    {
        var repuesto = await unitOfWork.Repuestos.GetByIdAsync(id)
            ?? throw new NotFoundException("Repuesto", id);

        mapper.Map(dto, repuesto);
        repuesto.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Repuestos.Update(repuesto);
        await auditoriaRegistro.RegistrarAsync(nameof(Repuesto), id, TipoAccionAuditoria.Modificar, usuarioId);
        await unitOfWork.CommitAsync();
        return mapper.Map<RepuestoDto>(repuesto);
    }

    public async Task UpdateStockAsync(int id, UpdateStockDto dto, int usuarioId)
    {
        var repuesto = await unitOfWork.Repuestos.GetByIdAsync(id)
            ?? throw new NotFoundException("Repuesto", id);

        repuesto.CantidadStock = dto.CantidadStock;
        repuesto.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Repuestos.Update(repuesto);
        await auditoriaRegistro.RegistrarAsync(nameof(Repuesto), id, TipoAccionAuditoria.Modificar, usuarioId,
            $"Stock actualizado a {dto.CantidadStock}.");
        await unitOfWork.CommitAsync();
    }

    public async Task DeleteAsync(int id, int usuarioId)
    {
        var repuesto = await unitOfWork.Repuestos.GetByIdAsync(id)
            ?? throw new NotFoundException("Repuesto", id);

        repuesto.Activo = false;
        repuesto.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Repuestos.Update(repuesto);
        await auditoriaRegistro.RegistrarAsync(nameof(Repuesto), id, TipoAccionAuditoria.Eliminar, usuarioId, "Repuesto dado de baja.");
        await unitOfWork.CommitAsync();
    }
}
