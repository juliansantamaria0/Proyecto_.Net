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

public class GestionarOrdenesServicioUseCase(
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IAuditoriaRegistroPort auditoriaRegistro) : IGestionarOrdenesServicioUseCase
{
    public async Task<PagedResult<OrdenServicioDto>> GetPagedAsync(
        PaginationParams pagination,
        EstadoOrden? estado = null,
        int? mecanicoId = null,
        int? clienteId = null,
        DateTime? fechaDesde = null,
        DateTime? fechaHasta = null)
    {
        var desdeUtc = DateTimeUtcHelper.AsUtcStartOfDay(fechaDesde);
        var hastaUtc = DateTimeUtcHelper.AsUtcEndOfDay(fechaHasta);

        var (items, total) = await unitOfWork.OrdenesServicio.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            o => (estado == null || o.Estado == estado) &&
                 (mecanicoId == null || o.MecanicoId == mecanicoId) &&
                 (clienteId == null || o.Vehiculo.ClienteId == clienteId) &&
                 (desdeUtc == null || o.FechaIngreso >= desdeUtc) &&
                 (hastaUtc == null || o.FechaIngreso <= hastaUtc),
            o => o.FechaIngreso,
            descending: true,
            o => o.Vehiculo,
            o => o.Vehiculo.Cliente,
            o => o.Mecanico,
            o => o.Detalles);

        foreach (var orden in items)
        {
            foreach (var detalle in orden.Detalles)
                detalle.Repuesto = (await unitOfWork.Repuestos.GetByIdAsync(detalle.RepuestoId))!;
        }

        return new PagedResult<OrdenServicioDto>
        {
            Items = mapper.Map<IReadOnlyList<OrdenServicioDto>>(items),
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<OrdenServicioDto> GetByIdAsync(int id)
    {
        var orden = await unitOfWork.OrdenesServicio.GetByIdAsync(id,
            o => o.Vehiculo,
            o => o.Vehiculo.Cliente,
            o => o.Mecanico,
            o => o.Detalles)
            ?? throw new NotFoundException("Orden de servicio", id);

        foreach (var detalle in orden.Detalles)
            detalle.Repuesto = (await unitOfWork.Repuestos.GetByIdAsync(detalle.RepuestoId))!;

        return mapper.Map<OrdenServicioDto>(orden);
    }

    public async Task<OrdenServicioDto> CreateAsync(CreateOrdenServicioDto dto, int usuarioId)
    {
        if (!await unitOfWork.Vehiculos.ExistsAsync(v => v.Id == dto.VehiculoId))
            throw new NotFoundException("Vehículo", dto.VehiculoId);

        if (await unitOfWork.OrdenesServicio.ExistsAsync(o =>
                o.VehiculoId == dto.VehiculoId && OrdenServicioRules.EsEstadoActivo(o.Estado)))
            throw new BusinessRuleException("El vehículo ya tiene una orden de servicio activa.");

        if (dto.MecanicoId.HasValue)
        {
            var mecanico = await unitOfWork.Usuarios.GetByIdAsync(dto.MecanicoId.Value)
                ?? throw new NotFoundException("Mecánico", dto.MecanicoId.Value);

            if (mecanico.Rol != RolUsuario.Mecanico && mecanico.Rol != RolUsuario.Admin)
                throw new BusinessRuleException("El usuario asignado no es un mecánico válido.");

            var mecanicoConOrdenActiva = await unitOfWork.OrdenesServicio.ExistsAsync(o =>
                o.MecanicoId == dto.MecanicoId.Value && OrdenServicioRules.EsEstadoActivo(o.Estado));

            OrdenServicioRules.ValidarDisponibilidadMecanico(mecanico.Activo, mecanicoConOrdenActiva);
        }

        var orden = new OrdenServicio
        {
            VehiculoId = dto.VehiculoId,
            TipoServicio = dto.TipoServicio,
            MecanicoId = dto.MecanicoId,
            FechaIngreso = DateTime.UtcNow,
            FechaEstimadaEntrega = OrdenServicioRules.CalcularFechaEstimadaEntrega(
                dto.TipoServicio, DateTime.UtcNow, dto.Complejidad),
            Estado = EstadoOrden.Pendiente,
            CostoManoObra = dto.CostoManoObra,
            Descripcion = dto.Descripcion
        };

        var pendienteEnOrden = new Dictionary<int, int>();
        foreach (var repuestoSolicitado in dto.Repuestos)
        {
            var repuesto = await unitOfWork.Repuestos.GetByIdAsync(repuestoSolicitado.RepuestoId)
                ?? throw new NotFoundException("Repuesto", repuestoSolicitado.RepuestoId);

            var comprometido = await ObtenerStockComprometidoAsync(repuesto.Id)
                + pendienteEnOrden.GetValueOrDefault(repuesto.Id, 0);
            repuesto.ValidarDisponibilidad(repuestoSolicitado.Cantidad, comprometido);
            pendienteEnOrden[repuesto.Id] = pendienteEnOrden.GetValueOrDefault(repuesto.Id, 0) + repuestoSolicitado.Cantidad;

            orden.Detalles.Add(new DetalleOrden
            {
                RepuestoId = repuesto.Id,
                Cantidad = repuestoSolicitado.Cantidad,
                CostoUnitario = repuesto.PrecioUnitario
            });
        }

        await unitOfWork.OrdenesServicio.AddAsync(orden);
        await unitOfWork.CommitAsync();
        await auditoriaRegistro.RegistrarAsync(nameof(OrdenServicio), orden.Id, TipoAccionAuditoria.Crear, usuarioId);
        await unitOfWork.CommitAsync();
        return await GetByIdAsync(orden.Id);
    }

    public async Task<OrdenServicioDto> ActualizarTrabajoAsync(int id, ActualizarOrdenTrabajoDto dto, int usuarioId)
    {
        var orden = await unitOfWork.OrdenesServicio.GetByIdAsync(id, o => o.Detalles)
            ?? throw new NotFoundException("Orden de servicio", id);

        OrdenServicioRules.ValidarTransicionEstado(orden.Estado, dto.Estado);

        var estadoAnterior = orden.Estado;
        orden.Estado = dto.Estado;
        orden.TrabajoRealizado = dto.TrabajoRealizado ?? orden.TrabajoRealizado;
        if (dto.CostoManoObra.HasValue)
            orden.CostoManoObra = dto.CostoManoObra.Value;
        orden.UpdatedAt = DateTime.UtcNow;

        if (dto.RepuestosAdicionales != null)
        {
            foreach (var repuestoSolicitado in dto.RepuestosAdicionales)
            {
                var repuesto = await unitOfWork.Repuestos.GetByIdAsync(repuestoSolicitado.RepuestoId)
                    ?? throw new NotFoundException("Repuesto", repuestoSolicitado.RepuestoId);

                var comprometido = await ObtenerStockComprometidoAsync(repuesto.Id);
                repuesto.ValidarDisponibilidad(repuestoSolicitado.Cantidad, comprometido);

                orden.Detalles.Add(new DetalleOrden
                {
                    RepuestoId = repuesto.Id,
                    Cantidad = repuestoSolicitado.Cantidad,
                    CostoUnitario = repuesto.PrecioUnitario
                });
            }
        }

        if (dto.Estado == EstadoOrden.Completada && estadoAnterior != EstadoOrden.Completada)
            await DescontarInventarioAsync(orden);

        unitOfWork.OrdenesServicio.Update(orden);
        await auditoriaRegistro.RegistrarAsync(nameof(OrdenServicio), id, TipoAccionAuditoria.Modificar, usuarioId,
            $"Estado actualizado a {dto.Estado}.");
        await unitOfWork.CommitAsync();
        return await GetByIdAsync(id);
    }

    public async Task CancelarAsync(int id, int usuarioId)
    {
        var orden = await unitOfWork.OrdenesServicio.GetByIdAsync(id)
            ?? throw new NotFoundException("Orden de servicio", id);

        if (orden.Estado == EstadoOrden.Completada)
            throw new BusinessRuleException("No se puede cancelar una orden completada.");

        orden.Estado = EstadoOrden.Cancelada;
        orden.UpdatedAt = DateTime.UtcNow;
        unitOfWork.OrdenesServicio.Update(orden);
        await auditoriaRegistro.RegistrarAsync(nameof(OrdenServicio), id, TipoAccionAuditoria.Modificar, usuarioId, "Orden cancelada.");
        await unitOfWork.CommitAsync();
    }

    private async Task<int> ObtenerStockComprometidoAsync(int repuestoId)
    {
        var ordenesActivas = await unitOfWork.OrdenesServicio.FindAsync(
            o => OrdenServicioRules.EsEstadoActivo(o.Estado),
            o => o.Detalles);

        return ordenesActivas
            .SelectMany(o => o.Detalles)
            .Where(d => d.RepuestoId == repuestoId)
            .Sum(d => d.Cantidad);
    }

    private async Task DescontarInventarioAsync(OrdenServicio orden)
    {
        foreach (var detalle in orden.Detalles)
        {
            var repuesto = await unitOfWork.Repuestos.GetByIdAsync(detalle.RepuestoId)
                ?? throw new NotFoundException("Repuesto", detalle.RepuestoId);

            repuesto.Descontar(detalle.Cantidad);
            unitOfWork.Repuestos.Update(repuesto);
        }
    }
}
