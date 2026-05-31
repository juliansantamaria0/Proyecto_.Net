using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoTallerManager.API.Controllers;

[Authorize]
public class FacturasController(IGestionarFacturasUseCase gestionarFacturasUseCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Mecanico,Recepcionista,Cliente")]
    public async Task<ActionResult<IReadOnlyList<FacturaDto>>> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] int? clienteId = null,
        [FromQuery] int? ordenId = null,
        [FromQuery] DateTime? fechaDesde = null)
    {
        var result = await gestionarFacturasUseCase.GetPagedAsync(pagination, ResolveClienteIdFilter(clienteId), ordenId, fechaDesde);
        AddPaginationHeader(result);
        return Ok(result.Items);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Mecanico,Recepcionista,Cliente")]
    public async Task<ActionResult<FacturaDto>> GetById(int id)
    {
        var factura = await gestionarFacturasUseCase.GetByIdAsync(id);
        var forbidden = ForbidIfNotOwnCliente(factura.ClienteId);
        if (forbidden != null) return forbidden;
        return Ok(factura);
    }

    [HttpPost("generar")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<ActionResult<FacturaDto>> Generar([FromBody] GenerarFacturaDto dto)
    {
        var result = await gestionarFacturasUseCase.GenerarAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
