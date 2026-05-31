using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoTallerManager.API.Controllers;

[Authorize]
public class ClientesController(IGestionarClientesUseCase gestionarClientesUseCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Recepcionista,Mecanico")]
    public async Task<ActionResult<IReadOnlyList<ClienteDto>>> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? nombre = null)
    {
        var result = await gestionarClientesUseCase.GetPagedAsync(pagination, nombre);
        AddPaginationHeader(result);
        return Ok(result.Items);
    }

    [HttpGet("mi-perfil")]
    [Authorize(Roles = "Cliente")]
    public async Task<ActionResult<ClienteDto>> GetMiPerfil()
    {
        var clienteId = GetCurrentClienteId();
        if (clienteId <= 0) return Forbid();
        return Ok(await gestionarClientesUseCase.GetByIdAsync(clienteId));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Recepcionista,Mecanico")]
    public async Task<ActionResult<ClienteDto>> GetById(int id)
    {
        return Ok(await gestionarClientesUseCase.GetByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<ActionResult<ClienteDto>> Create([FromBody] CreateClienteDto dto)
    {
        var result = await gestionarClientesUseCase.CreateAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("registrar-con-vehiculos")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<ActionResult<ClienteDto>> RegistrarConVehiculos([FromBody] RegistrarClienteConVehiculoDto dto)
    {
        var result = await gestionarClientesUseCase.RegistrarConVehiculosAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin,Recepcionista")]
    public async Task<ActionResult<ClienteDto>> Update(int id, [FromBody] UpdateClienteDto dto)
    {
        return Ok(await gestionarClientesUseCase.UpdateAsync(id, dto, GetCurrentUserId()));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await gestionarClientesUseCase.DeleteAsync(id, GetCurrentUserId());
        return NoContent();
    }
}
