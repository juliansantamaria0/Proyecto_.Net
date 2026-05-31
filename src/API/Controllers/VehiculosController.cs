using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoTallerManager.API.Controllers;

[Authorize]
public class VehiculosController(IGestionarVehiculosUseCase gestionarVehiculosUseCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Recepcionista,Mecanico,Cliente")]
    public async Task<ActionResult<IReadOnlyList<VehiculoDto>>> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] int? clienteId = null,
        [FromQuery] string? vin = null)
    {
        var result = await gestionarVehiculosUseCase.GetPagedAsync(pagination, ResolveClienteIdFilter(clienteId), vin);
        AddPaginationHeader(result);
        return Ok(result.Items);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Recepcionista,Mecanico,Cliente")]
    public async Task<ActionResult<VehiculoDto>> GetById(int id)
    {
        var vehiculo = await gestionarVehiculosUseCase.GetByIdAsync(id);
        var forbidden = ForbidIfNotOwnCliente(vehiculo.ClienteId);
        if (forbidden != null) return forbidden;
        return Ok(vehiculo);
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recepcionista,Cliente")]
    public async Task<ActionResult<VehiculoDto>> Create([FromBody] CreateVehiculoDto dto)
    {
        if (IsClienteRole())
            dto.ClienteId = GetCurrentClienteId();

        var result = await gestionarVehiculosUseCase.CreateAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<ActionResult<VehiculoDto>> Update(int id, [FromBody] UpdateVehiculoDto dto)
    {
        return Ok(await gestionarVehiculosUseCase.UpdateAsync(id, dto, GetCurrentUserId()));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await gestionarVehiculosUseCase.DeleteAsync(id, GetCurrentUserId());
        return NoContent();
    }
}
