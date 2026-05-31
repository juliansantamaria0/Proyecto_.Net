using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoTallerManager.API.Controllers;

[Authorize]
public class RepuestosController(IGestionarRepuestosUseCase gestionarRepuestosUseCase) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = "Admin,Mecanico,Recepcionista")]
    public async Task<ActionResult<IReadOnlyList<RepuestoDto>>> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? categoria = null,
        [FromQuery] string? descripcion = null,
        [FromQuery] int? stockMinimo = null)
    {
        var result = await gestionarRepuestosUseCase.GetPagedAsync(pagination, categoria, descripcion, stockMinimo);
        AddPaginationHeader(result);
        return Ok(result.Items);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Mecanico,Recepcionista")]
    public async Task<ActionResult<RepuestoDto>> GetById(int id)
    {
        return Ok(await gestionarRepuestosUseCase.GetByIdAsync(id));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RepuestoDto>> Create([FromBody] CreateRepuestoDto dto)
    {
        var result = await gestionarRepuestosUseCase.CreateAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<RepuestoDto>> Update(int id, [FromBody] UpdateRepuestoDto dto)
    {
        return Ok(await gestionarRepuestosUseCase.UpdateAsync(id, dto, GetCurrentUserId()));
    }

    [HttpPatch("{id:int}/stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateStock(int id, [FromBody] UpdateStockDto dto)
    {
        await gestionarRepuestosUseCase.UpdateStockAsync(id, dto, GetCurrentUserId());
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        await gestionarRepuestosUseCase.DeleteAsync(id, GetCurrentUserId());
        return NoContent();
    }
}
