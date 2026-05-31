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

public class GestionarClientesUseCase(IUnitOfWork unitOfWork, IMapper mapper, IAuditoriaRegistroPort auditoriaRegistro) : IGestionarClientesUseCase
{
    public async Task<PagedResult<ClienteDto>> GetPagedAsync(PaginationParams pagination, string? nombre = null)
    {
        var (items, total) = await unitOfWork.Clientes.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            c => nombre == null || c.Nombre.Contains(nombre),
            c => c.Nombre,
            includes: c => c.Vehiculos);

        return new PagedResult<ClienteDto>
        {
            Items = mapper.Map<IReadOnlyList<ClienteDto>>(items),
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<ClienteDto> GetByIdAsync(int id)
    {
        var cliente = await unitOfWork.Clientes.GetByIdAsync(id, c => c.Vehiculos)
            ?? throw new NotFoundException("Cliente", id);
        return mapper.Map<ClienteDto>(cliente);
    }

    public async Task<ClienteDto> CreateAsync(CreateClienteDto dto, int usuarioId)
    {
        var cliente = mapper.Map<Cliente>(dto);
        await unitOfWork.Clientes.AddAsync(cliente);
        await unitOfWork.CommitAsync();
        await auditoriaRegistro.RegistrarAsync(nameof(Cliente), cliente.Id, TipoAccionAuditoria.Crear, usuarioId);
        await unitOfWork.CommitAsync();
        return mapper.Map<ClienteDto>(cliente);
    }

    public async Task<ClienteDto> UpdateAsync(int id, UpdateClienteDto dto, int usuarioId)
    {
        var cliente = await unitOfWork.Clientes.GetByIdAsync(id)
            ?? throw new NotFoundException("Cliente", id);

        mapper.Map(dto, cliente);
        cliente.UpdatedAt = DateTime.UtcNow;
        unitOfWork.Clientes.Update(cliente);
        await auditoriaRegistro.RegistrarAsync(nameof(Cliente), id, TipoAccionAuditoria.Modificar, usuarioId);
        await unitOfWork.CommitAsync();
        return mapper.Map<ClienteDto>(cliente);
    }

    public async Task DeleteAsync(int id, int usuarioId)
    {
        var cliente = await unitOfWork.Clientes.GetByIdAsync(id, c => c.Vehiculos)
            ?? throw new NotFoundException("Cliente", id);

        foreach (var vehiculo in cliente.Vehiculos)
        {
            var ordenesActivas = await unitOfWork.OrdenesServicio.ExistsAsync(o =>
                o.VehiculoId == vehiculo.Id && OrdenServicioRules.EsEstadoActivo(o.Estado));

            if (ordenesActivas)
                throw new BusinessRuleException("No se puede eliminar el cliente porque tiene órdenes de servicio activas.");
        }

        unitOfWork.Clientes.Remove(cliente);
        await auditoriaRegistro.RegistrarAsync(nameof(Cliente), id, TipoAccionAuditoria.Eliminar, usuarioId);
        await unitOfWork.CommitAsync();
    }

    public async Task<ClienteDto> RegistrarConVehiculosAsync(RegistrarClienteConVehiculoDto dto, int usuarioId)
    {
        var cliente = mapper.Map<Cliente>(dto.Cliente);

        foreach (var vehiculoDto in dto.Vehiculos)
        {
            if (await unitOfWork.Vehiculos.ExistsAsync(v => v.Vin == vehiculoDto.Vin))
                throw new BusinessRuleException($"El VIN {vehiculoDto.Vin} ya está registrado.");

            var vehiculo = mapper.Map<Vehiculo>(vehiculoDto);
            cliente.Vehiculos.Add(vehiculo);
        }

        await unitOfWork.Clientes.AddAsync(cliente);
        await unitOfWork.CommitAsync();
        await auditoriaRegistro.RegistrarAsync(nameof(Cliente), cliente.Id, TipoAccionAuditoria.Crear, usuarioId,
            $"Cliente registrado con {dto.Vehiculos.Count} vehículo(s).");
        await unitOfWork.CommitAsync();

        return await GetByIdAsync(cliente.Id);
    }
}
