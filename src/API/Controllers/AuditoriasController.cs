using AutoTallerManager.Application.Common;
using AutoTallerManager.Application.Ports.Input;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoTallerManager.API.Controllers;

[Authorize(Roles = "Admin")]
public class AuditoriasController(IConsultarAuditoriasUseCase consultarAuditoriasUseCase) : ApiControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] PaginationParams pagination,
        [FromQuery] string? entidad = null,
        [FromQuery] int? usuarioId = null)
    {
        var result = await consultarAuditoriasUseCase.GetPagedAsync(pagination, entidad, usuarioId);
        AddPaginationHeader(result);
        return Ok(result.Items);
    }
}
