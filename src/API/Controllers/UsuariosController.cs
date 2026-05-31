using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.DTOs;
using AutoTallerManager.Application.Ports.Input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoTallerManager.API.Controllers;

[Authorize(Roles = "Admin")]
public class UsuariosController(IGestionarUsuariosUseCase gestionarUsuariosUseCase) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> GetAll([FromQuery] PaginationParams pagination)
    {
        var result = await gestionarUsuariosUseCase.GetPagedAsync(pagination);
        AddPaginationHeader(result);
        return Ok(result.Items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> GetById(int id)
    {
        return Ok(await gestionarUsuariosUseCase.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Create([FromBody] CreateUsuarioDto dto)
    {
        var result = await gestionarUsuariosUseCase.CreateAsync(dto, GetCurrentUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Update(int id, [FromBody] UpdateUsuarioDto dto)
    {
        return Ok(await gestionarUsuariosUseCase.UpdateAsync(id, dto, GetCurrentUserId()));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await gestionarUsuariosUseCase.DeleteAsync(id, GetCurrentUserId());
        return NoContent();
    }
}
