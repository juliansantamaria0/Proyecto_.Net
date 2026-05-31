using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using AutoTallerManager.API.Helpers;
using AutoTallerManager.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoTallerManager.API.Controllers;

[Authorize]
[Route("api/[controller]")]
public class OrdenesServicioController(IGestionarOrdenesServicioUseCase gestionarOrdenesServicioUseCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Mecanico,Recepcionista,Cliente")]
    public async Task<ActionResult<IReadOnlyList<OrdenServicioDto>>> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] EstadoOrden? estado = null,
        [FromQuery] int? mecanicoId = null,
        [FromQuery] int? clienteId = null,
        [FromQuery] DateTime? fechaDesde = null,
        [FromQuery] DateTime? fechaHasta = null)
    {
        if (IsClienteRole()) mecanicoId = null;
        else mecanicoId = ResolveMecanicoIdFilter(mecanicoId);

        var result = await gestionarOrdenesServicioUseCase.GetPagedAsync(
            pagination, estado, mecanicoId, ResolveClienteIdFilter(clienteId), fechaDesde, fechaHasta);

        var items = IsRecepcionistaRole()
            ? OrdenServicioResponseHelper.OcultarDetalleInterno(result.Items)
            : result.Items;

        AddPaginationHeader(result);
        return Ok(items);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Mecanico,Recepcionista,Cliente")]
    public async Task<ActionResult<OrdenServicioDto>> GetById(int id)
    {
        var orden = await gestionarOrdenesServicioUseCase.GetByIdAsync(id);
        var forbidden = ForbidIfNotOwnCliente(orden.ClienteId)
            ?? ForbidIfNotAssignedMecanico(orden.MecanicoId);
        if (forbidden != null) return forbidden;

        if (IsRecepcionistaRole())
            OrdenServicioResponseHelper.OcultarDetalleInterno(orden);

        return Ok(orden);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<ActionResult<OrdenServicioDto>> Create([FromBody] CreateOrdenServicioDto dto)
    {
        var result = await gestionarOrdenesServicioUseCase.CreateAsync(dto, GetCurrentUserId());
        if (IsRecepcionistaRole())
            OrdenServicioResponseHelper.OcultarDetalleInterno(result);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}/trabajo")]
    [Authorize(Roles = "Admin,Mecanico")]
    public async Task<ActionResult<OrdenServicioDto>> ActualizarTrabajo(int id, [FromBody] ActualizarOrdenTrabajoDto dto)
    {
        var orden = await gestionarOrdenesServicioUseCase.GetByIdAsync(id);
        var forbidden = ForbidIfNotAssignedMecanico(orden.MecanicoId);
        if (forbidden != null) return forbidden;

        return Ok(await gestionarOrdenesServicioUseCase.ActualizarTrabajoAsync(id, dto, GetCurrentUserId()));
    }

    [HttpPut("{id:int}/cancelar")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Cancelar(int id)
    {
        await gestionarOrdenesServicioUseCase.CancelarAsync(id, GetCurrentUserId());
        return NoContent();
    }
}
