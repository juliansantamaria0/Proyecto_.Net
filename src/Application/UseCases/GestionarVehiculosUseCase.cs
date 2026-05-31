using AutoMapper;
using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using AutoTallerManager.Application.Ports.Output;
using AutoTallerManager.Domain.Entities;
using AutoTallerManager.Domain.Enums;
using AutoTallerManager.Domain.Exceptions;
using AutoTallerManager.Domain.Ports.Output;
using AutoTallerManager.Domain.Services;

namespace AutoTallerManager.Application.UseCases;

public class GestionarVehiculosUseCase(IUnitOfWork unitOfWork, IMapper mapper, IAuditoriaRegistroPort auditoriaRegistro) : IGestionarVehiculosUseCase
{
    public async Task<PagedResult<VehiculoDto>> GetPagedAsync(PaginationParams pagination, int? clienteId = null, string? vin = null)
    {
        var (items, total) = await unitOfWork.Vehiculos.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            v => (clienteId == null || v.ClienteId == clienteId) &&
                 (vin == null || v.Vin.Contains(vin)),
            v => v.Marca,
            includes: v => v.Cliente);

        return new PagedResult<VehiculoDto>
        {
            Items = mapper.Map<IReadOnlyList<VehiculoDto>>(items),
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<VehiculoDto> GetByIdAsync(int id)
    {
        var vehiculo = await unitOfWork.Vehiculos.GetByIdAsync(id, v => v.Cliente)
            ?? throw new NotFoundException("Vehículo", id);
        return mapper.Map<VehiculoDto>(vehiculo);
    }

    public async Task<VehiculoDto> CreateAsync(CreateVehiculoDto dto, int usuarioId)
    {
        if (!await unitOfWork.Clientes.ExistsAsync(c => c.Id == dto.ClienteId))
            throw new NotFoundException("Cliente", dto.ClienteId);

        if (await unitOfWork.Vehiculos.ExistsAsync(v => v.Vin == dto.Vin))
            throw new BusinessRuleException($"El VIN {dto.Vin} ya está registrado.");

        var vehiculo = mapper.Map<Vehiculo>(dto);
        await unitOfWork.Vehiculos.AddAsync(vehiculo);
        await unitOfWork.CommitAsync();
        await auditoriaRegistro.RegistrarAsync(nameof(Vehiculo), vehiculo.Id, TipoAccionAuditoria.Crear, usuarioId);
        await unitOfWork.CommitAsync();
        return await GetByIdAsync(vehiculo.Id);
    }

    public async Task<VehiculoDto> UpdateAsync(int id, UpdateVehiculoDto dto, int usuarioId)
    {
        var vehiculo = await unitOfWork.Vehiculos.GetByIdAsync(id)
            ?? throw new NotFoundException("Vehículo", id);

        if (await unitOfWork.Vehiculos.ExistsAsync(v => v.Vin == dto.Vin && v.Id != id))
            throw new BusinessRuleException($"El VIN {dto.Vin} ya está registrado en otro vehículo.");

        mapper.Map(dto, vehiculo);
        vehiculo.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Vehiculos.Update(vehiculo);
        await auditoriaRegistro.RegistrarAsync(nameof(Vehiculo), id, TipoAccionAuditoria.Modificar, usuarioId);
        await unitOfWork.CommitAsync();
        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(int id, int usuarioId)
    {
        var vehiculo = await unitOfWork.Vehiculos.GetByIdAsync(id)
            ?? throw new NotFoundException("Vehículo", id);

        var tieneOrdenesActivas = await unitOfWork.OrdenesServicio.ExistsAsync(o =>
            o.VehiculoId == id && OrdenServicioRules.EsEstadoActivo(o.Estado));

        if (tieneOrdenesActivas)
            throw new BusinessRuleException("No se puede eliminar el vehículo porque tiene órdenes de servicio activas.");

        unitOfWork.Vehiculos.Remove(vehiculo);
        await auditoriaRegistro.RegistrarAsync(nameof(Vehiculo), id, TipoAccionAuditoria.Eliminar, usuarioId);
        await unitOfWork.CommitAsync();
    }
}
