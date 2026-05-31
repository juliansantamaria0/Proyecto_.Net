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

public class GestionarFacturasUseCase(IUnitOfWork unitOfWork, IMapper mapper, IAuditoriaRegistroPort auditoriaRegistro) : IGestionarFacturasUseCase
{
    public async Task<PagedResult<FacturaDto>> GetPagedAsync(PaginationParams pagination, int? clienteId = null, int? ordenId = null, DateTime? fechaDesde = null)
    {
        var desdeUtc = DateTimeUtcHelper.AsUtcStartOfDay(fechaDesde);

        var (items, total) = await unitOfWork.Facturas.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            f => (ordenId == null || f.OrdenServicioId == ordenId) &&
                 (clienteId == null || f.OrdenServicio.Vehiculo.ClienteId == clienteId) &&
                 (desdeUtc == null || f.FechaEmision >= desdeUtc),
            f => f.FechaEmision,
            descending: true,
            f => f.OrdenServicio,
            f => f.OrdenServicio.Vehiculo,
            f => f.OrdenServicio.Vehiculo.Cliente);

        return new PagedResult<FacturaDto>
        {
            Items = mapper.Map<IReadOnlyList<FacturaDto>>(items),
            TotalCount = total,
            PageNumber = pagination.PageNumber,
            PageSize = pagination.PageSize
        };
    }

    public async Task<FacturaDto> GetByIdAsync(int id)
    {
        var factura = await unitOfWork.Facturas.GetByIdAsync(id,
            f => f.OrdenServicio,
            f => f.OrdenServicio.Vehiculo,
            f => f.OrdenServicio.Vehiculo.Cliente)
            ?? throw new NotFoundException("Factura", id);
        return mapper.Map<FacturaDto>(factura);
    }

    public async Task<FacturaDto> GenerarAsync(GenerarFacturaDto dto, int usuarioId)
    {
        var orden = await unitOfWork.OrdenesServicio.GetByIdAsync(dto.OrdenServicioId, o => o.Detalles, o => o.Factura)
            ?? throw new NotFoundException("Orden de servicio", dto.OrdenServicioId);

        if (orden.Estado != EstadoOrden.Completada)
            throw new BusinessRuleException("Solo se puede generar factura para órdenes completadas.");

        if (orden.Factura != null)
            throw new BusinessRuleException("La orden ya tiene una factura generada.");

        var montoRepuestos = orden.Detalles.Sum(d => d.Cantidad * d.CostoUnitario);
        var factura = new Factura
        {
            OrdenServicioId = orden.Id,
            NumeroFactura = $"FAC-{DateTime.UtcNow:yyyyMMdd}-{orden.Id:D4}",
            FechaEmision = DateTime.UtcNow,
            MontoManoObra = orden.CostoManoObra,
            MontoRepuestos = montoRepuestos,
            MontoTotal = orden.CostoManoObra + montoRepuestos
        };

        await unitOfWork.Facturas.AddAsync(factura);
        await unitOfWork.CommitAsync();
        await auditoriaRegistro.RegistrarAsync(nameof(Factura), factura.Id, TipoAccionAuditoria.Crear, usuarioId);
        await unitOfWork.CommitAsync();
        return await GetByIdAsync(factura.Id);
    }
}
